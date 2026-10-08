using UnityEngine;
using VContainer.Unity;

namespace Weapon.Attachments
{
    public sealed class Muzzle : IAttachment, ITickable
    {
        private readonly ParticleSystem particles;
        private readonly Light flashLight;
        private readonly MuzzleBaseSettings settings;
        private float flashTime;
        public AttachmentBaseInfo AttachmentBaseInfo { get; set; }
        public Transform Transform { get; }
        public GameObject GameObject => Transform.gameObject;
        public Transform ShotPoint { get; }

        public Muzzle(Transform transform, AttachmentBaseInfo info, Transform shotPoint,
            ParticleSystem particles, Light flashLight)
        {
            Transform = transform;
            AttachmentBaseInfo = info;
            ShotPoint = shotPoint;
            this.particles = particles;
            this.flashLight = flashLight;
            settings = info.MuzzleBaseSettings;
        }

        public void Effect()
        {
            if (particles != null)
                particles.Emit(settings.ParticleCount);
            if (flashLight != null)
            {
                flashTime = settings.FlashDuration;
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