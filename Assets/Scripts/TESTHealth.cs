using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    float current;

    void Awake() { current = maxHealth; }

    public void TakeDamage(float amount)
    {
        current -= amount;
        if (current <= 0) Die();
    }

    void Die()
    {
        // simple: destroy or disable; expand for ragdoll etc.
        Destroy(gameObject);
    }
}
