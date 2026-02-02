using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Inventory.Pools;
using Inventory.Pools.Impact;
using Player.Stats;
using UnityEngine;
using VContainer.Unity;
using VContainer;
using Weapon.Settings;

namespace Weapon
{
    public class Projectile : MonoBehaviour
    {
        [HideInInspector] public bool CanInteract;
        [HideInInspector] public WeaponConfig WeaponConfig;
        [SerializeField, Min(.0f)] private float _timeToDeath = 2.0f;
        [SerializeField, Min(.0f)] private float distanceToUseShotPointRotation = .5f;
        [SerializeField, Min(.0f)] private float trailTime = .2f;

        private Coroutine coroutine = null!;
        private Vector3 shotPosition;
        private TrailRenderer trail;
        public ProjectilesPool projectilesPool;
        private ImpactPools impactPools;
        
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Init
            (
                ImpactPools impactPools,
                Vector3 shotPosition
            )
        {
            this.impactPools = impactPools;
            this.shotPosition = shotPosition;
            coroutine = StartCoroutine(DestroyAfter());
        }
        
        private void ApplyNow()
        {
            if (!trail) return;

            trail.emitting = false;
            trail.Clear();

            trail.time = trailTime;
            trail.emitting = true;
        }

        private async UniTaskVoid ApplyEndOfFrame()
        {
            // Ждём, пока Unity закончит внутренние апдейты/инициализацию трейла в этом кадре
            await UniTask.Yield();
           
            if (!trail)
                return;

            // Дожимаем значение ещё раз
            trail.time = trailTime;
        }
        
        private void Awake()
        {
            trail = GetComponent<TrailRenderer>();
        }
        
        private void OnEnable()
        {
            ApplyNow();
            ApplyEndOfFrame().Forget();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!CanInteract)
                return;
            if (coroutine != null!)
                StopCoroutine(coroutine);

            var direction = Vector3.Distance(transform.position, shotPosition) > distanceToUseShotPointRotation
                          ? Quaternion.LookRotation(-transform.forward)
                          : Quaternion.LookRotation(collision.contacts[0].normal);
            impactPools.Get(collision.gameObject.tag ,collision.contacts[0].point, direction);

            var targetScope = collision.gameObject.GetComponentInParent<LifetimeScope>();

            if (targetScope)
            {
                var statsController = targetScope.Container.Resolve<StatsController>();
                statsController.TakeDamage(WeaponConfig.DamageSettings.Damage);
            }
            
            projectilesPool.Release(this);
        }
        
        private IEnumerator DestroyAfter()
        {
            yield return new WaitForSeconds(_timeToDeath);
            projectilesPool.Release(this);
        }
    }
}
