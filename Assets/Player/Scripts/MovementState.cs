using UnityEngine;

public class MovementState : State
{
    public MovementState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        Debug.Log("Entered MovementState");
    }

    public override void Exit()
    {
        Debug.Log("Exiting MovementState");
    }

    public override void ContinuousAction()
    {
        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * player.moveInput.y + camRight * player.moveInput.x;

        player.controller.Move(move * player.speed * Time.deltaTime);
        player.animator.SetFloat("speed", move.magnitude);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
        }
        else
        {
            player.stateMachine.Transit(player.idleState);
        }
    }
}
