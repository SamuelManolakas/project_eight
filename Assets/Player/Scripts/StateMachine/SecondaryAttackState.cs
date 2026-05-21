using System.Collections;
using System.Threading;
using UnityEngine;

public class SecondaryAttackState : State
{
    public SecondaryAttackState(PlayerBehaviour player, State parent) : base(player , parent){}

    private float _timer;
    
    public override void Enter()
    {
        if (player.swordAndShield)
        {
            player.hitBox.damage = player.secondaryAttackDamage;
            player.StartCoroutine(SwordAndShieldCombo());
        }
        else if(player.greatSword)
        {
            player.hitBox.damage = player.secondaryAttackDamage;
            player.StartCoroutine(GreatSwordCombo());
        }
        else if(player.bolter)
        {
            Aim();
        }
        
    }

    public override void Exit()
    {
        if (player.swordAndShield || player.greatSword)
            player.weapon.GetComponent<Collider>().enabled = false;
    }

    private void Aim()
    {
        player.animator.Play("Aim");
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
            
            player.playerShoot.firePoint.transform.rotation = Quaternion.Slerp(player.playerShoot.firePoint.transform.rotation, toRotation, Time.deltaTime * 10f);
            
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, new Quaternion(0,toRotation.y,0, toRotation.w), Time.deltaTime * 10f);
            
            if (_timer >= 0)
                _timer -= Time.deltaTime;
        }
    }

    public override void OnPrimaryAttack()
    {
        if(_timer <= 0 && player.ammo > 0)
            player.StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        _timer = player.fireRate;
        player.ammo--;
        player.animator.Play("Shoot");
        player.playerShoot.Shoot();
        
        yield return new WaitForSeconds(0.25f);
        
        Aim();
    }

    private IEnumerator SwordAndShieldCombo()
    {
        player.ClearHitTargets();
        player.stamina.TryUseStamina(player.secondaryAttackStaminaCost);
        player.animator.Play("HeavyAttack");
        player.weapon.GetComponent<Collider>().enabled = true;
        
        yield return new WaitForSeconds(1.75f);
        player.stateMachine.Transit(player.idleState);
    }
    
    private IEnumerator GreatSwordCombo()
    {
        player.ClearHitTargets();
        player.stamina.TryUseStamina(player.secondaryAttackStaminaCost);
        player.animator.Play("HeavyAttack");
        player.weapon.GetComponent<Collider>().enabled = true;
        
        yield return new WaitForSeconds(1.75f);
        
        player.stateMachine.Transit(player.idleState);
    }
}
