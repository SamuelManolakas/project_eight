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

    // The footstep loop is played per player by PlayerFootsteps (a looping instance can't live on this
    // shared asset: all players on a machine would share one sound, and it would outlive the scene).
    public EventReference FootstepEvent => playerFootstepEvent;

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