using System;
using Localization;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

namespace Weapon.Attachments
{
    [CreateAssetMenu(fileName = "AttachmentBaseInfo", menuName = "configs/Weapons/Attachments/AttachmentBaseInfo")]
    public class AttachmentBaseInfo : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; }
        [field: SerializeField] public LocalizedString NameKey { get; private set; }
        [field: Header("То как аттачмент будет влиять на характеристики оружия")]
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public int RPM { get; private set; }              // скорострельность
        [field: SerializeField] public float Range { get; private set; }          // как долго пуля сохраняет урон.
        [field: SerializeField] public float AnimationSpeed { get; private set; } // скорость проигрывания анимаций.
        [field: SerializeField] public float Spread { get; private set; }         // разброс
        [field: SerializeField] public int Magazine { get; private set; }
        // Сам аттачмент, как объект
        [field: SerializeField] public GameObject Pref { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public AttachmentTypes Type { get; private set; }
        [field: SerializeField, ShowIf(nameof(isScope)), AllowNesting] 
        public ScopeBaseSettings ScopeSettings { get; private set; }
        [field: SerializeField, ShowIf(nameof(isMuzzle)), AllowNesting] 
        public MuzzleBaseSettings MuzzleBaseSettings { get; private set; }
        
        public string AttachmentName => NameKey.GetLocalizedStringCached();
        public bool isScope() => Type == AttachmentTypes.Scope;
        public bool isGrip() => Type == AttachmentTypes.Grip;
        public bool isMagazine() => Type == AttachmentTypes.Magazine;
        public bool isMuzzle() => Type == AttachmentTypes.Muzzle;
    }

    public enum AttachmentTypes
    {
        Scope,
        Grip,
        Magazine,
        Muzzle,
    }
    
    [Serializable]
    public class ScopeBaseSettings
    {
        [field: SerializeField] public float Zoom { get; private set; } = 1.0f;
    }
    
    [Serializable]
    public class MuzzleBaseSettings
    {
        //Shot effect какой нибудь partical    
        //shot sound
    } 
}
