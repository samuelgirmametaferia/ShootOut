using UnityEngine;
using UnityEngine.InputSystem;

public class MapSystem : MonoBehaviour
{
    [SerializeField] private GameObject MapCamera;
    [SerializeField] private Transform Player;
    [SerializeField] private GameObject PlayerCamera;
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject MapViewUI;
    [SerializeField] private float MapViewDistance = 100f;

    private void ShowMap()
    {
        MapCamera.SetActive(true);
        MapCamera.transform.position = Player.transform.position + new Vector3(0f,MapViewDistance,0f);
        MapViewUI.SetActive(true);
        PlayerCamera.SetActive(false);
        PlayerUI.SetActive(false);
    }
    private void HideMap()
    {
        MapCamera.SetActive(false);
        MapViewUI.SetActive(false);
        PlayerCamera.SetActive(true);
        PlayerUI.SetActive(true);
    }

    public void HandleMap(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            ShowMap();
        }
        else
        {
            HideMap();
        }
    }

}
