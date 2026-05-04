using System.Collections;
using UnityEngine;

public class DodgeState : State
{
    public DodgeState(PlayerBehaviour player, State parent) : base(player, parent){}

    private Vector3 _dodgeDirection;
    private float _iFrames;
    public override void Enter()
    {
        player.stamina.Value -= 10;
        player.animator.Play("Dodge");
        _dodgeDirection = player.controller.velocity.normalized;

        if (_dodgeDirection.sqrMagnitude == 0)
        {
            _dodgeDirection = -player.transform.forward;
        }
        
        player.StartCoroutine(Wait());

        _iFrames = 0.4f;
    }

    public override void Exit()
    {
        
    }

    public override void GetHit(int damage)
    {
        if (_iFrames >= 0)
        {
            if (!player.IsServer)
            {
                return;
            }
            player.currentHealth.Value -= damage;
        }
    }

    public override void ContinuousAction()
    {
        if(_iFrames > 0)
            _iFrames -= Time.deltaTime;
        
        player.controller.Move((_dodgeDirection * player.dodgeDistance + player.velocity) * Time.deltaTime);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        player.stateMachine.Transit(player.idleState);
    }

    public override void OnJump()
    {
    }
}
