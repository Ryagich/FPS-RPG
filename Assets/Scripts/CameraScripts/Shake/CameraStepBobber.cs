using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CameraScripts.Shake
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class CameraStepBobber : ITickable
    {
        private readonly Transform cameraParentTransform;

        // То, что мы уже добавили к локальной позиции по Y
        private float appliedY;

        private readonly List<BobInstance> bobs = new(16);

        private struct BobInstance
        {
            public float Elapsed;
            public float Duration;
            public float Amplitude;          // в метрах
            public AnimationCurve Curve;     // ожидаем 0 -> -1 -> 0
        }

        public CameraStepBobber([Key("CameraParentTransform")] Transform cameraParentTransform)
        {
            this.cameraParentTransform = cameraParentTransform;
        }

        /// <summary>
        /// Один шаговый bob: камера опускается и возвращается за duration.
        /// </summary>
        public void AddStepBob(float duration, float amplitude, AnimationCurve curve)
        {
            if (duration <= 0f || amplitude <= 0f)
                return;

            bobs.Add(new BobInstance
            {
                Elapsed = 0f,
                Duration = duration,
                Amplitude = amplitude,
                Curve = curve
            });
        }

        public void Tick()
        {
            var dt = Time.deltaTime;

            float targetY = 0f;

            for (int i = bobs.Count - 1; i >= 0; i--)
            {
                var b = bobs[i];
                b.Elapsed += dt;

                if (b.Elapsed >= b.Duration)
                {
                    bobs.RemoveAt(i);
                    continue;
                }

                var t01 = Mathf.Clamp01(b.Elapsed / b.Duration);

                // Если кривая не задана, используем физически понятный профиль: 0 -> -1 -> 0
                var k = (b.Curve != null && b.Curve.length > 0)
                    ? b.Curve.Evaluate(t01)
                    : -Mathf.Sin(t01 * Mathf.PI);

                targetY += k * b.Amplitude;

                bobs[i] = b;
            }

            // Применяем дельтой, чтобы не затирать чужие анимации позиции
            var deltaY = targetY - appliedY;
            if (Mathf.Abs(deltaY) > 0.00001f)
            {
                cameraParentTransform.localPosition += new Vector3(0f, deltaY, 0f);
                appliedY = targetY;
            }
        }
    }
}
