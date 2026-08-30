using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu(menuName = "Scriptables/Audio/AVA")]
public class AvatarAudio : ScriptableObject
{
    [SerializeField] private EventReference playerFootstepEvent;
    [SerializeField] private EventReference playerRollEvent;
    [SerializeField] private EventReference playerSSLightAttackEvent;
    [SerializeField] private EventReference playerSSHeavyAttackEvent;
    [SerializeField] private EventReference playerGSLightAttackEvent;
    [SerializeField] private EventReference playerGSHeavyAttackEvent;
    [SerializeField] private EventReference playerBolterAttackEvent;
    [SerializeField] private EventReference playerHealEvent;
    
    // 1. Store the instance here so it persists between method calls
    private EventInstance footstepInstance;

    public void PlayFootstepAudioPlay(GameObject feetObj, int moving)
    {
        // 2. If the instance hasn't been created yet (or was destroyed), create it.
        if (!footstepInstance.isValid())
        {
            footstepInstance = RuntimeManager.CreateInstance(playerFootstepEvent);
            RuntimeManager.AttachInstanceToGameObject(footstepInstance, feetObj.transform, feetObj.GetComponent<Rigidbody>());
        }
         
        // 3. Update the parameter on the persistent instance
        footstepInstance.setParameterByName("Moving", moving);

        // 4. Start or Stop based on the integer
        if (moving == 1)
        {
            footstepInstance.start();
        }
        else if (moving == 0)
        {
            // Stop gracefully, letting reverb and tails ring out.
            // Notice we do NOT release() it here, so it's ready for the next footstep.
            footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    // 5. ALWAYS clean up FMOD instances in ScriptableObjects when the game quits
    private void OnDisable()
    {
        if (footstepInstance.isValid())
        {
            footstepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            footstepInstance.release();
        }
    }

    public void PlayRollAudioPlay(GameObject feetObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerRollEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, feetObj.transform, feetObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
        
    }

    public void PlaySSLightAudioPlay(GameObject weaponObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerSSLightAttackEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, weaponObj.transform, weaponObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void PlaySSHeavyAudioPlay(GameObject weaponObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerSSHeavyAttackEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, weaponObj.transform, weaponObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void PlayGSLightAudioPlay(GameObject weaponObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerGSLightAttackEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, weaponObj.transform, weaponObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void PlayGSHeavyAudioPlay(GameObject weaponObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerGSHeavyAttackEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, weaponObj.transform, weaponObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }
    
    public void PlayPewPewAudioPlay(GameObject weaponObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerBolterAttackEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, weaponObj.transform, weaponObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }

    public void PlayHealAudioPlay(GameObject playerObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerHealEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, playerObj.transform, playerObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }
}