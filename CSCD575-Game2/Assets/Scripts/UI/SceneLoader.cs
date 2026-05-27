using UnityEngine;
using UnityEngine.SceneManagement; // <-- this is so we can load scenes
using System; // <-- this is so we can catch any problems and exit gracefully

public class SceneLoader : MonoBehaviour
{
    // This is for when we know the scene name and don't know the build order/scene index number
    public void LoadByName(string _scene)
    {
        try
        {
            SceneManager.LoadScene(_scene, LoadSceneMode.Single);
        }catch(Exception exc)
        {
            Debug.Log(exc);
        }
            
    }

    // This is for when we know the scene index number, but not the name
    public void LoadByIndex(int i)
    {
        try
        {
            SceneManager.LoadScene(i, LoadSceneMode.Single);
        }catch(Exception exc)
        {
            Debug.Log(exc);
        }
    }
}
