using Sounds.Movement;

namespace CameraScripts.Shake
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CameraShakeOnStep
    {
        private readonly CameraShaker cameraShaker;
        private readonly CameraStepBobber cameraStepBobber;
        private readonly MovementSoundConfig config;

        private bool leftStep;

        public CameraShakeOnStep(
                CameraShaker cameraShaker,
                CameraStepBobber cameraStepBobber,
                MovementSoundConfig config
            )
        {
            this.cameraShaker = cameraShaker;
            this.cameraStepBobber = cameraStepBobber;
            this.config = config;
        }

        public void OnStep()
        {
            var direction = leftStep ? -1f : 1f;
            leftStep = !leftStep;

            // Roll (влево-вправо)
            cameraShaker.AddStepShake(
                                      duration: config.StepShakeDuration,
                                      amplitude: config.StepShakeAmplitude,
                                      direction: direction
                                     );

            // Bob (вверх-вниз) — за то же время, что и roll
            cameraStepBobber.AddStepBob(
                                        duration: config.StepShakeDuration,
                                        amplitude: config.StepBobAmplitude,
                                        curve: config.StepBobCurve
                                       );
        }
    }
}