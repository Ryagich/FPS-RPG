using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace CameraScripts.Shake
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CameraShaker : ITickable
    {
        private readonly Transform cameraTransform;

        private const float SmoothTime = 0.08f;

        private float currentZ;
        private float zVelocity;
        private float appliedZ;

        private readonly List<StepShake> stepShakes = new();
        private readonly List<NoiseShake> noiseShakes = new();

        #region structs

        private struct StepShake
        {
            public float Elapsed;
            public float Duration;
            public float Amplitude;
            public float Direction;
        }

        private struct NoiseShake
        {
            public float Elapsed;
            public float Duration;
            public float Amplitude;
            public float Frequency;
            public AnimationCurve Falloff;
            public float Seed;
        }

        #endregion

        public CameraShaker(PlayerCamera playerCamera)
        {
            cameraTransform = playerCamera.cameraParentTransform;
        }

        #region API

        /// <summary>
        /// Импульсный шейк для шагов (лево/право)
        /// </summary>
        public void AddStepShake(float duration, float amplitude, float direction)
        {
            if (duration <= 0f || amplitude <= 0f)
                return;

            stepShakes.Add(new StepShake
            {
                Elapsed = 0f,
                Duration = duration,
                Amplitude = amplitude,
                Direction = Mathf.Sign(direction)
            });
        }

        /// <summary>
        /// Шумовой шейк (recoil, взрывы, урон)
        /// </summary>
        public void AddNoiseShake(
            float duration,
            float amplitude,
            float frequency,
            AnimationCurve falloff
        )
        {
            if (duration <= 0f || amplitude <= 0f)
                return;

            noiseShakes.Add(new NoiseShake
            {
                Elapsed = 0f,
                Duration = duration,
                Amplitude = amplitude,
                Frequency = frequency,
                Falloff = falloff,
                Seed = Random.value * 1000f
            });
        }

        #endregion

        public void Tick()
        {
            var dt = Time.deltaTime;
            float targetZ = 0f;

            // === STEP SHAKES (импульсы) ===
            for (int i = stepShakes.Count - 1; i >= 0; i--)
            {
                var s = stepShakes[i];
                s.Elapsed += dt;

                if (s.Elapsed >= s.Duration)
                {
                    stepShakes.RemoveAt(i);
                    continue;
                }

                var t = s.Elapsed / s.Duration;
                var wave = Mathf.Sin(t * Mathf.PI); // 0 → 1 → 0

                targetZ += wave * s.Amplitude * s.Direction;

                stepShakes[i] = s;
            }

            // === NOISE SHAKES (recoil etc.) ===
            for (int i = noiseShakes.Count - 1; i >= 0; i--)
            {
                var n = noiseShakes[i];
                n.Elapsed += dt;

                if (n.Elapsed >= n.Duration)
                {
                    noiseShakes.RemoveAt(i);
                    continue;
                }

                var t = n.Elapsed / n.Duration;
                var damper = n.Falloff.Evaluate(t);

                var noise =
                    Mathf.PerlinNoise(
                        (Time.time + n.Seed) * n.Frequency,
                        n.Seed
                    ) * 2f - 1f;

                targetZ += noise * n.Amplitude * damper;

                noiseShakes[i] = n;
            }

            // === Smooth + Apply delta ===
            currentZ = Mathf.SmoothDamp(
                currentZ,
                targetZ,
                ref zVelocity,
                SmoothTime
            );

            var deltaZ = currentZ - appliedZ;
            if (Mathf.Abs(deltaZ) > 0.0001f)
            {
                cameraTransform.localRotation *= Quaternion.Euler(0f, 0f, deltaZ);
                appliedZ = currentZ;
            }
        }
    }
}
