using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MoveTo : MonoBehaviour
{
    public InputAction Tutorial;

    public int GoTo = 0;
    private void Update()
    {
        if(Tutorial.ReadValue<bool>())
        {
            SceneManager.LoadScene(GoTo);
        }
    }
}
