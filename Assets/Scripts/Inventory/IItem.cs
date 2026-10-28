using UnityEngine;

namespace Inventory
{
    public interface IItem
    {
        public GameObject GameObject { get; set; }
        public void Activate();
        public void Disable();
    }

    /// <summary>Способность предмета быть выброшенным. Нож её намеренно не реализует.</summary>
    public interface IDroppableItem
    {
        VContainer.Unity.LifetimeScope GetDropPrefab();
    }

    /// <summary>Общая первичная атака активного предмета.</summary>
    public interface IPrimaryActionItem
    {
        bool IsPrimaryActionInProgress { get; }
        bool TryStartPrimaryAction();
        void StopPrimaryAction();
    }
}
