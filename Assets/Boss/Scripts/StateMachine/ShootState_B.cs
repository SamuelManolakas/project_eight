using System.Collections;
using UnityEngine;

public class ShootState_B : State_B
{
    public ShootState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.animator.Play("Shoot");
        StartCoroutine(Wait());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerRangedAudio();

        }
        boss.SetEngineSound(0);

        boss.currentTarget = GetFurthestPlayer();
    }

    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        Vector3 direction = boss.FlatDirectionTo(boss.currentTarget.transform.position);
        boss.TurnToward(boss.upperBody.transform, direction, 3f); // torso tracks the target during the attack
    }

    private IEnumerator Wait()
    {
        boss.animator.Play("Shoot");
        yield return new WaitForSeconds(1.5f);
        boss.Shoot();
        yield return new WaitForSeconds(2.5f);
        boss.stateMachine.Transit(boss.movementState);
    }
    
    private GameObject GetFurthestPlayer()
    {
        GameObject furthest = null;
        float maxDistance = float.MinValue;

        foreach (var player in boss.players)
        {
            if (player == null) continue;
            float dist = Vector3.Distance(boss.transform.position, player.transform.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = player;
            }
        }

        return furthest;
    }
}
