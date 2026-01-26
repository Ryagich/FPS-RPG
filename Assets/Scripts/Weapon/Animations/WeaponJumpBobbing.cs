using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponJumpBobbing : ILateTickable
    {
        public CharacterController characterController;
        private readonly Transform transform;

        private float jumpBlend;
        private float jumpBlendVel;
        private float fallBlend;
        private float fallBlendVel;
        
        private WeaponConfig config;
        private JumpBobbingSettings currentSettings;

        public WeaponJumpBobbing
            (
                WeaponConfig config,
                CharacterController characterController,
                Transform transform
            )
        {
            this.config = config;
            this.characterController = characterController;
            this.transform = transform;

            UseCurrentSettings(false);
        }
        
        public void LateTick()
        {
            // 1) Считываем вертикальную скорость
            var velY = characterController.velocity.y;
            // 2) Целевые значения blend для jump/fall
            var targetJump = velY > currentSettings.JumpThreshold ? 1f : 0f;
            var targetFall = velY < -currentSettings.FallThreshold ? 1f : 0f;
            // 3) Плавно обновляем blend
            jumpBlend = Mathf.SmoothDamp(jumpBlend, targetJump, ref jumpBlendVel, currentSettings.JumpTransitionTime);
            fallBlend = Mathf.SmoothDamp(fallBlend, targetFall, ref fallBlendVel, currentSettings.FallTransitionTime);
            // 4) Расчёт оффсетов позиции
            var jumpPosOff = currentSettings.JumpPositionOffset * jumpBlend;
            var fallPosOff = currentSettings.FallPositionOffset * fallBlend;
            // 5) Расчёт оффсетов ротации
            var jumpRotOff = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(currentSettings.JumpRotationEuler), jumpBlend);
            var fallRotOff = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(currentSettings.FallRotationEuler), fallBlend);

            // 6) Аддитивное применение поверх существующего трансформа
            var t = transform;
            t.localPosition += jumpPosOff + fallPosOff;
            t.localRotation *= jumpRotOff * fallRotOff;
        }

        public void UseCurrentSettings(bool isAim)
        {
            currentSettings = isAim
                            ? config.WeaponAnimationSettings.AimJumpBobbingSettings
                            : config.WeaponAnimationSettings.JumpBobbingSettings;
        }
    }
}
