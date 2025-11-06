using UnityEngine;
using UnityEngine.SceneManagement;

public class ENDSCREEN : MonoBehaviour
{
    // on start
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }
}
