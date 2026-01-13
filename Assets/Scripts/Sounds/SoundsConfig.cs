using UnityEngine;

namespace Sounds
{
    [CreateAssetMenu(fileName = "Sounds Config", menuName = "configs/Sounds/Sounds Config")]
    public class SoundsConfig : ScriptableObject
    {
        [field: SerializeField] public AudioSource AudioSourcePrefab { get; private set; }
    }
}