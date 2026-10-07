using System;
using Dependencies;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Weapon;
using Weapon.Settings;
using Object = UnityEngine.Object;

namespace Inventory.Pools
{
    public sealed class ProjectilesPool : IDisposable
    {
        private readonly ProjectileLifetimeScope prefab;
        private readonly LifetimeScope scope;
        private readonly Transform parent;
        private readonly ObjectPool<Projectile> pool;
        
        public ProjectilesPool(InventoryConfig config, GameScope scopeReference,
            PoolsParent parentReference)
        {
            var scope = scopeReference.Value;
            var parent = parentReference.Value;
            prefab = config.ProjectilePref;
            this.scope = scope;
            this.parent = parent;
            pool = new ObjectPool<Projectile>(Create, null, projectile => projectile.Deactivate(),
                projectile => Object.Destroy(projectile.GameObject), false, 200, 2000);
        }

        public Projectile Get(Vector3 position, Quaternion rotation, WeaponConfig config)
        {
            var projectile = pool.Get();
            projectile.Launch(position, rotation, config, Release);
            return projectile;
        }
        
        public void Release(Projectile projectile) => pool.Release(projectile);

        private Projectile Create()
        {
            var instance = scope.CreateChildFromPrefab(prefab);
            instance.transform.SetParent(parent, false);
            return instance.Instance;
        }
        
        public void Dispose() => pool.Dispose();
    }
}
