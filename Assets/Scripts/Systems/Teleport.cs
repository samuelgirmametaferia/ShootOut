using UnityEngine;
using UnityEngine.SceneManagement;
public class Teleport : MonoBehaviour
{
    [SerializeField] public int EndGame = 2;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            SceneManager.LoadScene(EndGame);
        }
    }
    public void LoadScene(int next)
    {
        Debug.Log("Hello!?");
        SceneManager.LoadScene(next);
    }
}
