using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    [CreateAssetMenu(fileName = "InputConfig", menuName = "configs/Input/InputConfig")]
    public class InputConfig : ScriptableObject
    {
        [field: SerializeField] public InputActionReference PointerPosition { get; private set; }
        [field: SerializeField] public InputActionReference Click { get; private set; }
        [field: SerializeField] public InputActionReference RightClick { get; private set; }
        [field: SerializeField] public InputActionReference MoveInput { get; private set; }
        [field: SerializeField] public InputActionReference LookInput { get; private set; }
        [field: SerializeField] public InputActionReference SprintInput { get; private set; }
        [field: SerializeField] public InputActionReference CrouchInput { get; private set; }
        [field: SerializeField] public InputActionReference JumpInput { get; private set; }
        
    }
}   