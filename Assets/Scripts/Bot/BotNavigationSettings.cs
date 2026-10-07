using System;
using UnityEngine;

namespace Bot
{
    [Serializable]
    public sealed class BotNavigationSettings
    {
        [Tooltip("Must match the agent type used to bake the scene NavMesh.")]
        public int AgentTypeId;
        [Tooltip("Bit mask of NavMesh areas available to this bot; -1 allows every area.")]
        public int AreaMask = -1;
        public bool PlayDemo = true;
        public bool RefillDemoAmmo = true;
        [Min(0.05f)] public float RepathInterval = 0.4f;
        [Min(0.05f)] public float ArrivalDistance = 0.2f;
        [Min(0.05f)] public float CornerDistance = 0.25f;
        [Min(0.1f)] public float SampleRadius = 1.5f;
        [Min(1f)] public float TurnSpeed = 180f;
        [Min(0.1f)] public float StuckTimeout = 1f;
        [Min(0.1f)] public float DemoMoveDistance = 3f;
        [Min(0.5f)] public float DemoStepDuration = 3f;
    }
}
