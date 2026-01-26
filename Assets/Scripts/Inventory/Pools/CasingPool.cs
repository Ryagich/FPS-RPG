using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Inventory.Pools
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CasingPool : IFixedTickable
    {
        private readonly GameObject casingPref;
        private readonly float casingLifeTime;
        private readonly IObjectPool<GameObject> casingPool;
        private readonly Transform casingPoolsObj;
        
        private readonly List<ActiveCasing> activeCasings = new();

        private struct ActiveCasing
        {
            public GameObject Go;
            public float TimeLeft;
        }
        
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public CasingPool
            (
                InventoryConfig inventoryConfig,
                [Key("PoolsParent")] Transform poolsParent
            )
        {
            casingPoolsObj = new GameObject("Casing Pool").transform;
            casingPoolsObj.SetParent(poolsParent);
            
            casingPref = inventoryConfig.CasingPref;
            casingLifeTime = inventoryConfig.casingLifeTime;
            
            casingPool = new ObjectPool<GameObject>(CreateCasing, //Метод создания объектов
                                                    OnGet, //Действие при извлечении из пула
                                                    OnRelease, //Действие при возврате в пул
                                                    DestroyProjectile, //Очистка объектов (опционально)
                                                    false, //Коллекция для отслеживания объектов не используется (опционально)
                                                    200, //Минимальный размер пула
                                                    1000 //Максимальный размер пула
                                                   );
        }
        
        public void FixedTick()
        {
            var dt = Time.fixedDeltaTime;

            for (var i = activeCasings.Count - 1; i >= 0; i--)
            {
                var casing = activeCasings[i];
                casing.TimeLeft -= dt;

                if (casing.TimeLeft <= 0f)
                {
                    casingPool.Release(casing.Go);
                    activeCasings.RemoveAt(i);
                }
                else
                {
                    activeCasings[i] = casing;
                }
            }
        }
        
        public void GetCasing
            (
                Vector3 position,
                Quaternion rotation,
                Vector2 forceRange,
                float ejectTorque,
                float coneAngle
           )
        {
            var casing = casingPool.Get();

            casing.transform.SetPositionAndRotation(position, rotation);

            var rb = casing.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Конус вылета
            var baseDir = casing.transform.TransformDirection(Vector3.right);
            var angleDeg = Random.Range(-coneAngle, coneAngle);
            var dir = Quaternion.AngleAxis(angleDeg, casing.transform.up) * baseDir;

            // Сила
            var force = Random.Range(forceRange.x, forceRange.y);
            rb.AddForce(dir.normalized * force, ForceMode.VelocityChange);
            rb.AddTorque(Random.onUnitSphere * ejectTorque, ForceMode.Impulse);

            activeCasings.Add(new ActiveCasing
                              {
                                  Go = casing,
                                  TimeLeft = casingLifeTime
                              });
        }
        
        private GameObject CreateCasing()
        {
            var go = Object.Instantiate(casingPref, casingPoolsObj);
            go.SetActive(false);
            return go;
        }

        private void OnGet(GameObject casing)
        {
            casing.gameObject.SetActive(true);
        }
        
        private void OnRelease(GameObject casing)
        {
            casing.gameObject.SetActive(false);
            casing.transform.position = casingPoolsObj.position;
        }

        private void DestroyProjectile(GameObject casing) 
        {
           Object.Destroy(casing);
        }
    }
}
