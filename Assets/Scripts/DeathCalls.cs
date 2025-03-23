using UnityEngine;
using UnityEngine.InputSystem;

public class DeathCalls : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private GunWeaponSystem gunWeaponSystem;
    [SerializeField] private PlayerInput inputSystem;
    public void DeathCall()
    {

        Time.timeScale = 0.3f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cameraMovement.enabled = false;
        playerMovement.enabled = false;
        gunWeaponSystem.enabled = false;
        inputSystem.enabled = false;
    }
}
