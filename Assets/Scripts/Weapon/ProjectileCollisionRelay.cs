using System;
using UnityEngine;

namespace Weapon
{
    // Unity collision messages require a component; gameplay remains in Projectile.
    public sealed class ProjectileCollisionRelay : MonoBehaviour
    {
        public event Action<Collision> Collided;
        private void OnCollisionEnter(Collision collision) => Collided?.Invoke(collision);
    }
}
