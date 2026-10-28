using UnityEngine;

namespace Player
{
    /// <summary>
    /// FPS-камера следует за головой анимированного humanoid, как Camera в Kinemation demo.
    /// Поворот остаётся под контролем существующего PlayerCamera через Camera parent.
    /// </summary>
    public sealed class PlayerCameraSocket : MonoBehaviour
    {
        [field: SerializeField] public PlayerAvatarView AvatarView { get; private set; } = null!;
        [field: SerializeField] public Transform CameraPivot { get; private set; } = null!;
        [field: SerializeField] public Camera PlayerCamera { get; private set; } = null!;
        [field: SerializeField] public Transform HeadAnchor { get; private set; } = null!;
        [field: SerializeField] public Vector3 HeadLocalOffset { get; private set; } = new(0f, .112f, .05f);

        public bool UsesAvatarSocket => enabled && AvatarView != null && CameraPivot != null && PlayerCamera != null;

        private Transform head = null!;
        private float defaultPivotY;

        private void Awake()
        {
            if (!UsesAvatarSocket) return;
            head = HeadAnchor;
            if (head == null && AvatarView.Animator.isHuman)
                head = AvatarView.Animator.GetBoneTransform(HumanBodyBones.Head);
            if (head == null)
            {
                enabled = false;
                Debug.LogError("PlayerCameraSocket requires HeadAnchor for a Generic avatar.", this);
                return;
            }

            defaultPivotY = CameraPivot.localPosition.y;
            PlayerCamera.nearClipPlane = .03f;
        }

        private void LateUpdate()
        {
            if (!enabled || head == null) return;

            var pivotOffset = transform.TransformVector(Vector3.up * (CameraPivot.localPosition.y - defaultPivotY));
            var position = head.TransformPoint(HeadLocalOffset) + pivotOffset;
            var rotation = transform.rotation * CameraPivot.localRotation;
            PlayerCamera.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
