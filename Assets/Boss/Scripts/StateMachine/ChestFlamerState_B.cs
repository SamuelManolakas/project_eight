using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ChestFlamerState_B : State_B
{
    public ChestFlamerState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public float sweepAngle = 45f;       // degrees each side
    public float sweepSpeed = 25f;       // degrees per second
    public float spawnInterval = 0.08f;  // seconds between flame spawns

    private float _currentAimAngle;
    private float _sweepStartAngle;
    private float _sweepEndAngle;
    private float _spawnTimer;
    private bool  _sweeping;
    
    public override void Enter()
    {
        boss.animator.Play("ChestFlamer");
        boss.StartCoroutine(Wait());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.BossChestAudioPlay(boss.audioSource);
            boss.bossAudioNetworker.TriggerChestAudio();
        }
        boss.bossEngineSound = 0;
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.PlayEngineAudioPlay(boss.audioSource, boss.bossEngineSound, boss.bossChargeCrashSound);
            boss.bossAudioNetworker.TriggerEngineAudio(boss.bossEngineSound, boss.bossChargeCrashSound);
        }
    }

    public override void Exit()
    {
        boss.StopAllCoroutines();
    }
    
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }

        if (_sweeping)
        {
            bool done = UpdateSweep();

            if (done)
            {
                boss.stateMachine.Transit(boss.movementState);
            } 
        }
    }

    private IEnumerator Wait()
    {
        boss.animator.Play("ChestFlamer");
        yield return new WaitForSeconds(1.5f);
        BeginSweep();
        
        yield return new WaitForSeconds(4.5f);
        //boss.stateMachine.Transit(boss.movementState);
    }
    
    // ---- Call this every Update() while sweeping ----
    // Returns true when the sweep is finished.
    void BeginSweep()
    {
        float playerAngle = GetAngleTowardPlayer();
        _sweepStartAngle = playerAngle - sweepAngle;
        _sweepEndAngle   = playerAngle + sweepAngle;
        _currentAimAngle = _sweepStartAngle;
        _spawnTimer      = 0f;
        _sweeping        = true;
    }
    
    // ---- Helpers ----
    // World-space Y-rotation angle pointing from this enemy toward the player.
    bool UpdateSweep()
    {
        _currentAimAngle += sweepSpeed * Time.deltaTime;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            Vector3 aimDir = AimAngleToDirection(_currentAimAngle);
            boss.Flamer(aimDir, 3f); // <-- plug in your own spawn call here
            _spawnTimer = spawnInterval;
        }

        if (_currentAimAngle >= _sweepEndAngle)
        {
            _currentAimAngle = _sweepEndAngle;
            boss.Flamer(AimAngleToDirection(_currentAimAngle), 3f); // final flame
            _sweeping = false;
            return true;  // sweep done
        }

        return false;
    }
    
    float GetAngleTowardPlayer()
    {
        Vector3 toPlayer = boss.currentTarget.transform.position - boss.transform.position;
        toPlayer.y = 0f;
        return Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;
    }

// Converts a world-space Y-rotation angle back into a flat direction vector.
    Vector3 AimAngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), -0.2f, Mathf.Cos(rad)).normalized;
    }
}