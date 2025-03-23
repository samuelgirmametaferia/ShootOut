using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuUI;
    public GameObject CrossHairUI;
    public GunWeaponSystem GunWeapon;
    public void ManageMenu(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            PauseMenuUI.SetActive(true);
            CrossHairUI.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GunWeapon.enabled = false;
            Time.timeScale = 0f;

        }
        else
        {
            PauseMenuUI.SetActive(false);
            CrossHairUI.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            GunWeapon.enabled = true;
        }
    }
}
