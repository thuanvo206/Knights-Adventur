using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("=== BOOTLOADER AWAKE === Scene: " + SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        Debug.Log("=== BOOTLOADER START === Scene: " + SceneManager.GetActiveScene().name);
    }
}