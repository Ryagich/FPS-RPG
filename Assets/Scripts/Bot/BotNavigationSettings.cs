using UnityEngine;

namespace Bot
{
    [CreateAssetMenu(fileName = "BotNavigationSettings", menuName = "configs/Bot/Navigation")]
    public sealed class BotNavigationSettings : ScriptableObject
    {
        [field: Tooltip("Must match the agent type used to bake the scene NavMesh.")]
        [field: SerializeField] public int AgentTypeId { get; private set; }
        [field: Tooltip("Bit mask of NavMesh areas available to this bot; -1 allows every area.")]
        [field: SerializeField] public int AreaMask { get; private set; } = -1;
        [field: SerializeField, Min(0.05f)] public float RepathInterval { get; private set; } = 0.4f;
        [field: SerializeField, Min(0.05f)] public float ArrivalDistance { get; private set; } = 0.2f;
        [field: SerializeField, Min(0.05f)] public float CornerDistance { get; private set; } = 0.25f;
        [field: SerializeField, Min(0.1f)] public float SampleRadius { get; private set; } = 1.5f;
        [field: SerializeField, Min(1f)] public float TurnSpeed { get; private set; } = 180f;
        [field: SerializeField, Min(0.1f)] public float StuckTimeout { get; private set; } = 1f;
    }
}
