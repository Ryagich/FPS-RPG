using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapon.Attachments
{
    [Serializable]
    public class AttachmentSection
    {
        [field: SerializeField] public AttachmentSectionInfo MainInfo { get; private set; }
        [SerializeField] private List<AttachmentInfo> attachmentInfos = new();
        // [field: Header("Положение ui элемента")]
        // [field: SerializeField] public Vector3 UIPosition { get; private set; }
        // [field: SerializeField] public Vector2 Directional { get; private set; }
        
        public readonly List<AttachmentInfo> currentAttachmentInfos = new();
        public List<AttachmentInfo> AttachmentInfos => attachmentInfos;
        public IEnumerable<AttachmentInfo> CurrentAttachmentInfos => currentAttachmentInfos;

        public void RemoveCurrentAttachment(AttachmentInfo toRemove)
        {
            currentAttachmentInfos.Remove(toRemove);
        }
        
        public void AddCurrentAttachments(AttachmentInfo toAdd)
        {
            currentAttachmentInfos.Add(toAdd);
        }

        public int GetCurrentIndex(AttachmentInfo toIndex)
        {
            return currentAttachmentInfos.IndexOf(toIndex);
        }

        public void SetByIndex(int i, AttachmentInfo toSet)
        { 
            currentAttachmentInfos[i] = toSet;
        }

        public AttachmentInfo GetAttachmentInfoById(string id)
        {
            return AttachmentInfos.First(attInfo => attInfo.BaseInfo.ID.Equals(id));
        }
    }
}