using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : NetworkBehaviour
{
    [SerializeField]
    private GameObject m_camera;
    public Transform target;
    public Transform cameraPivotTarget;
    Vector3 m_offsetFromPlayer;
    Vector3 m_originPosition;

    public float lookSpeed;
    public float followSpeed;
    public float pivotSpeed;

    private float defaultPosition;
    private float lookAngle;
    private float pivotAngle;
    public float minimumPivot;
    public float maximumPivot;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner == false)
            return;
        m_camera = GameObject.FindGameObjectWithTag("CameraFollow");
        m_originPosition = m_camera.transform.position;
        m_offsetFromPlayer = transform.position - m_camera.transform.position;
        target = transform;
        cameraPivotTarget = transform;
    }

    public override void OnNetworkDespawn()
    {
        if(IsOwner && m_camera != null)
        {
            m_camera.transform.position = m_originPosition;
        }
        base.OnNetworkDespawn();
    }

    private void LateUpdate()
    {
        if(IsOwner && m_camera != null)
        {
            m_camera.transform.position = transform.position;
            //m_camera.transform.rotation = transform.rotation;
            
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        CameraRotation(context.ReadValue<Vector2>().magnitude, context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
    }

    private void CameraRotation(float delta, float mouseXInput, float mouseYInput)
    {
        lookAngle += (mouseYInput * lookSpeed) / delta; 
        pivotAngle -= (mouseXInput * pivotSpeed) / delta;
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);
        
        Vector3 rotation = Vector3.zero;
        rotation.y = lookAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        m_camera.transform.rotation = targetRotation;
        
        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        
        targetRotation = Quaternion.Euler(rotation);
        //cameraPivotTarget.localRotation = targetRotation;
    }
}