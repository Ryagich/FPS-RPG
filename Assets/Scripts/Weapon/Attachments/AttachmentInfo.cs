using System;
using Fps.Shared.Game.Inventory.Weapon;
using NaughtyAttributes;
using UnityEngine;

namespace Weapon.Attachments
{
    [Serializable]
    public class AttachmentInfo
    {
        [field: SerializeField] public AttachmentBaseInfo BaseInfo { get; private set; }
        [Tooltip("Положение аттачмента относительно оружия")]
        [field: SerializeField] public Vector3 Offset { get; private set; }

        [field: Tooltip("Открыт ли аттачмент для данного оружия")]
        [field: SerializeField] public BlockingConditions Conditions { get; private set; } = null!;
        [field: Tooltip("Положение прицела")]
        [field: SerializeField, ShowIf(nameof(isScope)), AllowNesting] 
        public ScopesSettings ScopesSettings { get; private set; }

        private bool isScope() => BaseInfo != null && BaseInfo.isScope();
    }

    [Serializable]
    public class ScopesSettings
    {
        [field: SerializeField] public Vector3 AimPosition { get; private set; }
        [field: SerializeField] public Vector3 aimRotationEuler { get; private set; } = new (5f, .0f, .0f);
        [field: SerializeField] public float AddKickBack { get; private set; }
    } 
}