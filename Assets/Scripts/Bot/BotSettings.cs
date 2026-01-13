

using StateMachine.Graph;
using UnityEngine;

namespace Bot
{
    [CreateAssetMenu(fileName = "BotSettings", menuName = "configs/Bot/BotSettings")]
    public class BotSettings : ScriptableObject
    {
        [field: SerializeField] public StateMachineGraph StateMachineGraph {get; private set;} = null!;
    }
}