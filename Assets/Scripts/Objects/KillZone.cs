using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SceneManager.LoadScene("Arena_Whitebox", LoadSceneMode.Additive);
        }
    }
}
