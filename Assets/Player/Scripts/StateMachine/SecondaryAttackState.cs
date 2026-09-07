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
        
        player.StopAllCoroutines();
    }

    private void Aim()
    {
        //player.animator.Play("Aim");
        
        player.camera.canAim = true;
    }

    public void ExitAim()
    {
        player.camera.canAim = false;
        
        player.stateMachine.Transit(player.idleState);
    }

    public override void ContinuousAction()
    {
        if (player.bolter)
        {
            // Body still turns to face the camera's yaw
            Quaternion cameraYawRotation = Quaternion.LookRotation(player.cameraTransform.forward, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation,
                new Quaternion(0, cameraYawRotation.y, 0, cameraYawRotation.w), Time.deltaTime * 50f);

            // Fire point aims at what the crosshair is actually over
            Vector3 aimPoint     = player.camera.AimPoint;
            Vector3 firePointPos = player.playerShoot.firePoint.transform.position;
            Quaternion aimRotation = Quaternion.LookRotation((aimPoint - firePointPos).normalized, Vector3.up);

            player.playerShoot.firePoint.transform.rotation = Quaternion.Slerp(
                player.playerShoot.firePoint.transform.rotation, aimRotation, Time.deltaTime * 9999f);

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
        
        //Audio
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayPewPewAudioPlay(player.audioSource);
        }
        
        yield return new WaitForSeconds(0.25f);
        
        Aim();
    }

    private IEnumerator SwordAndShieldCombo()
    {
        player.ClearHitTargets();
        player.stamina.TryUseStamina(player.secondaryAttackStaminaCost);
        player.animator.Play("HeavyAttack");
        player.weapon.GetComponent<Collider>().enabled = true;
        
        //Audio
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlaySSHeavyAudioPlay(player.audioSource);
        }
        
        yield return new WaitForSeconds(1.75f);
        player.stateMachine.Transit(player.idleState);
    }
    
    private IEnumerator GreatSwordCombo()
    {
        player.ClearHitTargets();
        player.stamina.TryUseStamina(player.secondaryAttackStaminaCost);
        player.animator.Play("HeavyAttack");
        player.weapon.GetComponent<Collider>().enabled = true;
        
        //Audio
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayGSHeavyAudioPlay(player.audioSource);
        }
        
        yield return new WaitForSeconds(1.75f);
        
        player.stateMachine.Transit(player.idleState);
    }
}
