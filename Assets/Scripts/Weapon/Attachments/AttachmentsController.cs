using System.Linq;
using Dependencies;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Attachments
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AttachmentsController
    {
        private readonly WeaponConfig weaponConfig;
        private readonly LifetimeScope scope;
        
        public Scope Scope = null!;
        public Grip Grip = null!;
        public Muzzle Muzzle = null!;
        public GameObject Magazine = null!;

        public AttachmentsController
            (
                WeaponConfig weaponConfig,
                WeaponScope scopeReference
            )
        {
            var scope = scopeReference.Value;
            this.weaponConfig = weaponConfig;
            this.scope = scope;
        }
        
        public void UpdateAttachments()
        {
            Clear();
            if (weaponConfig.GetActiveScope() != null!)
                InstantiateScope(weaponConfig.GetActiveScope());
            if (weaponConfig.GetActiveGrip() != null!)
                InstantiateGrip(weaponConfig.GetActiveGrip());
            if (weaponConfig.GetActiveMuzzle() != null!)
                InstantiateMuzzle(weaponConfig.GetActiveMuzzle());
            if (weaponConfig.GetActiveMagazine() != null!)
                InstantiateMagazine(weaponConfig.GetActiveMagazine());
        }

        public void ShowAttachment(string attId)
        {
            var attInfo = weaponConfig.AttachmentSections
                                      .SelectMany(section => section.AttachmentInfos)
                                      .First(a => a.BaseInfo.ID.Equals(attId));
            ShowAttachment(attInfo);
        }
        
        public void ShowAttachment(AttachmentInfo attInfo)
        {
            if (attInfo.BaseInfo.isGrip())
            {
                InstantiateGrip(attInfo);
            }
            if (attInfo.BaseInfo.isScope())
            {
                InstantiateScope(attInfo);
            }
            if (attInfo.BaseInfo.isMuzzle())
            {
                InstantiateMuzzle(attInfo);
            }
            if (attInfo.BaseInfo.isMagazine())
            {
                InstantiateMagazine(attInfo);
            }
        }

        public void ChangeScopeState(bool state)
        {
            if (Scope != null)
            {
                if (Scope.ScopeCamera)
                {
                    Scope.ScopeCamera.enabled = state;
                }
            }
        }
        
        public void Clear()
        {
            if (Scope != null)
                Object.Destroy(Scope.GameObject);
            if (Grip != null)
                Object.Destroy(Grip.GameObject);
            if (Magazine)
                Object.Destroy(Magazine.gameObject);
            if (Muzzle != null)
                Object.Destroy(Muzzle.GameObject);
            Scope = null;
            Grip = null;
            Magazine = null;
            Muzzle = null;
        }

        private void InstantiateMagazine(AttachmentInfo attInfo)
        {
            if (Magazine)
                Object.Destroy(Magazine.gameObject);
            Magazine = Object.Instantiate(attInfo.BaseInfo.Pref, scope.transform);
            Magazine.transform.localPosition = attInfo.Offset;
        }

        private void InstantiateMuzzle(AttachmentInfo attInfo)
        {
            if (Muzzle != null)
                Object.Destroy(Muzzle.GameObject);
            Muzzle = (Muzzle)scope.CreateChildFromPrefab(attInfo.BaseInfo.EntityPrefab).Instance;
            Muzzle.Transform.localPosition = attInfo.Offset;
        }

        private void InstantiateGrip(AttachmentInfo attInfo)
        {
            if (Grip != null)
                Object.Destroy(Grip.GameObject);
            Grip = (Grip)scope.CreateChildFromPrefab(attInfo.BaseInfo.EntityPrefab).Instance;
            Grip.Transform.localPosition = attInfo.Offset;
        }
        
        private void InstantiateScope(AttachmentInfo attInfo)
        {
            if (Scope != null)
                Object.Destroy(Scope.GameObject);
            Scope = (Scope)scope.CreateChildFromPrefab(attInfo.BaseInfo.EntityPrefab).Instance;
            Scope.Transform.localPosition = attInfo.Offset;
        }
    }
}
