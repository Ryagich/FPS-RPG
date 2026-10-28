using UnityEngine;

namespace Player
{
    /// <summary>Ссылка на визуальное тело игрока. Игровая физика остаётся на корневом CharacterController.</summary>
    public sealed class PlayerAvatarView : MonoBehaviour
    {
        [SerializeField] private Animator animator = null!;

        public Animator Animator => animator != null
            ? animator
            : animator = GetComponentInChildren<Animator>(true);
    }
}
