using System.Collections;
using System.Threading;
using UnityEngine;

public class HeavyAttackState : State
{
    public HeavyAttackState(PlayerBehaviour player, State parent) : base(player , parent){}

    private float _timer;
    
    public override void Enter()
    {
        if (player.swordAndShield)
        {
            player.hitBox.damage = player.damage * 2;
            player.StartCoroutine(Wait());
        }
        else if(player.bolter)
        {
            Aim();
        }
        
    }

    public override void Exit()
    {
    }

    private void Aim()
    {
        player.animator.Play("Aim");
        
        // add aim logic here
        
    }

    public void ExitAim()
    {
        player.stateMachine.Transit(player.idleState);
    }

    public override void ContinuousAction()
    {
        if (player.bolter)
        {
            Quaternion toRotation = Quaternion.LookRotation(Camera.main.transform.forward,Vector3.up);
            
            player.playerShoot.firePoint.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
            
            toRotation.x = 0;
            toRotation.z = 0;
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
            
            if (_timer >= 0)
                _timer -= Time.deltaTime;
        }
    }

    public override void OnAttack()
    {
        if(_timer <= 0)
            player.StartCoroutine(Shoot());
        
        
    }

    private IEnumerator Shoot()
    {
        _timer = player.fireRate;
        player.animator.Play("Shoot");
        player.playerShoot.Shoot();
        
        yield return new WaitForSeconds(0.25f);
        
        Aim();
    }

    private IEnumerator Wait()
    {
        player.stamina.TryUseStamina(20);
        player.animator.Play("HeavyAttack");
        
        yield return new WaitForSeconds(1.75f);
        player.stateMachine.Transit(player.idleState);
    }
}
