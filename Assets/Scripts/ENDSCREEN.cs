using UnityEngine;
using UnityEngine.SceneManagement;

public class ENDSCREEN : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene("Main");
    }
}
