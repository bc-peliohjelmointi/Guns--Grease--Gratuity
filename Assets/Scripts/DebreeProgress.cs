using UnityEngine;

public class DebreeProgress : MonoBehaviour
{
    [Range(0f, 1f)]
    public float progress = 0f;
    public AudioSource audioSource;
    public AudioClip debreeSound;
}