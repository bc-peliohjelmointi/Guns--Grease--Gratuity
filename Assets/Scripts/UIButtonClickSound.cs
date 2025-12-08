using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlayClick);
    }

    void PlayClick()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
