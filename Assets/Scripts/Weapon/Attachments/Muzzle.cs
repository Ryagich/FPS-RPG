using UnityEngine;
using VContainer.Unity;

namespace Weapon.Attachments
{
    public sealed class Muzzle : IAttachment, ITickable
    {
        private readonly ParticleSystem particles;
        private readonly Light flashLight;
        private readonly int particleCount;
        private readonly float flashDuration;
        private float flashTime;
        public AttachmentBaseInfo AttachmentBaseInfo { get; set; }
        public Transform Transform { get; }
        public GameObject GameObject => Transform.gameObject;
        public Transform ShotPoint { get; }

        public Muzzle(Transform transform, AttachmentBaseInfo info, Transform shotPoint,
            ParticleSystem particles, Light flashLight, int particleCount, float flashDuration)
        {
            Transform = transform;
            AttachmentBaseInfo = info;
            ShotPoint = shotPoint;
            this.particles = particles;
            this.flashLight = flashLight;
            this.particleCount = particleCount;
            this.flashDuration = flashDuration;
        }

        public void Effect()
        {
            if (particles != null)
                particles.Emit(particleCount);
            if (flashLight != null)
            {
                flashTime = flashDuration;
                flashLight.enabled = true;
            }
        }
        
        public void Tick()
        {
            if (flashLight == null || !flashLight.enabled)
                return;
            flashTime -= Time.deltaTime;
            if (flashTime <= 0f)
                flashLight.enabled = false;
        }
    }
}