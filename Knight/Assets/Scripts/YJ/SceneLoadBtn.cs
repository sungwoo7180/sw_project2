using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadBtn : MonoBehaviour
{
    public void StartClick(string sceneName)
    {
       SceneManager.LoadScene("play");
    }
}
