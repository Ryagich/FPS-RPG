using UnityEngine;

namespace InteractableScripts
{
    [CreateAssetMenu(fileName = "Interactable Config", menuName = "configs/Interactable/Interactable Config")]
    public class InteractableConfig : ScriptableObject
    {
        [field: SerializeField] public LayerMask InteractableMask { get; private set; }
        [field: SerializeField] public float Distance { get; private set; } = 1.8f;
    }
}