using UnityEngine;
using VContainer.Unity;

namespace Player
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerMotor //: ITickable
    {
        // private readonly CharacterController controller;
        // private readonly PlayerGravity gravity;
        // private readonly PlayerJump jump;
        //
        // public PlayerMotor
        //     (
        //         CharacterController controller,
        //         PlayerGravity gravity,
        //         PlayerJump jump
        //     )
        // {
        //     this.controller = controller;
        //     this.gravity = gravity;
        //     this.jump = jump;
        // }

        // public void Tick()
        // {
        //     var grounded = controller.isGrounded;
        //     var vertical = gravity.Tick(grounded, Time.deltaTime) + jump.Tick(grounded);
        //
        //     controller.Move(Vector3.up * vertical * Time.deltaTime);
        // }
    }
}