using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "CharacterAnimationConfig", menuName = "configs/Character/Animation")]
    public sealed class CharacterAnimationConfig : ScriptableObject
    {
        [field: Header("Turning transitions")]
        [field: SerializeField, Min(0f)] public float TurnVelocitySmoothingTime { get; private set; } = 0.12f;
        [field: SerializeField, Min(0f)] public float TurnBlendTime { get; private set; } = 0.18f;
        [field: SerializeField, Min(0f)] public float TurnStartSpeed { get; private set; } = 12f;
        [field: SerializeField, Min(0f)] public float TurnStopSpeed { get; private set; } = 5f;

        [field: Header("Turning playback")]
        [field: Tooltip("Degrees per second of the source clips; must match the turn blend-tree thresholds.")]
        [field: SerializeField, Min(0.01f)] public float StandingTurnSpeed { get; private set; } = 90f;
        [field: SerializeField, Min(0.01f)] public float CrouchingTurnSpeed { get; private set; } = 71.052632f;
        [field: SerializeField, Min(0.01f)] public float MinTurnPlaybackSpeed { get; private set; } = 0.2f;
        [field: SerializeField, Min(0.01f)] public float MaxTurnPlaybackSpeed { get; private set; } = 2.5f;
        [field: SerializeField, Min(0f)] public float TurnPlaybackSmoothingTime { get; private set; } = 0.12f;
    }
}
