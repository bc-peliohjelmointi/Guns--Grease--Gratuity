using UnityEngine;
using System.Collections;

public class MenuMusicPLay : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(StartMusic());
    }

    IEnumerator StartMusic()
    {
        yield return null;   // Wait 1 frame so audio backend finishes initializing
        GetComponent<AudioSource>().Play();
    }
}
