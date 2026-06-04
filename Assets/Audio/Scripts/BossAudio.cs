using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[CreateAssetMenu(menuName = "Scriptables/Audio/Enemy/Boss")]
public class BossAudio : ScriptableObject
{
    [SerializeField] private EventReference bossEngineEvent;
    [SerializeField] private EventReference bossSweepEvent;
    [SerializeField] private EventReference bossStrikeEvent;
    [SerializeField] private EventReference bossSpinEvent;
    [SerializeField] private EventReference bossGrabEvent;
    [SerializeField] private EventReference boss4HcEvent;
    [SerializeField] private EventReference boss3HcEvent;
    [SerializeField] private EventReference bossRangedEvent;
    [SerializeField] private EventReference bossChestEvent;
    [SerializeField] private EventReference bossNukeEvent;
    [SerializeField] private EventReference bossDamageEvent;
    [SerializeField] private EventReference bossDieEvent;


    private EventInstance engineInstance;

    public void PlayEngineAudioPlay(GameObject feetObj, int state, int crash)
    {
        // If the instance hasn't been created yet (or was destroyed), create it.
        if (!engineInstance.isValid())
        {
            engineInstance = RuntimeManager.CreateInstance(bossEngineEvent);
            RuntimeManager.AttachInstanceToGameObject(engineInstance, feetObj.transform, feetObj.GetComponent<Rigidbody>());
            engineInstance.start();
        }
        
        // Changes value of parameters in FMOD
        if (state == 0)
        {
            engineInstance.setParameterByName("Boss Engine", state);
        }
        else if (state == 1)
        {
            engineInstance.setParameterByName("Boss Engine", state);
        }
        else if (state == 2)
        {
            engineInstance.setParameterByName("Boss Engine", state);
            if (state == 3)
            {
                engineInstance.setParameterByName("Charge", crash);
                engineInstance.setParameterByName("Boss Engine", state);
            }
        }
    }
    public void BossSweepAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossSweepEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossSweepEvent, PunchObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossStrikeAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossStrikeEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossStrikeEvent, PunchObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossSpinAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossSpinEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossSpinEvent, PunchObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossGrabAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossGrabEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossGrabEvent, PunchObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void Boss4HcAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(boss4HcEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(boss4HcEvent, PunchObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void Boss3HcAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(boss3HcEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(boss3HcEvent, PunchObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossRangedAudioPlay(GameObject RangedObj)
    {

        EventInstance eventInstance = RuntimeManager.CreateInstance(bossRangedEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, RangedObj.transform, RangedObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossRangedEvent, RangedObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossChestAudioPlay(GameObject RangedObj)
    {

        EventInstance eventInstance = RuntimeManager.CreateInstance(bossChestEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, RangedObj.transform, RangedObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossChestEvent, RangedObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossNukeAudioPlay(GameObject RangedObj)
    {

        EventInstance eventInstance = RuntimeManager.CreateInstance(bossNukeEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, RangedObj.transform, RangedObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossNukeEvent, RangedObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossDieAudioPlay(GameObject BossObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossDieEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, BossObj.transform, BossObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossDieEvent, BossObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void BossDamageAudioPlay(GameObject BossObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossDamageEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, BossObj.transform, BossObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossDamageEvent, BossObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
}
