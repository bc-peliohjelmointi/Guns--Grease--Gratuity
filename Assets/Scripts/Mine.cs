using UnityEngine;

public class Mine : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageToPlayer = 25f;
    public float damageToDelivery = 30f;
    public float explosionRadius = 3f;

    [Header("Trigger Settings")]
    public float triggerDelay = 0.5f;
    public bool oneTimeUse = true;

    [Header("Shoot-to-Destroy Settings")]
    public float mineHealth = 1f;       // How many bullets it takes (1 = one shot kill)
    public bool explodeWhenShot = true; // true = shot triggers explosion, false = silent destroy

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
    private float currentHealth;

    private void Start()
    {
        currentHealth = mineHealth;
    }

    // ?? Called by your bullet/projectile script on hit ?????????????????????
    public void TakeDamage(float damage)
    {
        if (hasExploded || isTriggered) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            if (explodeWhenShot)
            {
                // Triggers full explosion — damages player/packages in radius
                isTriggered = true;
                if (audioSource && triggerSound)
                    audioSource.PlayOneShot(triggerSound);
                Invoke(nameof(Explode), triggerDelay);
            }
            else
            {
                // Silent destroy — mine is disarmed, no explosion
                DestroyMine();
            }
        }
    }

    // ?? Player steps on mine ???????????????????????????????????????????????
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered && !hasExploded)
        {
            Debug.Log("Mine triggered by player!");
            isTriggered = true;

            if (audioSource && triggerSound)
                audioSource.PlayOneShot(triggerSound);

            Invoke(nameof(Explode), triggerDelay);
        }
    }

    // ?? Explosion ??????????????????????????????????????????????????????????
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log("Mine exploded!");

        if (audioSource && explosionSound)
            audioSource.PlayOneShot(explosionSound);

        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectLifetime);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            // Damage player via PlayerHealth
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
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

        if (oneTimeUse)
            Destroy(gameObject, explosionSound != null ? explosionSound.length : 0.5f);
        else
        {
            isTriggered = false;
            hasExploded = false;
            currentHealth = mineHealth;
        }
    }

    // ?? Silent disarm ??????????????????????????????????????????????????????
    private void DestroyMine()
    {
        Debug.Log("Mine disarmed by bullet!");
        // Optionally spawn a small puff effect here instead of explosion
        Destroy(gameObject);
    }

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