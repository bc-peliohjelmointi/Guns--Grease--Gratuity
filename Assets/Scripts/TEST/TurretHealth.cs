using UnityEngine;

public class TurretHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public System.Action onHit;
    public System.Action onDeath;
    public GameObject smokePuffPrefab; // assign in Inspector

    [Header("Effects")]
    public GameObject deathEffect; // assign in Inspector

    void Start()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        onHit?.Invoke();  // play hit SFX

        if (currentHealth <= 0)
        {
            onDeath?.Invoke(); // play death SFX
            if (deathEffect != null)
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            if (smokePuffPrefab != null)
                Instantiate(smokePuffPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
