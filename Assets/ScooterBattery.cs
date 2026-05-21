using UnityEngine;

public class ScooterBattery : MonoBehaviour
{
    public float chargeAmount = 30.0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupClip;

    private void OnTriggerEnter(Collider other)
    {
        ScooterMount player = other.GetComponent<ScooterMount>();

        if (player != null && !player.isMounted && !player.HasBattery())
        {
            player.GetBattery(chargeAmount);

            PlayPickupSound();

            Destroy(gameObject, 0.1f);
        }
    }

    private void PlayPickupSound()
    {
        if (audioSource != null && pickupClip != null)
        {
            // detach so it survives destroy
            audioSource.transform.parent = null;

            audioSource.PlayOneShot(pickupClip);

            Destroy(audioSource.gameObject, pickupClip.length);
        }
    }
}