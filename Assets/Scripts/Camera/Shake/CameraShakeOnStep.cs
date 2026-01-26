using Sounds.Movement;

namespace Camera.Shake
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CameraShakeOnStep
    {
        private readonly CameraShaker cameraShaker;
        private readonly MovementSoundConfig config;

        private bool leftStep;

        public CameraShakeOnStep(
                CameraShaker cameraShaker,
                MovementSoundConfig config
            )
        {
            this.cameraShaker = cameraShaker;
            this.config = config;
        }

        public void OnStep()
        {
            var direction = leftStep ? -1f : 1f;
            leftStep = !leftStep;

            cameraShaker.AddStepShake(
                                      duration: config.StepShakeDuration,
                                      amplitude: config.StepShakeAmplitude,
                                      direction: direction
                                     );
        }
    }
}