using UnityEngine;

namespace Weapon.Settings
{
    /// <summary>
    /// Общие данные любого боевого предмета. Специализированные конфиги не должны
    /// дублировать урон и темп основной атаки.
    /// </summary>
    public abstract class WeaponConfigBase : ScriptableObject
    {
        [field: SerializeField] public string ID { get; private set; } = "Weapon ID";
        [field: SerializeField] public DamageSettings DamageSettings { get; private set; } = new();
        [field: Min(0.01f), SerializeField] public float AttackInterval { get; private set; } = 0.1f;

    }
}
