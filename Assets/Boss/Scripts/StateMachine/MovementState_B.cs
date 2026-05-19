using UnityEngine;

public class MovementState_B : State_B
{
    public MovementState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    private float _rangedAttackTimer;
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        Vector3 move = boss.currentTarget.transform.position - boss.transform.position;
        move = Vector3.ClampMagnitude(move, 1);
        move.y = 0;
        
        Vector3 finalMove = move * boss.speed;
        
        if (Vector3.Distance(boss.transform.position, boss.currentTarget.transform.position) > 8f)
        {
            boss.controller.Move(finalMove * Time.deltaTime);
            boss.animator.SetFloat("speed", move.magnitude);
            
            _rangedAttackTimer += Time.deltaTime;
            if (_rangedAttackTimer > 7f)
            {
                _rangedAttackTimer = 0f;
                RangedAttack();
            }
        }
        else
        {
            boss.animator.SetFloat("speed", 0);
            CombatWheel();
        }
        
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            boss.lowerBody.transform.rotation = Quaternion.Slerp(boss.lowerBody.transform.rotation, toRotation, Time.deltaTime);
            boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
        }
    }

    private void CombatWheel()
    {
        int attack = Random.Range(0, 8);

        switch (attack)
        {
            case >= 0 and <= 2:
                boss.stateMachine.Transit(boss.combo1State);
                break;
            case 3 or 4:
                boss.stateMachine.Transit(boss.grabState);
                break;
            case 5:
                boss.stateMachine.Transit(boss.strikeState);
                break;
            case 6:
                boss.stateMachine.Transit(boss.sweepState);
                break;
        }
    }

    private void RangedAttack()
    {
        int attack = Random.Range(0, 10);

        switch (attack)
        {
            case >= 0 and <= 3:
                break;
            case 4:
                boss.stateMachine.Transit(boss.chargeState);
                break;
            case >= 5 and <= 9:
                boss.stateMachine.Transit(boss.shootState);
                break;
        }
    }
}
