using TMPro;
using UnityEngine;

namespace CanvasScripts
{
    [CreateAssetMenu(fileName = "Canvas Config", menuName = "configs/UI/Canvas Config")]
    public class CanvasConfig : ScriptableObject
    {
        [field: SerializeField] public CanvasLifetimeScope CanvasPrefab { get; private set; } = null!;
        [field: SerializeField] public TMP_Text InteractableText { get; private set; } = null!;
    }
}