using UnityEngine;

public class TurretHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Effects")]
    public GameObject smokeDeathEffect; // expanding smoke sphere
    public AudioClip hitSound;
    public AudioClip deathSound;

    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;

        if (hitSound)
            audioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (deathSound)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        if (smokeDeathEffect)
            Instantiate(smokeDeathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
