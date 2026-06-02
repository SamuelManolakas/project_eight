using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[CreateAssetMenu (menuName = "Scriptables/Audio/AVA")]
public class AvatarAudio : ScriptableObject
{
    [SerializeField] private EventReference playerFootstepEvent;


    public void PlayFootstepAudioPlay(GameObject feetObj)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(playerFootstepEvent);
        
        RuntimeManager.AttachInstanceToGameObject(eventInstance, feetObj.transform, feetObj.GetComponent<Rigidbody>());
        
        eventInstance.start();
        eventInstance.release();
    }
}