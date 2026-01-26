using System.Linq;
using UnityEngine;
using Weapon.Settings;

namespace Weapon.Attachments
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AttachmentsController
    {
        private readonly WeaponConfig weaponConfig;
        private readonly Transform parent;
        
        public Scope Scope = null!;
        public Grip Grip = null!;
        public Muzzle Muzzle = null!;
        public GameObject Magazine = null!;

        public AttachmentsController
            (
                WeaponConfig weaponConfig,
                Transform parent
            )
        {
            this.weaponConfig = weaponConfig;
            this.parent = parent;
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
            if (Scope)
            {
                if (Scope.ScopeCamera)
                {
                    Scope.ScopeCamera.enabled = state;
                }
            }
        }
        
        public void Clear()
        {
            if (Scope)
                Object.Destroy(Scope.gameObject);
            if (Grip)
                Object.Destroy(Grip.gameObject);
            if (Magazine)
                Object.Destroy(Magazine.gameObject);
            if (Muzzle)
                Object.Destroy(Muzzle.gameObject);
        }

        private void InstantiateMagazine(AttachmentInfo attInfo)
        {
            if (Magazine)
                Object.Destroy(Magazine.gameObject);
            Magazine = Object.Instantiate(attInfo.BaseInfo.Pref, parent);
            Magazine.transform.localPosition = attInfo.Offset;
        }

        private void InstantiateMuzzle(AttachmentInfo attInfo)
        {
            if (Muzzle)
                Object.Destroy(Muzzle.gameObject);
            Muzzle = Object.Instantiate(attInfo.BaseInfo.Pref, parent).GetComponent<Muzzle>();
            Muzzle.transform.localPosition = attInfo.Offset;
        }

        private void InstantiateGrip(AttachmentInfo attInfo)
        {
            if (Grip)
                Object.Destroy(Grip.gameObject);
            Grip = Object.Instantiate(attInfo.BaseInfo.Pref, parent).GetComponent<Grip>();
            Grip.transform.localPosition = attInfo.Offset;
        }
        
        private void InstantiateScope(AttachmentInfo attInfo)
        {
            if (Scope)
                Object.Destroy(Scope.gameObject);
            Scope = Object.Instantiate(attInfo.BaseInfo.Pref, parent).GetComponent<Scope>();
            Scope.transform.localPosition = attInfo.Offset;
        }
    }
}
