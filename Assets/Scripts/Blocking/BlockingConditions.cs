using System;
using System.Collections.Generic;
using Good;
using NaughtyAttributes;

using UnityEngine;

namespace Fps.Shared.Game.Inventory.Weapon
{
    [Serializable]
    public class BlockingConditions
    {
        [field: SerializeField] public List<BlockingCondition> Conditions { get; private set; } = new();

        public bool IsCondition()
        {
            foreach (var condition in Conditions)
                if (!condition.IsCondition())
                    return false;
            return true;
        }
    }
    
    [Serializable]
    public class BlockingCondition
    {
        [field: SerializeField] public BlockingTypes Type = BlockingTypes.Nothing;
        [field: SerializeField, ShowIf(nameof(IsKillFromWeaponType)), AllowNesting] 
        public FromWeaponBlockingConditions KillFromWeaponConditions = null!;
        [field: SerializeField, ShowIf(nameof(IsKillFromCharacterType)), AllowNesting] 
        public KillsFromCharacterBlockingConditions KillFromCharacterConditions = null!;
        [field: SerializeField, ShowIf(nameof(IsKillFromClassType)), AllowNesting] 
        public KillsFromClassBlockingConditions KillFromClassConditions = null!;
        [field: SerializeField, ShowIf(nameof(IsWinFromClassType)), AllowNesting] 
        public WinFromClassBlockingConditions WinFromClassConditions = null!;
        [field: SerializeField, ShowIf(nameof(IsWinFromCharacterType)), AllowNesting] 
        public WinFromCharacterBlockingConditions WinFromCharacterConditions = null!;
        [field: SerializeField, ShowIf(nameof(IsPurchaseType)), AllowNesting] 
        public BlockingByPurchase PurchaseCondition = null!;

        private bool IsKillFromWeaponType() => Type is BlockingTypes.KillFromWeapon;
        private bool IsKillFromCharacterType() => Type is BlockingTypes.KillFromCharacter;
        private bool IsKillFromClassType() => Type is BlockingTypes.KillFromClass;
        private bool IsWinFromClassType() => Type is BlockingTypes.WinFromClass;
        private bool IsWinFromCharacterType() => Type is BlockingTypes.WinFromCharacter;
        private bool IsPurchaseType() => Type is BlockingTypes.Purchase;

        public bool IsCondition()
        {
            switch (Type)
            {
                case BlockingTypes.Nothing:
                    return true;
                case BlockingTypes.KillFromWeapon:
                    return KillFromWeaponConditions.IsCondition();
                case BlockingTypes.KillFromCharacter:
                    return KillFromCharacterConditions.IsCondition();
                case BlockingTypes.KillFromClass:
                    return KillFromClassConditions.IsCondition();
                case BlockingTypes.WinFromClass:
                    return WinFromClassConditions.IsCondition();
                case BlockingTypes.WinFromCharacter:
                    return WinFromCharacterConditions.IsCondition();
                case BlockingTypes.Purchase:
                    return PurchaseCondition.IsCondition();
                default: 
                    return false;
            }
        }

        public IBlockingCondition Get()
        {
            switch (Type)
            {
                case BlockingTypes.Nothing:
                    return null!;
                case BlockingTypes.KillFromWeapon:
                    return KillFromWeaponConditions;
                case BlockingTypes.KillFromCharacter:
                    return KillFromCharacterConditions;
                case BlockingTypes.KillFromClass:
                    return KillFromClassConditions;
                case BlockingTypes.WinFromClass:
                    return WinFromClassConditions;
                case BlockingTypes.WinFromCharacter:
                    return WinFromCharacterConditions;
                case BlockingTypes.Purchase:
                    return PurchaseCondition;
                default: 
                    return null!;
            }
        }
    }

    [Serializable]
    public class BlockingByPurchase : IBlockingCondition
    {
        [field: SerializeField] public GoodConfig GoodConfig { get; private set; } = null!;
        [field: SerializeField] public int Count { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
        public void SetValue(int value) { }
        public bool IsCondition() => GoodConfig.State;
    }
        
    [Serializable]
    public class FromWeaponBlockingConditions : IBlockingCondition
    {
        [field: SerializeField] public string WeaponId { get; private set; } = null!;
        [field: SerializeField] public int Count { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        public void SetWeaponId(string id) => WeaponId = id;
        public bool IsCondition() => Value > Count;
        public void SetValue(int value) => Value = value;
    }

    [Serializable]
    public class KillsFromCharacterBlockingConditions : IBlockingCondition
    {
        [field: SerializeField] public string CharacterId { get; private set; } = null!;
        [field: SerializeField] public int Count { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        public bool IsCondition() => Value >= Count;
        public void SetValue(int value) => Value = value;
    }
    
    [Serializable]
    public class KillsFromClassBlockingConditions : IBlockingCondition
    {
        [field: SerializeField] public string ClassId { get; private set; } = null!;
        [field: SerializeField] public int Count { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        public bool IsCondition() => Value >= Count;
        public void SetValue(int value) => Value = value;
    }
    
    [Serializable]
    public class WinFromCharacterBlockingConditions : IBlockingCondition
    {
        [field: SerializeField] public string CharacterId { get; private set; } = null!;
        [field: SerializeField] public int Count { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        public bool IsCondition() => Value >= Count;
        public void SetValue(int value) => Value = value;
    }
    
    [Serializable]
    public class WinFromClassBlockingConditions : IBlockingCondition
    {
        [field: SerializeField] public string ClassId { get; private set; } = null!;
        [field: SerializeField] public int Count { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

        public bool IsCondition() => Value >= Count;
        public void SetValue(int value) => Value = value;
    }
    
    public interface IBlockingCondition
    {
        public int Count { get; }
        public int Value { get; }
        public bool IsCondition();
        public void SetValue(int value);
    }
    
    public enum BlockingTypes
    {
        Nothing,
        KillFromWeapon,
        KillFromCharacter,
        KillFromClass,
        WinFromCharacter,
        WinFromClass,
        Purchase,
    }
}
