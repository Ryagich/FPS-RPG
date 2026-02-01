using System.Linq;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using Weapon.Attachments;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponKickBack : ILateTickable
    {
        private readonly Transform transform;

        // динамические оффсеты
        private Vector3 positionOffset = Vector3.zero;
        private Quaternion rotationOffset = Quaternion.identity;

        private readonly WeaponConfig config;
        private KickBackSettings currentSettings = null!;
        private bool isAim;

        public WeaponKickBack
            (
                WeaponConfig config,
                Transform transform,
                Weapon weapon,
                ISubscriber<AimChangedMessage> aimChangedMessageSubscriber
            )
        {
            this.transform = transform;
            this.config = config;

            SetCurrentSettings(false);
            
            weapon.RequestRecoil += ApplyKickback;
            aimChangedMessageSubscriber.Subscribe(SetCurrentSettings);
        }

        public void LateTick()
        {
            // плавный возврат позиции
            positionOffset = Vector3.Lerp(positionOffset, Vector3.zero, currentSettings.speed * Time.deltaTime);
            transform.localPosition += positionOffset;

            // плавный возврат вращения
            rotationOffset = Quaternion.Slerp(rotationOffset, Quaternion.identity, currentSettings.rotationReturnSpeed * Time.deltaTime);
            transform.localRotation *= rotationOffset;
        }

        private void ApplyKickback(Vector2 recoil)
        {
            // позиционная отдача
            var sign = Random.value < 0.5f ? -1f : 1f;
            var scope = config.GetCurrentAttachments().First(el => el.BaseInfo.Type == AttachmentTypes.Scope);
            
            positionOffset += new Vector3(
                Random.Range(recoil.x * 0.5f, recoil.x) * currentSettings.recoilMultiplyX * sign,
                Random.Range(recoil.y * 0.5f, recoil.y) * currentSettings.recoilMultiplyY,
                currentSettings.zKickBack + (isAim ? scope.ScopesSettings.AddKickBack : 0)
            );

            // ротационная отдача
            var rotX = Random.Range(currentSettings.rotationRecoilX * 0.5f, currentSettings.rotationRecoilX);
            var rotY = Random.Range(-currentSettings.rotationRecoilY, currentSettings.rotationRecoilY);
            var recoilRot = Quaternion.Euler(-rotX, rotY, 0f);
            rotationOffset *= recoilRot;
        }
        
        private void SetCurrentSettings(AimChangedMessage msg)
        {
            SetCurrentSettings(msg.State);
        }
        
        private void SetCurrentSettings(bool newAimState)
        {
            isAim = newAimState;
            currentSettings = newAimState
                            ? config.WeaponAnimationSettings.AimKickBackSettings
                            : config.WeaponAnimationSettings.KickBackSettings;
        }
    }
}
