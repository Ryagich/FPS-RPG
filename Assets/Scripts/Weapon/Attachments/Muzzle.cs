using System.Collections;
using UnityEngine;

namespace Weapon.Attachments
{
    public class Muzzle : MonoBehaviour, IAttachment
    {
        [field: SerializeField] public AttachmentBaseInfo AttachmentBaseInfo { get; set; } = null!;
        [field: SerializeField] public Transform ShotPoint { get; private set; } = null!;
        [field: SerializeField] public ParticleSystem particles { get; private set; } = null!;
        [field: SerializeField] private Light flashLight = null!;
        [field: SerializeField] private int flashParticlesCount = 5;
        [field: SerializeField] private float flashLightDuration;

        public void Effect()
        {
            if(particles != null)
                particles.Emit(flashParticlesCount);

            if (flashLight != null)
            {
                flashLight.enabled = true;
                StartCoroutine(nameof(DisableLight));
            }
        }
        
        private IEnumerator DisableLight()
        {
            yield return new WaitForSeconds(flashLightDuration);
            flashLight.enabled = false;
        }
    }
}