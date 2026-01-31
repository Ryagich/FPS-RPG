using Inventory.Ammo;
using UnityEngine;
using VContainer.Unity;

namespace Scopes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ProjectInitializer : IStartable
    {
        private readonly AmmoStorage ammoStorage;

        public ProjectInitializer(AmmoStorage ammoStorage)
        {
            this.ammoStorage = ammoStorage;
        }

        public async void Start()
        {
            await ammoStorage.Ready;
            Debug.Log("Project resources ready");
        }
    }
}