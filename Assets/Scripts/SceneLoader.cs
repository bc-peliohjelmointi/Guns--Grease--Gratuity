using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Call this method from your UI Button
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
