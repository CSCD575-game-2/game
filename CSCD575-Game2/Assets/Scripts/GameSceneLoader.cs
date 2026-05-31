using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    public void GoToTitleScreen()
    {
        SceneManager.LoadScene("Main-Menu");
    }
}
