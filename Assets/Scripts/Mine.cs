using UnityEngine;

public class MineTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageToPlayer = 25f;
    public float damageToDelivery = 30f;
    public float explosionRadius = 3f;

    [Header("Trigger Settings")]
    public float triggerDelay = 0.5f; // Time before explosion after trigger
    public bool oneTimeUse = true; // Destroy after exploding?

    [Header("Visual Effects")]
    public GameObject explosionEffect;
    public float effectLifetime = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip triggerSound;
    public AudioClip explosionSound;

    [Header("Debug")]
    public bool showRadius = true;

    private bool isTriggered = false;
    private bool hasExploded = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if player stepped on mine
        if (other.CompareTag("Player") && !isTriggered && !hasExploded)
        {
            Debug.Log("Mine triggered by player!");
            isTriggered = true;

            // Play trigger sound
            if (audioSource && triggerSound)
                audioSource.PlayOneShot(triggerSound);

            // Start explosion countdown
            Invoke(nameof(Explode), triggerDelay);
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log("Mine exploded!");

        // Play explosion sound
        if (audioSource && explosionSound)
            audioSource.PlayOneShot(explosionSound);

        // Spawn visual effect
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectLifetime);
        }

        // Find all objects in explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hitColliders)
        {
            // Damage player
            StarterAssets.FirstPersonController player = hit.GetComponent<StarterAssets.FirstPersonController>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
                Debug.Log($"Mine dealt {damageToPlayer} damage to player!");
            }

            // Damage delivery package
            DeliverySystem delivery = hit.GetComponent<DeliverySystem>();
            if (delivery != null && delivery.hasPackage)
            {
                delivery.TakeDamage(damageToDelivery);
                Debug.Log($"Mine dealt {damageToDelivery} damage to delivery!");
            }
        }

        // Destroy mine if one-time use
        if (oneTimeUse)
        {
            Destroy(gameObject, explosionSound != null ? explosionSound.length : 0.5f);
        }
        else
        {
            // Reset for reuse
            isTriggered = false;
            hasExploded = false;
        }
    }

    // Visualize explosion radius in editor
    private void OnDrawGizmos()
    {
        if (showRadius)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}