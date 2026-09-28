using System.Collections;
using UnityEngine;

public class MovementState_B : State_B
{
    public MovementState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    private float _attackCooldownTimer;

    private bool _hasFiredNuke;
    public override void Enter()
    {
        //Audio
        boss.SetEngineSound(1);

        _attackCooldownTimer = boss.attackCooldown;

        if (boss.currentHealth.Value <= boss.maxHealth / 2 && !_hasFiredNuke)
        {
            StartCoroutine(FireNuke());
        }
    }

    private float _rangedAttackTimer;
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        Vector3 targetPosition = boss.currentTarget.transform.position;
        Vector3 move = boss.FlatDirectionTo(targetPosition);

        if (_attackCooldownTimer >= 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }

        if (boss.MoveToward(targetPosition, boss.speed, BossBehaviour.StoppingDistance))
        {
            boss.animator.SetFloat(AnimatorParams.Speed, move.magnitude);
            
            _rangedAttackTimer += Time.deltaTime;
            if (_rangedAttackTimer > 7f)
            {
                _rangedAttackTimer = 0f;
                RangedAttack();
            }
        }
        else
        {
            boss.animator.SetFloat(AnimatorParams.Speed, 0);

            if (_attackCooldownTimer <= 0)
            {
                CombatWheel();
            }
        }

        boss.FaceDirection(move);
    }

    private void CombatWheel()
    {
        int attack = Random.Range(0, 37);

        switch (attack)
        {
            case >= 0 and <= 4:
                boss.stateMachine.Transit(boss.combo1State);
                break;
            case >= 5 and <= 9:
                boss.stateMachine.Transit(boss.combo2State);
                break;
            case >= 10 and <= 12:
                boss.stateMachine.Transit(boss.grabState);
                break;
            case >= 13 and <= 17:
                boss.stateMachine.Transit(boss.strikeState);
                break;
            case >= 18 and <= 20:
                boss.stateMachine.Transit(boss.sweepState);
                break;
            case >= 21 and <= 25:
                boss.stateMachine.Transit(boss.chestFlamerState);
                break;
            case >= 26 and <= 36:
                boss.stateMachine.Transit(boss.SpinAttackState);
                break;
        }
    }

    private IEnumerator FireNuke()
    {
        yield return new WaitForSeconds(0.2f);

        // Set only once the nuke actually starts: if the boss leaves this state during the wait,
        // this coroutine is cancelled and the nuke is retried next time it's back in movement.
        _hasFiredNuke = true;
        boss.stateMachine.Transit(boss.NukeState);
    }

    private void RangedAttack()
    {
        int attack = Random.Range(0, 16);

        switch (attack)
        {
            case >= 0 and <= 5:
                break;
            case >= 6 and <= 9:
                boss.stateMachine.Transit(boss.chargeState);
                break;
            case >= 10 and <= 15:
                boss.stateMachine.Transit(boss.shootState);
                break;
        }
    }
}
