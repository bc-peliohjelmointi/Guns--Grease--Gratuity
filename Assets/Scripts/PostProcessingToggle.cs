using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingToggle : MonoBehaviour
{
    public Volume volume; // Drag your PP Volume here
    public bool enableOnEnter = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            volume.enabled = enableOnEnter;
    }
}

