using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuButtons : MonoBehaviour
{
    public AudioClip ButtonPresssfx;
    public AudioSource audioSource;
    public Button startButton;
    public Scene sceneName;


    void Start()
    {
        PlaySound(ButtonPresssfx);
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
