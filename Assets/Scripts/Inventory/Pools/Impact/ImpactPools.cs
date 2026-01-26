using System;
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Inventory.Pools.Impact
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ImpactPools : IFixedTickable
    {
        private readonly IPublisher<PlaySoundMessage> playSoundMessagePublisher;

        // [SerializeField] private Dictionary<ImpactConfig, List<ImpactPool>> cImpacts = new();
        private readonly List<ImpactPool> pools = new();
        private readonly List<(ImpactConfig, List<Impact>)> activeImpacts = new();

        // [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public ImpactPools
            (
                InventoryConfig inventoryConfig,
                [Key("PoolsParent")] Transform poolsParent,
                IPublisher<PlaySoundMessage> playSoundMessagePublisher
            )
        {
            var ImpactPoolsObj = new GameObject("Impact Pools");
            ImpactPoolsObj.transform.SetParent(poolsParent);
            
            this.playSoundMessagePublisher = playSoundMessagePublisher;

            foreach (var impact in inventoryConfig.Impacts)
            {
                pools.Add(new ImpactPool(impact));
                activeImpacts.Add((impact, new List<Impact>()));
            }
        }

        public void FixedTick()
        {
            var toRemove = new List<(ImpactConfig, List<Impact>)>();

            foreach (var impacts in activeImpacts)
            {
                var toRemoveList = new List<Impact>();
                foreach (var activeImpact in impacts.Item2)
                {
                    activeImpact.Time += Time.fixedDeltaTime;
                    if (activeImpact.Time >= impacts.Item1.LifeTime)
                    {
                        toRemoveList.Add(activeImpact);
                    }
                }
                toRemove.Add((impacts.Item1, toRemoveList));
            }
            foreach (var impacts in activeImpacts)
            {
                var type = impacts.Item1.Type;
                var toRemoveList = toRemove.First(tr => tr.Item1.Type.Equals(type)).Item2;
                var pool = pools.First(p => p.ImpactConfig.Type.Equals(type));
                
                foreach (var a in toRemoveList)
                {
                    impacts.Item2.Remove(a);
                    pool.Release(a);
                }
            }
        }

        public void Get(string tag, Vector3 position, Quaternion rotation)
        {
            var pool = pools.FirstOrDefault(p => p.ImpactConfig.Tag.Equals(tag));
            if (pool == default)
                pool = pools.First();
            var impact = pool.Get(position, rotation);
            activeImpacts.First(p => p.Item1.Type.Equals(pool.ImpactConfig.Type))
                         .Item2
                         .Add(impact);
            playSoundMessagePublisher.Publish(new PlaySoundMessage(pool.ImpactConfig.SoundConfig.SoundSettings, position, null));
        }
    }

    public class ImpactPool
    {
        public readonly ImpactConfig ImpactConfig;
        private readonly IObjectPool<Impact> pool;
        
        public ImpactPool(ImpactConfig impactConfig)
        {
            ImpactConfig = impactConfig;
            pool = new ObjectPool<Impact>(
                                          InstantiateImpact, //Метод создания объектов
                                          OnGet, //Действие при извлечении из пула
                                          OnRelease, //Действие при возврате в пул
                                          Destroy, //Очистка объектов (опционально)
                                          false, //Коллекция для отслеживания объектов не используется (опционально)
                                          200, //Минимальный размер пула
                                          2000 //Максимальный размер пула
                                         );
        }
        
        public Impact Get(Vector3 position, Quaternion rotation)
        {
            var impact = pool.Get();
            impact.GO.transform.SetPositionAndRotation(position, rotation);
            impact.GO.SetActive(true);

            return impact;
        }

        private Impact InstantiateImpact() => new (Object.Instantiate(ImpactConfig.Pref));
        public void Release(Impact impact) => pool.Release(impact);
        private void OnRelease(Impact impact) 
        {
            impact.GO.SetActive(false);
            impact.GO.transform.position = Vector3.zero;
            impact.Time = .0f;
        }
        
        private void OnGet(Impact impact) { }
        private void Destroy(Impact impact) { }
    }

    [Serializable]
    public class Impact
    {
        public GameObject GO;
        public float Time;

        public Impact(GameObject go)
        {
            GO = go;
        }
    }
}

