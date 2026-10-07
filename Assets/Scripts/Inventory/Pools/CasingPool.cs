using System;
using System.Collections.Generic;
using Dependencies;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Inventory.Pools
{
    public sealed class CasingPool : IFixedTickable, IDisposable
    {
        private readonly Rigidbody prefab;
        private readonly Transform parent;
        private readonly float lifetime;
        private readonly ObjectPool<Rigidbody> pool;
        private readonly List<(Rigidbody Body, float TimeLeft)> active = new();
        
        public CasingPool(InventoryConfig config, CasingPrefab prefabReference,
            PoolsParent parentReference)
        {
            var prefab = prefabReference.Value;
            var parent = parentReference.Value;
            this.prefab = prefab;
            this.parent = parent;
            lifetime = config.casingLifeTime;
            pool = new ObjectPool<Rigidbody>(Create, null, body => body.gameObject.SetActive(false),
                body => Object.Destroy(body.gameObject), false, 200, 1000);
        }
        
        public void GetCasing(Vector3 position, Quaternion rotation, Vector2 forceRange,
            float ejectTorque, float coneAngle)
        {
            var body = pool.Get();
            body.transform.SetPositionAndRotation(position, rotation);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.gameObject.SetActive(true);
            var baseDirection = body.transform.TransformDirection(Vector3.right);
            var angle = Random.Range(-coneAngle, coneAngle);
            var direction = Quaternion.AngleAxis(angle, body.transform.up) * baseDirection;
            body.AddForce(direction.normalized * Random.Range(forceRange.x, forceRange.y), ForceMode.VelocityChange);
            body.AddTorque(Random.onUnitSphere * ejectTorque, ForceMode.Impulse);
            active.Add((body, lifetime));
        }
        
        public void FixedTick()
        {
            for (var i = active.Count - 1; i >= 0; i--)
            {
                var casing = active[i];
                casing.TimeLeft -= Time.fixedDeltaTime;
                if (casing.TimeLeft <= 0f)
                {
                    pool.Release(casing.Body);
                    active.RemoveAt(i);
                }
                else
                {
                    active[i] = casing;
                }
            }
        }
        
        private Rigidbody Create()
        {
            var body = Object.Instantiate(prefab, parent);
            body.gameObject.SetActive(false);
            return body;
        }
        
        public void Dispose()
        {
            pool.Dispose();
            active.Clear();
        }
    }
}
