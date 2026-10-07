using System;
using InteractableScripts;
using UnityEngine;
using VContainer.Unity;
using Weapon.Providers;
using Weapon.Settings;
using Object = UnityEngine.Object;

namespace Weapon.Drop
{
    public sealed class WeaponAdderInInventory : IStartable, IDisposable
    {
        private readonly WeaponConfig config;
        private readonly GameObject gameObject;
        private readonly Interactable interactable;

        public WeaponAdderInInventory(WeaponConfig config, Interactable interactable, GameObject gameObject)
        {
            this.config = config;
            this.gameObject = gameObject;
            this.interactable = interactable;
            interactable.Interacted += AddWeapon;
        }

        private void AddWeapon(WeaponProvider actor)
        {
            actor.TakeNewWeapon(config);
            Object.Destroy(gameObject);
        }

        public void Start() { }
        public void Dispose() => interactable.Interacted -= AddWeapon;
    }
}