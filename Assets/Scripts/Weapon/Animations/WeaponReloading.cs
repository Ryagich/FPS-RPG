using System;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon.Animations
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class WeaponReloading : ITickable
    {
        public event Action EndedReloading;
        public event Action<float, float> UpdateReloadingTime;

        public bool IsReloading;
        
        private readonly WeaponConfig config;
        
        private float currentTime;

        public WeaponReloading
            (
                WeaponConfig config
            )
        {
            this.config = config;
        }
        
        public void Tick()
        {
            if (!IsReloading)
                return;
            currentTime += Time.deltaTime;
            UpdateReloadingTime?.Invoke(currentTime, config.ReloadingTime);

            if (currentTime >= config.ReloadingTime)
            {
                IsReloading = false;
                EndedReloading?.Invoke();
            }
        }
        
        public void StartReloading()
        {
            IsReloading = true;
            currentTime = .0f;
            UpdateReloadingTime?.Invoke(currentTime, config.ReloadingTime);
        }

        public void StopReloading()
        {
            IsReloading = false;
        }
    }
}