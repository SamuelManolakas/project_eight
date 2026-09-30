using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE; // FMODUnity has its own STOP_MODE too; EventInstance.stop() takes this one

/// <summary>
/// Each player's own looping footstep sound. Runs on every machine and decides locally whether this
/// player is walking from animator values the NetworkAnimator already syncs (the "speed" parameter,
/// or the Strafe layer's weight while locked on), so everyone hears everyone's footsteps in the right
/// place without extra network messages.
/// </summary>
[RequireComponent(typeof(PlayerBehaviour))]
public class PlayerFootsteps : MonoBehaviour
{
    [Tooltip("Above this animator speed (or Strafe layer weight) the player counts as walking.")]
    [SerializeField] private float movingThreshold = 0.1f;

    private PlayerBehaviour _player;
    private int _strafeLayer = -1;
    private EventInstance _footsteps;
    private bool _isPlaying;

    private void Awake() => _player = GetComponent<PlayerBehaviour>();

    private void Start()
    {
        if (_player.animator != null)
            _strafeLayer = _player.animator.GetLayerIndex("Strafe");
    }

    private void Update()
    {
        bool moving = IsWalking();
        if (moving == _isPlaying) return;

        _isPlaying = moving;
        if (moving) StartFootsteps();
        else StopFootsteps(STOP_MODE.ALLOWFADEOUT); // let reverb and tails ring out
    }

    private bool IsWalking()
    {
        Animator animator = _player.animator;
        if (animator == null || !animator.isActiveAndEnabled) return false;

        if (animator.GetFloat(AnimatorParams.Speed) > movingThreshold) return true;
        return _strafeLayer >= 0 && animator.GetLayerWeight(_strafeLayer) > movingThreshold; // locked-on strafing
    }

    private void StartFootsteps()
    {
        AvatarAudio audio = _player.PlayerAudioScriptableObject;
        if (audio == null || audio.FootstepEvent.IsNull) return;

        if (!_footsteps.isValid())
        {
            GameObject feet = _player.audioSource != null ? _player.audioSource : gameObject;
            _footsteps = RuntimeManager.CreateInstance(audio.FootstepEvent);
            RuntimeManager.AttachInstanceToGameObject(_footsteps, feet.transform, feet.GetComponent<Rigidbody>());
        }

        _footsteps.setParameterByName("Moving", 1);
        _footsteps.start();
    }

    private void StopFootsteps(STOP_MODE mode)
    {
        if (!_footsteps.isValid()) return;

        _footsteps.setParameterByName("Moving", 0);
        _footsteps.stop(mode);
    }

    private void OnDisable()
    {
        StopFootsteps(STOP_MODE.IMMEDIATE);
        _isPlaying = false;
    }

    private void OnDestroy()
    {
        if (!_footsteps.isValid()) return;

        _footsteps.stop(STOP_MODE.IMMEDIATE);
        _footsteps.release();
        _footsteps.clearHandle();
    }
}
