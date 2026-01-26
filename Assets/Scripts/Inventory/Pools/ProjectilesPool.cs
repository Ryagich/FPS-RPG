using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Weapon;

namespace Inventory.Pools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ProjectilesPool
    {
        private readonly Projectile projectilePref;
        private readonly IObjectPool<Projectile> ProjectilePool;
        private readonly GameObject projectilePoolObj;
        
        // [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public ProjectilesPool
            (
                InventoryConfig inventoryConfig,
                [Key("PoolsParent")] Transform poolsParent
            )
        {
            projectilePoolObj = new GameObject("Projectiles Pool");
            projectilePoolObj.transform.SetParent(poolsParent);
            
            projectilePref = inventoryConfig.ProjectilePref;
            ProjectilePool = new ObjectPool<Projectile>(
                                            Instantiate, //Метод создания объектов
                                            OnGet, //Действие при извлечении из пула
                                            OnRelease, //Действие при возврате в пул
                                            DestroyProjectile, //Очистка объектов (опционально)
                                            false, //Коллекция для отслеживания объектов не используется (опционально)
                                            200, //Минимальный размер пула
                                            2000 //Максимальный размер пула
                                           );
        }

        public Projectile Get(Vector3 position, Vector3 rotation)
        {
            var projectile = ProjectilePool.Get();
            var projectileTrans = projectile.transform;
            var trail = projectile.GetComponent<TrailRenderer>();
            
            projectileTrans.SetPositionAndRotation(position, Quaternion.LookRotation(rotation));
            projectile.gameObject.SetActive(true);
            trail.Clear();
            trail.time = 1;
            
            projectile.GetComponent<Collider>().enabled = true;
            projectile.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            projectile.CanInteract = true;

            return projectile;
        }
        
        public Projectile Get(Vector3 position, Quaternion rotation)
        {
            var projectile = ProjectilePool.Get();
            projectile.projectilesPool = this;
            var projectileTrans = projectile.transform;
            var trail = projectile.GetComponent<TrailRenderer>();

            projectileTrans.SetPositionAndRotation(position, rotation);
            projectile.gameObject.SetActive(true);
            trail.Clear();
            trail.time = 1;
            
            projectile.GetComponent<Collider>().enabled = true;
            projectile.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            projectile.CanInteract = true;

            return projectile;
        }
        
        public void Release(Projectile projectile)
        {
            var rb = projectile.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            projectile.GetComponent<Collider>().enabled = false;
            projectile.gameObject.SetActive(false);
            projectile.CanInteract = false;
            ProjectilePool.Release(projectile);
        }
        
        private Projectile Instantiate()
        {
            return Object.Instantiate(projectilePref, projectilePoolObj.transform);
        }

        private void OnGet(Projectile projectile) { }
        private void OnRelease(Projectile projectile) { }
        private void DestroyProjectile(Projectile projectile) { }
    }
}
