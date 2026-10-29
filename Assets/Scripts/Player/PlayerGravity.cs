using Gravity;
using UnityEngine;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerGravity
    {
        private readonly PlayerGrounding grounding;
        private readonly GravityConfig config;

        private float verticalVelocity;

        public PlayerGravity
            (
                PlayerGrounding grounding,
                GravityConfig config
            )
        {
            this.grounding = grounding;
            this.config = config;
        }

        public Vector3 GetVelocity()
        {
            if (grounding.HasGroundSupport)
            {
                verticalVelocity = 0f;
            }
            // v = v0 + g * dt
            verticalVelocity -= config.Gravity * Time.deltaTime;

            // Δs = v * dt
            var velocity = Vector3.up * verticalVelocity * Time.deltaTime;
            
            return velocity;
        }
    }
}
