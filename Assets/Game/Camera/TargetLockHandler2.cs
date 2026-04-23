using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetLockHandler2 : NetworkBehaviour
{
    [Header("ScriptReferences")] [SerializeField]
    private PlayerBehaviour player;
    
    [SerializeField] private Animator animator;

    [Header("Settings")] 
    [SerializeField] [Range(5, 50)] private float enemyDetectionRange;
    [SerializeField] private Transform currentTarget;

    [Header("Cameras")] 
    [SerializeField] private GameObject freeLookCamera;
    [SerializeField] private GameObject targetLockCamera;
    
    private void Start()
    {
        animator = GameObject.FindGameObjectWithTag("CameraAnimator").GetComponent<Animator>();   
        SwitchCams();
    }

    private int _shouldSwitch;
    
    public void OnTargetLock(InputAction.CallbackContext context)
    {
        animator = GameObject.FindGameObjectWithTag("CameraAnimator").GetComponent<Animator>();   
        SwitchCams();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner == false)
            return;
        freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCamera");
        freeLookCamera.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        
        targetLockCamera = GameObject.FindGameObjectWithTag("TargetLockCamera");
        targetLockCamera.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        
        animator = GameObject.FindGameObjectWithTag("CameraAnimator").GetComponent<Animator>();   
    }

    private void SwitchCams()
    {
        if (IsOwner && freeLookCamera != null)
        {
            CinemachineVirtualCamera cinemachineFreeLookCamera = freeLookCamera.GetComponent<CinemachineVirtualCamera>();
            CinemachineVirtualCamera cinemachineLockCamera = targetLockCamera.GetComponent<CinemachineVirtualCamera>();

            if (_shouldSwitch == 0)
            {
                cinemachineLockCamera.ForceCameraPosition(cinemachineFreeLookCamera.State.FinalPosition,
                    cinemachineFreeLookCamera.State.FinalOrientation);
                animator.Play("TargetLockCamera");
                _shouldSwitch = 1;
            }
            else if (_shouldSwitch == 1)
            {
                cinemachineFreeLookCamera.ForceCameraPosition(cinemachineLockCamera.State.FinalPosition,
                    cinemachineLockCamera.State.FinalOrientation);
                animator.Play("FreeLookCamera");
                _shouldSwitch = 0;
            }
        }
    }
}
