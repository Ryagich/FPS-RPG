using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Ammo;
using Sounds;
using UnityEngine;
using Weapon.Attachments;

namespace Weapon.Settings
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "configs/Weapons/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; } = "Weapon ID";
        [field: SerializeField] public DamageSettings DamageSettings { get; private set; }
        [field: SerializeField] public WeaponRole Role { get; private set; }
        [field: SerializeField] public WeaponType Type { get; private set; }
        [field: SerializeField] public List<ShootingMode> Modes { get; private set; } = new();
        [field: SerializeField] public AmmoConfig AmmoConfig { get; private set; }
        [field: SerializeField] public int RPM { get; private set; } = 600; // скорострельность
        [field: SerializeField] public int MaxAmmo { get; private set; } = 30; // Как перенесу аттачменты - закину это туда
        [field: Tooltip("Временная переменная, пока не имеем анимаций")] 
        [field: SerializeField] public float ReloadingTime { get; private set; } = 2.0f;
        [field: Space]
        [Tooltip("How far the weapon can fire from the center of the screen.")] 
        [field: SerializeField] public ShakeSettings ShakeSettings { get; private set; } = null!;
        [field: Space] 
        [field: SerializeField] public RecoilSettings RecoilSettings {get; private set;} = null!;
        [field: SerializeField] public RecoilSettings AimRecoilSettings {get; private set;} = null!;
        [field: SerializeField] public WeaponAnimationSettings WeaponAnimationSettings { get; private set; } = null!;
        [field: SerializeField] public CasingProperties CasingProperties { get; private set; } = null!;
        [field: SerializeField] public SoundsConfig SoundsConfig { get; private set; } = null!;
        [field: SerializeField] public WeaponLifetimeScope WeaponPref {get; private set;}
        [field: SerializeField] public float MovementMultiply { get; private set; } = 1.0f;
        [field: SerializeField] public float ProjectileSpeed { get; private set; } = 400.0f;
        //Обязательно закидывать хотя бы один дефолтный:
        //Ствол, прицел, магазин, цевье
        [field: SerializeField] public List<AttachmentSection> AttachmentSections {get; private set;} = new();
        [field: SerializeField] public SoundConfig ShootSound {get; private set;}
        [field: SerializeField] public SoundConfig EmptyShootSound {get; private set;}

        public IEnumerable<AttachmentInfo> GetActiveAttachments() 
            => AttachmentSections.Select(attachmentSection => attachmentSection.CurrentAttachmentInfos.First());
        public IEnumerable<AttachmentInfo> GetCurrentAttachments()
            => AttachmentSections.SelectMany(attachmentSection => attachmentSection.CurrentAttachmentInfos);
        
        public AttachmentInfo GetActiveScope() => GetActiveAttachments().First(att => att.BaseInfo.isScope());
        public AttachmentInfo GetActiveGrip() => GetActiveAttachments().First(att => att.BaseInfo.isGrip());
        public AttachmentInfo GetActiveMuzzle() => GetActiveAttachments().First(att => att.BaseInfo.isMuzzle());
        public AttachmentInfo GetActiveMagazine() => GetActiveAttachments().First(att => att.BaseInfo.isMagazine());
        public RecoilSettings GetCurrentRecoilSettings(bool isAim) => isAim ? AimRecoilSettings : RecoilSettings;
    
        public float GetRPM(AttachmentInfo exception = null!)
            => RPM + AttachmentSections
                    .Select(attachmentSection => attachmentSection.CurrentAttachmentInfos.First())
                    .Where(info => info != exception)
                    .Sum(info => info.BaseInfo.RPM);
        
        public int GetMaxCapacity(AttachmentInfo exception = null!)
            => AttachmentSections
              .Select(attachmentSection => attachmentSection.CurrentAttachmentInfos.First())
              .Where(info => info != exception)
              .Sum(info => info.BaseInfo.Magazine);
        
        //В каждой секции должен быть хотя бы 1 выбранный(Открытый) аттачмент И он всегда будет - нулевым
        //25.05.25 У оружия обязаны быть все 4 секции, в каждой, обязан быть хотя бы 1 открытый аттачмент
        private void OnValidate()
        {
            if (AttachmentSections.Count() < 4)
                throw new ArgumentException($"У оружия {ID} Не хватает секций аттачментов.");
            var invalidAttachment = AttachmentSections
                                   .SelectMany(section => section.AttachmentInfos, (section, attachment) => new { section, attachment })
                                   .FirstOrDefault(x => x.attachment.BaseInfo.Type != x.section.MainInfo.Type)
                                  ?.attachment;
            if (invalidAttachment != null)
            {
                if (invalidAttachment.BaseInfo.NameKey is null)
                    throw new  ArgumentException($"Тип секции и аттачмента не совпадает. Аттачмент: {invalidAttachment.BaseInfo.ID}");
                throw new ArgumentException($"Тип секции и аттачмента не совпадает. Аттачмент: {invalidAttachment.BaseInfo.ID} | {invalidAttachment.BaseInfo.AttachmentName}");
            }
            foreach (var attachmentSection in AttachmentSections)
            {
                if (attachmentSection.AttachmentInfos.Count is 0)
                    throw new ArgumentException($"Weapon Name: {ID} | Attachment Section Name: {attachmentSection.MainInfo.name}\n" +
                                                $"Секция есть, а аттачментов нет");
            }
            foreach (var attachmentSection in AttachmentSections)
            {
                if (attachmentSection.CurrentAttachmentInfos.Count() is 0)
                {
                    attachmentSection.AddCurrentAttachments(attachmentSection.AttachmentInfos.First());
                }
            }
        }

    }
    
    public enum ShootingMode : byte
    {
        Single,
        Burst,
        Auto,
    }
    
    public enum WeaponType : byte
    {
        SMG,
        AssaultRifle,
        BattleRifle,
        MarksmanRifle,
        SniperRifle,
        LMG,
        Pistol,
    }
    
    public enum WeaponRole : byte
    {
        Primary = 1,
        Secondary = 2
    }
}