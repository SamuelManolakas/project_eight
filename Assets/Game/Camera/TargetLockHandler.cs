using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetLockHandler : NetworkBehaviour
{
    [Header("ScriptReferences")] [SerializeField]
    private PlayerBehaviour player;

    //public TargetMe targetMe;
    [SerializeField] private Animator animator;

    [Header("Settings")] 
    public bool activeTarget;
    [SerializeField] [Range(5, 50)] private float enemyDetectionRange;
    [ReadOnly] [SerializeField] private List<Transform> nearbyTargets;
    [SerializeField] private Transform currentTarget;

    [Header("Cameras")] 
    [SerializeField] private GameObject freeLookCamera;
    [SerializeField] private GameObject targetLockCamera;

    [SerializeField] private LayerMask enemyLayer;

    public static Action<bool> OnTargetLock;

    private void Start()
    {
        animator = GameObject.FindGameObjectWithTag("CameraAnimator").GetComponent<Animator>();   
        SwitchCams();
        //targetMe.graphics.SetActive(false);
    }

    private float switchLockCooldownTime = 0.5f;
    private float switchAfterTime = 0;

    private bool shouldSwitch;
    public void OnTargetLocked(InputAction.CallbackContext context)
    {
        animator = GameObject.FindGameObjectWithTag("CameraAnimator").GetComponent<Animator>();   
        targetLockCamera.GetComponent<CinemachineCamera>().LookAt = currentTarget;
        TargetLock(!activeTarget);
        //SwitchCams();
        if (shouldSwitch)
        {
            SwitchTarget(false);
            shouldSwitch = true;
        }
        else
        {
            SwitchTarget(true);
            shouldSwitch = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        freeLookCamera = GameObject.FindGameObjectWithTag("FreeLookCamera");
        freeLookCamera.GetComponent<CinemachineCamera>().Follow = player.transform;
        
        targetLockCamera = GameObject.FindGameObjectWithTag("TargetLockCamera");
        targetLockCamera.GetComponent<CinemachineCamera>().Follow = player.transform;
        targetLockCamera.GetComponent<CinemachineCamera>().LookAt = currentTarget.transform;
        
        animator = GameObject.FindGameObjectWithTag("CameraAnimator").GetComponent<Animator>();   
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (activeTarget)
        {
            float distance = Vector3.Distance(player.transform.position, currentTarget.position);
            if (distance > enemyDetectionRange + enemyDetectionRange/2)
            {
                TargetLock(false);
            }
        }
    }

    private void TargetLock(bool boolVal)
    {
        Debug.Log("TagetLock called");
        if (boolVal)
        {
            CollectTargetsAndGetMostInFrontTarget(out Transform targetMostInFront);
            currentTarget = targetMostInFront;

            if (targetMostInFront != null)
            {
                //targetMe.SetTargetPos(targetMostInFront);
                //targetMe.graphics.SetActive(true);
                activeTarget = true;
                SwitchCams();
            }
        }
        else
        {
            nearbyTargets.Clear();
            //targetMe.graphics.SetActive(false);
            activeTarget = false;
            SwitchCams();
        }

        OnTargetLock?.Invoke(activeTarget);
    }

    private void CollectTargetsAndGetMostInFrontTarget(out Transform targetMostInFront)
    {
        List<Transform> foundTargets = new List<Transform>();

        Collider[] enemyColliders = Physics.OverlapSphere(player.transform.position, enemyDetectionRange);
        foreach (Collider collider in enemyColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                GameObject boss = collider.gameObject;

                if (boss != null)
                {
                    Vector3 dirToEnemy = (collider.transform.position - player.transform.position).normalized;
                    float dot = Vector3.Dot(GetActiveCamera().transform.position, dirToEnemy);
                    if (dot > 0.5f)
                    {
                        bool hitSomthing = Physics.Raycast(player.transform.position, dirToEnemy, out RaycastHit hit,
                            enemyDetectionRange);
                        if (hitSomthing)
                        {
                            if (hit.collider == collider)
                            {
                                foundTargets.Add(collider.transform);
                            }
                        }
                        else
                        {
                            foundTargets.Add(collider.transform);
                        }
                    }
                }
            }
        }

        if (foundTargets.Count <= 0)
        {
            targetMostInFront = null;
            nearbyTargets = null;
            return;
        }

        foundTargets.Sort ((Transform enemy, Transform enemy2) =>
        {
            Vector3 dirToEnemy = (enemy.position - player.transform.position).normalized;
            Vector3 dirToEnemy2 = (enemy2.position - player.transform.position).normalized;

            float crossEnemy = Vector3.Cross(GetActiveCamera().transform.forward, dirToEnemy).y;
            float crossEnemy2 = Vector3.Cross(GetActiveCamera().transform.position, dirToEnemy2).y;

            return crossEnemy.CompareTo(crossEnemy2);
        });
        
        nearbyTargets = foundTargets;

        float maxDot = -1;
        int mostFrontTargetIndex = 0;
        for (int i = 0; i < foundTargets.Count; i++)
        {
            Vector3 dirToEnemy = (foundTargets[i].transform.position - player.transform.position).normalized;
            float dot = Vector3.Dot(GetActiveCamera().transform.forward, dirToEnemy);
            if (maxDot > dot)
            {
                maxDot = dot;
                mostFrontTargetIndex = i;
            }
        }
        
        targetMostInFront = nearbyTargets[mostFrontTargetIndex];
    }

    private void SwitchTarget(bool switchToLeft)
    {
        if(!activeTarget)
            return;
        CollectTargetsAndGetMostInFrontTarget(out Transform targetMostInFront);

        if (nearbyTargets.Count <= 0)
        {
            TargetLock(false);
            return;
        }
        
        int currentTargetIndex = GetTargetIndex(currentTarget);

        if (currentTargetIndex == -1)
        {
            currentTargetIndex = GetTargetIndex(targetMostInFront);
        }

        if (switchToLeft)
        {
            if (currentTargetIndex > 0)
            {
                currentTargetIndex -= 1;
            }
            else
            {
                currentTargetIndex = 0;
            }
        }
        else
        {
            if (currentTargetIndex < nearbyTargets.Count - 1)
            {
                currentTargetIndex += 1;
            }
            else
            {
                currentTargetIndex = nearbyTargets.Count - 1;
            }
        }
        
        currentTarget = nearbyTargets[currentTargetIndex];

        if (currentTarget != null)
        {
            CinemachineGroupFraming cinemachineLockCamGroupFraming = targetLockCamera.GetComponent<CinemachineGroupFraming>();
            //cinemachineLockCamGroupFraming.Damping = 1;
            //targetMe.SetTargetPos(currentTarget);
        }
    }

    private void SwitchCams()
    {
        Debug.Log("SwitchCams");
        CinemachineInputAxisController axisControllerFreeLook =
            freeLookCamera.GetComponent<CinemachineInputAxisController>();
        CinemachineCamera cinemachineFreeLookCamera = freeLookCamera.GetComponent<CinemachineCamera>();
        CinemachineCamera cinemachineLockCamera = targetLockCamera.GetComponent<CinemachineCamera>();
        CinemachineGroupFraming cinemachineLockCameraGroupFraming = targetLockCamera.GetComponent<CinemachineGroupFraming>();

        if (axisControllerFreeLook != null)
        {
            axisControllerFreeLook.enabled = !activeTarget;
        }

        if (activeTarget)
        {
            cinemachineLockCamera.ForceCameraPosition(cinemachineFreeLookCamera.State.GetFinalPosition(), cinemachineFreeLookCamera.State.GetFinalOrientation());
            animator.Play("TargetLockCamera");
        }
        else
        {
            //cinemachineLockCameraGroupFraming.Damping = 0;
            cinemachineFreeLookCamera.ForceCameraPosition(cinemachineLockCamera.State.GetFinalPosition(), cinemachineLockCamera.State.GetFinalOrientation());
            animator.Play("FreeLookCamera");
        }
    }

    private int GetTargetIndex(Transform Target)
    {
        return nearbyTargets.IndexOf(Target);
    }

    public GameObject GetActiveCamera()
    {
        if(targetLockCamera.GetComponent<CinemachineCamera>().IsLive) return targetLockCamera;
        if(freeLookCamera.GetComponent<CinemachineCamera>().IsLive) return freeLookCamera;
        
        return null;
    }
}
