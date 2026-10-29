using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.KAnimationCore.Runtime.Rig;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Player
{
    /// <summary>
    /// Kinemation-compatible Foot IK. It alters cached foot targets, then the framework's IkLayer solves the legs.
    /// </summary>
    public sealed class FootPlacementLayerSettings : FPSAnimatorLayerSettings
    {
        [Header("Rig")]
        public KRigElement pelvis;
        public KRigElement rightFootTarget = new KRigElement(-1, "IK RightFoot");
        public KRigElement leftFootTarget = new KRigElement(-1, "IK LeftFoot");

        [Header("Ground alignment")]
        // Player is on layer 6; excluding it prevents casts from selecting the owner's controller.
        public LayerMask groundLayers = ~(1 << 6);
        [Min(0.001f)] public float footRadius = 0.08f;
        [Min(0f)] public float footHeight = 0.06f;
        [Min(0f)] public float rayOffset = 0.55f;
        [Min(0.01f)] public float rayDistance = 2f;
        [Min(0f)] public float maxFootOffset = 1.4f;

        [Header("Stabilization")]
        [Min(0f)] public float pelvisMaxOffset = 0.35f;
        [Min(0f)] public float interpolationSpeed = 18f;

        [Header("Terrain activation")]
        [Tooltip("IK remains inactive when both feet stand on one flat surface, preserving authored locomotion.")]
        [Min(0f)] public float flatSurfaceHeightTolerance = 0.025f;
        [Range(0f, 45f)] public float flatSurfaceAngleTolerance = 1f;

        [Header("Foot contact")]
        [Tooltip("The animated foot starts leaving the ground at this local height above its contact baseline.")]
        [Min(0f)] public float plantLiftStart = 0.025f;
        [Tooltip("The animated foot is fully in swing at this local height above its contact baseline.")]
        [Min(0.001f)] public float plantLiftEnd = 0.1f;
        [Min(0f)] public float contactWeightSmoothing = 16f;

        [Header("Landing")]
        [Tooltip("During this landing state Foot IK runs a short procedural settling step instead of pinning both feet immediately.")]
        public string landingStateName = "JumpEnd";
        public bool forceFootPlacementOnLanding = true;
        [Min(0.01f)] public float landingStepDuration = 0.18f;
        [Range(0f, 1f)] public float landingLeadFootWeight = 1f;
        [Range(0f, 1f)] public float landingFollowFootWeight = 1f;

        public override IAnimationLayerJob CreateAnimationJob() => new FootPlacementLayerJob();

#if UNITY_EDITOR
        public override void OnRigUpdated()
        {
            UpdateRigElement(ref pelvis);
            UpdateRigElement(ref rightFootTarget);
            UpdateRigElement(ref leftFootTarget);
        }
#endif
    }

    public struct FootPlacementLayerJob : IAnimationJob, IAnimationLayerJob
    {
        private struct FootSample
        {
            public bool isValid;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 up;
            public float localHeight;
        }

        private struct FootPose
        {
            public bool isValid;
            public Vector3 position;
            public Quaternion rotation;
            public float surfaceHeight;
            public float surfaceAngle;
        }

        private FootPlacementLayerSettings settings;
        private LayerJobData jobData;
        private TransformStreamHandle pelvisHandle;
        private TransformStreamHandle rightFootTargetHandle;
        private TransformStreamHandle leftFootTargetHandle;
        private bool isInitialized;
        private int landingStateHash;

        // Stream samples are read in the next Update, so casts remain on the game thread.
        private FootSample rightFootSample;
        private FootSample leftFootSample;
        private FootPose rightFootPose;
        private FootPose leftFootPose;
        private float pelvisOffset;

        private FootPose smoothedRightFootPose;
        private FootPose smoothedLeftFootPose;
        private float smoothedPelvisOffset;

        private bool hasRightContactBaseline;
        private bool hasLeftContactBaseline;
        private float rightContactBaseline;
        private float leftContactBaseline;
        private float rightFootWeight;
        private float leftFootWeight;

        // Landing has its own short support phase. It prevents both targets from snapping to the
        // floor in one frame while the authored JumpEnd pose is still lowering the pelvis.
        private bool wasLanding;
        private bool landingLeadIsRight;
        private float landingStepProgress;

        public void Initialize(LayerJobData newJobData, FPSAnimatorLayerSettings newSettings)
        {
            settings = (FootPlacementLayerSettings)newSettings;
            jobData = newJobData;

            var pelvis = newJobData.rigComponent.GetRigTransform(settings.pelvis);
            var rightTarget = newJobData.rigComponent.GetRigTransform(settings.rightFootTarget);
            var leftTarget = newJobData.rigComponent.GetRigTransform(settings.leftFootTarget);
            isInitialized = pelvis != null && rightTarget != null && leftTarget != null;

            if (!isInitialized)
            {
                Debug.LogError("Foot placement layer: pelvis or foot IK target is missing from the rig.");
                return;
            }

            pelvisHandle = newJobData.animator.BindStreamTransform(pelvis);
            rightFootTargetHandle = newJobData.animator.BindStreamTransform(rightTarget);
            leftFootTargetHandle = newJobData.animator.BindStreamTransform(leftTarget);
            landingStateHash = string.IsNullOrEmpty(settings.landingStateName)
                ? 0
                : Animator.StringToHash(settings.landingStateName);
        }

        public AnimationScriptPlayable CreatePlayable(PlayableGraph graph) => AnimationScriptPlayable.Create(graph, this);
        public FPSAnimatorLayerSettings GetSettingAsset() => settings;
        public void OnLayerLinked(FPSAnimatorLayerSettings newSettings) { }
        public void UpdateEntity(FPSAnimatorEntity newEntity) { }
        public void OnPreGameThreadUpdate() { }

        public void UpdatePlayableJobData(AnimationScriptPlayable playable, float weight)
        {
            var job = playable.GetJobData<FootPlacementLayerJob>();
            rightFootSample = job.rightFootSample;
            leftFootSample = job.leftFootSample;

            job.rightFootWeight = rightFootWeight;
            job.leftFootWeight = leftFootWeight;
            job.rightFootPose = rightFootPose;
            job.leftFootPose = leftFootPose;
            job.pelvisOffset = pelvisOffset;
            job.jobData.weight = weight;
            playable.SetJobData(job);
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            if (!isInitialized)
                return;

            var root = AnimLayerJobUtility.GetTransformFromHandle(stream, jobData.rootHandle);
            var rootUp = root.rotation * Vector3.up;
            rightFootSample = CaptureFootSample(stream, rightFootTargetHandle, root.position, rootUp);
            leftFootSample = CaptureFootSample(stream, leftFootTargetHandle, root.position, rootUp);

            if (Mathf.Approximately(jobData.weight, 0f))
                return;

            ApplyFootPose(stream, rightFootTargetHandle, rightFootPose, rightFootWeight);
            ApplyFootPose(stream, leftFootTargetHandle, leftFootPose, leftFootWeight);

            if (!Mathf.Approximately(pelvisOffset, 0f))
                pelvisHandle.SetPosition(stream, pelvisHandle.GetPosition(stream) + rootUp * pelvisOffset *
                                         Mathf.Max(rightFootWeight, leftFootWeight));
        }

        private static FootSample CaptureFootSample(AnimationStream stream, TransformStreamHandle handle,
                                                    Vector3 rootPosition, Vector3 up)
        {
            var position = handle.GetPosition(stream);
            return new FootSample
            {
                isValid = true,
                position = position,
                rotation = handle.GetRotation(stream),
                up = up,
                localHeight = Vector3.Dot(position - rootPosition, up)
            };
        }

        private static void ApplyFootPose(AnimationStream stream, TransformStreamHandle handle, FootPose pose,
                                          float weight)
        {
            if (!pose.isValid)
                return;

            handle.SetPosition(stream, Vector3.Lerp(handle.GetPosition(stream), pose.position, weight));
            handle.SetRotation(stream, Quaternion.Slerp(handle.GetRotation(stream), pose.rotation, weight));
        }

        public void LateUpdate()
        {
            var nextRightPose = BuildGroundedPose(rightFootSample);
            var nextLeftPose = BuildGroundedPose(leftFootSample);
            var isLanding = IsLandingAnimation();

            var authoredRightWeight = UpdateFootPlantWeight(rightFootSample, ref hasRightContactBaseline,
                                                             ref rightContactBaseline, rightFootWeight);
            var authoredLeftWeight = UpdateFootPlantWeight(leftFootSample, ref hasLeftContactBaseline,
                                                            ref leftContactBaseline, leftFootWeight);

            if (isLanding)
            {
                UpdateLandingStep(nextRightPose, nextLeftPose, authoredRightWeight, authoredLeftWeight);
            }
            else
            {
                ResetLandingStep();
                rightFootWeight = authoredRightWeight;
                leftFootWeight = authoredLeftWeight;
            }

            if (isLanding || RequiresTerrainCorrection(nextRightPose, nextLeftPose))
            {
                smoothedRightFootPose = SmoothPose(smoothedRightFootPose, nextRightPose);
                smoothedLeftFootPose = SmoothPose(smoothedLeftFootPose, nextLeftPose);
                rightFootPose = smoothedRightFootPose;
                leftFootPose = smoothedLeftFootPose;
            }
            else
            {
                // Do not pin feet or offset the pelvis on level terrain: authored strafe animations
                // are the source of truth there.
                smoothedRightFootPose = default;
                smoothedLeftFootPose = default;
                rightFootPose = default;
                leftFootPose = default;
            }

            var desiredPelvisOffset = CalculatePelvisOffset(rightFootSample, rightFootPose,
                                                             leftFootSample, leftFootPose);
            var alpha = 1f - Mathf.Exp(-settings.interpolationSpeed * Time.deltaTime);
            smoothedPelvisOffset = Mathf.Lerp(smoothedPelvisOffset, desiredPelvisOffset, alpha);
            pelvisOffset = smoothedPelvisOffset;

        }

        private void UpdateLandingStep(FootPose rightPose, FootPose leftPose,
                                       float authoredRightWeight, float authoredLeftWeight)
        {
            if (!wasLanding)
            {
                wasLanding = true;
                landingStepProgress = 0f;

                // Prefer the foot which is already lower in the authored landing pose. If one
                // raycast missed, the valid foot is necessarily the support foot.
                landingLeadIsRight = rightPose.isValid &&
                                     (!leftPose.isValid || rightFootSample.localHeight <= leftFootSample.localHeight);
            }

            landingStepProgress = Mathf.Clamp01(landingStepProgress +
                Time.deltaTime / Mathf.Max(settings.landingStepDuration, 0.01f));
            var settle = landingStepProgress * landingStepProgress * (3f - 2f * landingStepProgress);

            var leadWeight = Mathf.Lerp(0f, settings.landingLeadFootWeight, settle);
            var followWeight = Mathf.Lerp(0f, settings.landingFollowFootWeight, settle);

            if (landingLeadIsRight)
            {
                rightFootWeight = rightPose.isValid ? Mathf.Max(authoredRightWeight, leadWeight) : authoredRightWeight;
                leftFootWeight = leftPose.isValid ? Mathf.Max(authoredLeftWeight, followWeight) : authoredLeftWeight;
                return;
            }

            leftFootWeight = leftPose.isValid ? Mathf.Max(authoredLeftWeight, leadWeight) : authoredLeftWeight;
            rightFootWeight = rightPose.isValid ? Mathf.Max(authoredRightWeight, followWeight) : authoredRightWeight;
        }

        private void ResetLandingStep()
        {
            wasLanding = false;
            landingStepProgress = 0f;
        }

        private bool IsLandingAnimation()
        {
            if (!settings.forceFootPlacementOnLanding || string.IsNullOrEmpty(settings.landingStateName))
                return false;

            var animator = jobData.animator;
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == landingStateHash)
                return true;

            return animator.IsInTransition(0) &&
                   animator.GetNextAnimatorStateInfo(0).shortNameHash == landingStateHash;
        }

        private float UpdateFootPlantWeight(FootSample sample, ref bool hasBaseline, ref float baseline,
                                            float currentWeight)
        {
            if (!sample.isValid)
                return 0f;

            if (!hasBaseline)
            {
                baseline = sample.localHeight;
                hasBaseline = true;
            }

            // The baseline follows a planted foot quickly downward and a lifted foot slowly upward.
            // It adapts to stance changes while preserving the animated lift phase of a step.
            var baselineSpeed = sample.localHeight < baseline ? 24f : 1.5f;
            baseline = Mathf.Lerp(baseline, sample.localHeight,
                                  1f - Mathf.Exp(-baselineSpeed * Time.deltaTime));

            var lift = Mathf.Max(0f, sample.localHeight - baseline);
            var targetWeight = 1f - Mathf.InverseLerp(settings.plantLiftStart, settings.plantLiftEnd, lift);
            var alpha = 1f - Mathf.Exp(-settings.contactWeightSmoothing * Time.deltaTime);
            return Mathf.Lerp(currentWeight, targetWeight, alpha);
        }

        private FootPose BuildGroundedPose(FootSample sample)
        {
            if (!sample.isValid)
                return default;

            var origin = sample.position + sample.up * settings.rayOffset;
            if (!Physics.SphereCast(origin, settings.footRadius, -sample.up, out var hit, settings.rayDistance,
                                    settings.groundLayers, QueryTriggerInteraction.Ignore))
                return default;

            var offset = Vector3.Dot(hit.point - sample.position, sample.up);
            if (Mathf.Abs(offset) > settings.maxFootOffset)
                return default;

            // On a flat surface this is identity, so the model keeps its authored foot orientation.
            var rotation = Quaternion.FromToRotation(sample.up, hit.normal) * sample.rotation;
            return new FootPose
            {
                isValid = true,
                position = hit.point + hit.normal * settings.footHeight,
                rotation = rotation,
                surfaceHeight = Vector3.Dot(hit.point, sample.up),
                surfaceAngle = Vector3.Angle(hit.normal, sample.up)
            };
        }

        private bool RequiresTerrainCorrection(FootPose rightPose, FootPose leftPose)
        {
            // With no pair of valid samples there is no reliable terrain comparison. Leave the
            // authored animation untouched; the grounding system handles actual airborne state.
            if (!rightPose.isValid || !leftPose.isValid)
                return false;

            return rightPose.surfaceAngle > settings.flatSurfaceAngleTolerance
                   || leftPose.surfaceAngle > settings.flatSurfaceAngleTolerance
                   || Mathf.Abs(rightPose.surfaceHeight - leftPose.surfaceHeight) >
                   settings.flatSurfaceHeightTolerance;
        }

        private FootPose SmoothPose(FootPose previousPose, FootPose nextPose)
        {
            if (!nextPose.isValid)
                return default;

            if (!previousPose.isValid || Mathf.Approximately(settings.interpolationSpeed, 0f))
                return nextPose;

            var alpha = 1f - Mathf.Exp(-settings.interpolationSpeed * Time.deltaTime);
            return new FootPose
            {
                isValid = true,
                position = Vector3.Lerp(previousPose.position, nextPose.position, alpha),
                rotation = Quaternion.Slerp(previousPose.rotation, nextPose.rotation, alpha)
            };
        }

        private float CalculatePelvisOffset(FootSample rightSample, FootPose rightPose,
                                            FootSample leftSample, FootPose leftPose)
        {
            var hasOffset = false;
            var lowestOffset = 0f;
            AccumulatePelvisOffset(rightSample, rightPose, rightFootWeight, ref hasOffset, ref lowestOffset);
            AccumulatePelvisOffset(leftSample, leftPose, leftFootWeight, ref hasOffset, ref lowestOffset);

            return hasOffset
                ? Mathf.Clamp(lowestOffset, -settings.pelvisMaxOffset, settings.pelvisMaxOffset)
                : 0f;
        }

        private static void AccumulatePelvisOffset(FootSample sample, FootPose pose, float footWeight,
                                                   ref bool hasOffset, ref float lowestOffset)
        {
            if (!sample.isValid || !pose.isValid || Mathf.Approximately(footWeight, 0f))
                return;

            var offset = Vector3.Dot(pose.position - sample.position, sample.up) * footWeight;
            if (!hasOffset || offset < lowestOffset)
            {
                lowestOffset = offset;
                hasOffset = true;
            }
        }

        public void ProcessRootMotion(AnimationStream stream) { }
        public void Destroy() { }
    }
}
