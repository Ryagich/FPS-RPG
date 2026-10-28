using Inventory;
using KINEMATION.FPSAnimationFramework.Runtime.Core;
using KINEMATION.FPSAnimationFramework.Runtime.Playables;
using Player;
using UnityEngine;

namespace Weapon.Melee
{
    /// <summary>
    /// Контейнерная игровая логика холодного оружия. Зона попадания подключается
    /// отдельно; этот класс отвечает за экипировку, анимацию и блокировку атаки.
    /// </summary>
    public sealed class MeleeWeapon : IItem, IPrimaryActionItem
    {
        private readonly MeleeWeaponConfig config;
        private readonly PlayerAvatarView avatarView;
        private readonly FPSAnimator fpsAnimator;
        private readonly IPlayablesController playables;

        private float primaryActionEndsAt;

        public GameObject GameObject { get; set; }
        public bool IsPrimaryActionInProgress => Time.time < primaryActionEndsAt;

        public MeleeWeapon(MeleeWeaponConfig config, GameObject gameObject, PlayerAvatarView avatarView)
        {
            this.config = config;
            GameObject = gameObject;
            this.avatarView = avatarView;

            fpsAnimator = avatarView.Animator.GetComponent<FPSAnimator>();
            playables = avatarView.Animator.GetComponent<IPlayablesController>();

            if (fpsAnimator == null || playables == null)
                throw new MissingComponentException(
                    "Player avatar requires FPSAnimator and FPSPlayablesController for melee weapons.");
        }

        public void Activate()
        {
            GameObject.SetActive(true);
            playables.UpdateAnimatorController(config.OverrideController);
            fpsAnimator.LinkAnimatorProfile(GameObject);
            if (config.EquipMotion != null)
                fpsAnimator.LinkAnimatorLayer(config.EquipMotion);
        }

        public void Disable()
        {
            if (config.UnequipMotion != null)
                fpsAnimator.LinkAnimatorLayer(config.UnequipMotion);
            GameObject.SetActive(false);
        }

        public bool TryStartPrimaryAction()
        {
            if (IsPrimaryActionInProgress || !FPSAnimationAsset.IsValid(config.AttackAnimation))
                return false;

            Play(config.AttackAnimation);
            primaryActionEndsAt = Time.time + Mathf.Max(config.AttackInterval, config.AttackAnimation.clip.length);
            return true;
        }

        public void StopPrimaryAction()
        {
            // Холодное оружие атакует только на нажатие; отпускание ЛКМ атаку не отменяет.
        }

        private void Play(FPSAnimationAsset animation)
        {
            if (FPSAnimationAsset.IsValid(animation))
                playables.PlayAnimation(animation);
        }
    }
}
