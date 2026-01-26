using System;
using UnityEngine;

namespace Weapon.Settings
{
    [Serializable]
    public class RecoilSettings
    {
        [field: Header("на сколько растёт recoil.x и recoil.y при одном выстреле")]
        [field: SerializeField] public Vector2 RecoilPower {get; private set;}
        [field: Header("максимальное ограничение recoil по осям")]
        [field: SerializeField] public Vector2 MaxRecoilPower {get; private set;}
        [field: Header("базовый «разброс» перекрестия")] 
        [field: SerializeField] public float Spread { get; private set; } = .25f; //разброс
        [field: Header("как сильно recoil.y влияет на текущий Spread")]
        [field: SerializeField] public float RecoilSpreadMultiplier { get; private set; } = 1.0f;
        [field: Header("с какой скоростью Spread возвращается к базовому")] 
        [field: SerializeField] public float SpreadChillCoefficient { get; private set; } = .5f;
        [field: Tooltip("множитель затухания recoil (1 = без затухания, 0 = мгновенная очистка)")] 
        [field: SerializeField, Range(.0f, 1.0f)] public float RecoilChillCoefficient { get; private set; } = .5f;
    }
}