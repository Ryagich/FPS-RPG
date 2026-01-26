using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponRunBobbing : ILateTickable
    {
        public bool isRunning;

        private float runBlend;
        private float runBlendVel;
        private float runPhase;

        private readonly WeaponConfig config;
        private readonly Transform transform;

        public WeaponRunBobbing
            (
                WeaponConfig config,
                Transform transform
            )
        {
            this.config = config;
            this.transform = transform;
        }

        public void LateTick()
        {
            // 1) Блендим запуск/останов бега
            var target = isRunning ? 1f : 0f;
            runBlend = Mathf.SmoothDamp(runBlend, target, ref runBlendVel, config.WeaponAnimationSettings.RunBobbingSettings.RunTransitionTime);

            // 2) Положение для бега
            var runOffPos = config.WeaponAnimationSettings.RunBobbingSettings.RunPositionOffset * runBlend;
            var runOffRot = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(config.WeaponAnimationSettings.RunBobbingSettings.RunRotationEuler), runBlend);

            // 3) Фаза для боба
            runPhase += Time.deltaTime * config.WeaponAnimationSettings.RunBobbingSettings.RunBobFrequency;
            var phase = runPhase * 2 * Mathf.PI;

            // 4) Расчет позиционного боба
            var bobX = Mathf.Sin(phase) * config.WeaponAnimationSettings.RunBobbingSettings.RunBobPositionAmplitude * runBlend;
            var bobPos = new Vector3(bobX, 0f, 0f);

            // 5) Расчет поворотного боба (ролл)
            var rotBobZ = Mathf.Sin(phase) * config.WeaponAnimationSettings.RunBobbingSettings.RunBobRotationAmplitude * runBlend;
            var bobRot = Quaternion.Euler(0f, 0f, rotBobZ);

            // 6) Применяем аддитивно поверх текущего трансформа
            var trans = transform;
            trans.localPosition += runOffPos + bobPos;
            trans.localRotation *= runOffRot * bobRot;
        }
        public void StartRun() => isRunning = true;
        public void StopRun() => isRunning = false;
    }
}
