using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameAndLoadScene : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        UFE.EndGame();
        SceneManager.LoadScene(sceneName);
    }
}
