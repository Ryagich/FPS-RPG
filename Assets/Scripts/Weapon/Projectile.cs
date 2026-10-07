using System;
using Characters;
using Inventory.Pools.Impact;
using UnityEngine;
using VContainer.Unity;
using Weapon.Settings;

namespace Weapon
{
    public sealed class Projectile : ITickable, IDisposable
    {
        private readonly Rigidbody body;
        private readonly Collider collider;
        private readonly TrailRenderer trail;
        private readonly ProjectileCollisionRelay collisionRelay;
        private readonly ProjectileSettings settings;
        private readonly ImpactPools impacts;
        private readonly EntityTargets targets;
        private WeaponConfig config;
        private Action<Projectile> release;
        private Vector3 shotPosition;
        private float timeLeft;
        private bool active;

        public GameObject GameObject => body.gameObject;
        public Rigidbody Body => body;
        
        public Projectile(Rigidbody body, Collider collider, TrailRenderer trail,
            ProjectileCollisionRelay collisionRelay, ProjectileSettings settings,
            ImpactPools impacts, EntityTargets targets)
        {
            this.body = body;
            this.collider = collider;
            this.trail = trail;
            this.collisionRelay = collisionRelay;
            this.settings = settings;
            this.impacts = impacts;
            this.targets = targets;
            collisionRelay.Collided += OnCollision;
        }
        
        public void Launch(Vector3 position, Quaternion rotation, WeaponConfig config, Action<Projectile> release)
        {
            this.config = config;
            this.release = release;
            shotPosition = position;
            timeLeft = settings.Lifetime;
            active = true;
            body.transform.SetPositionAndRotation(position, rotation);
            body.constraints = RigidbodyConstraints.None;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            collider.enabled = true;
            GameObject.SetActive(true);
            trail.Clear();
            trail.time = settings.TrailTime;
            trail.emitting = true;
        }

        public void Deactivate()
        {
            active = false;
            release = null;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.constraints = RigidbodyConstraints.FreezeAll;
            collider.enabled = false;
            trail.emitting = false;
            GameObject.SetActive(false);
        }
        
        public void Tick()
        {
            if (!active)
                return;
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
                release(this);
        }
        
        private void OnCollision(Collision collision)
        {
            if (!active)
                return;
            var contact = collision.GetContact(0);
            var direction = Vector3.Distance(body.position, shotPosition) > settings.NormalDistance
                ? Quaternion.LookRotation(-body.transform.forward) : Quaternion.LookRotation(contact.normal);
            impacts.Get(collision.gameObject.tag, contact.point, direction);
            // Return to the pool before damage can destroy the target entity and its scope.
            var damage = config.DamageSettings.Damage;
            release(this);
            if (targets.TryGetDamage(collision.collider, out var target))
                target.TakeDamage(damage);
        }

        public void Dispose() => collisionRelay.Collided -= OnCollision;
    }
}
