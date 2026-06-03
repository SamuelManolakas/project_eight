using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[CreateAssetMenu(menuName = "Scriptables/Audio/Enemy/Boss")]
public class BossAudio : ScriptableObject
{
    [SerializeField] private EventReference bossEngineEvent;
    [SerializeField] private EventReference bossSwingEvent;
    [SerializeField] private EventReference bossRangedEvent;
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
    public void BossSwingAudioPlay(GameObject PunchObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(bossSwingEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, PunchObj.transform, PunchObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossSwingEvent, PunchObj.transform.position);
        
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
    
    public void BossRangedAudioPlay(GameObject RangedObj)
    {

        EventInstance eventInstance = RuntimeManager.CreateInstance(bossRangedEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, RangedObj.transform, RangedObj.GetComponent<Rigidbody>());
        RuntimeManager.PlayOneShot(bossRangedEvent, RangedObj.transform.position);
        
        eventInstance.start();
        eventInstance.release();
    }
}
