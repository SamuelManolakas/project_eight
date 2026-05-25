using System.Collections;
using UnityEngine;

public class SpinAttackState_B : State_B
{
    public SpinAttackState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public Vector3 rotationSpeed = new Vector3(0f, -200f, 0f);
    public bool ignoreTimeScale = false;

    private float _spawnTimer;
    private float _spawnInterval = 0.08f;
    
    private bool _isSpinning = false;
    
    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.StartCoroutine(Spin());
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
    }
    
    private IEnumerator Spin()
    {
        boss.hitBox.hitTargets.Clear();
        boss.animator.Play("SpinAttack");
        
        yield return new WaitForSeconds(1f);
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;
        _isSpinning = true;

        yield return new WaitForSeconds(4.5f);
        _isSpinning = false;
        
        yield return new WaitForSeconds(0.5f);
        boss.stateMachine.Transit(boss.movementState);
    }

    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        
        if(!_isSpinning) return;
        
        float delta = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        boss.upperBody.transform.Rotate(rotationSpeed * delta, Space.Self);
        
        Vector3 move = boss.currentTarget.transform.position - boss.transform.position;
        move = Vector3.ClampMagnitude(move, 1);
        move.y = 0;
        
        Vector3 finalMove = move * boss.speed / 1.5f;

        if (Vector3.Distance(boss.transform.position, boss.currentTarget.transform.position) > 8f)
        {
            boss.controller.Move(finalMove * Time.deltaTime);
        }
        
        HandFlamer();
    }

    private void HandFlamer()
    {
        _spawnTimer -= Time.deltaTime;
        
        if (_spawnTimer <= 0f)
        {
            boss.HandFlamer(0.5f);
            _spawnTimer =  _spawnInterval;
        }
    }
}