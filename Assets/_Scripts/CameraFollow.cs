using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    [SerializeField]
    private GameObject m_camera;
    Vector3 m_offsetFromPlayer;
    Vector3 m_originPosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner == false)
            return;
        m_camera = GameObject.FindGameObjectWithTag("CameraFollow");
        m_originPosition = m_camera.transform.position;
        m_offsetFromPlayer = transform.position - m_camera.transform.position;
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
        }
    }
}