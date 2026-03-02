using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float deliveryDamage = 15f;
    public float speed = 20f;
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // --------------------
        // PLAYER HIT
        // --------------------
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player HP
            StarterAssets.FirstPersonController player =
                collision.gameObject.GetComponent<StarterAssets.FirstPersonController>();

            if (player != null)
                player.TakeDamage(damage);

            // Delivery HP (only if carrying package)
            DeliverySystem delivery =
                collision.gameObject.GetComponent<DeliverySystem>();

            if (delivery != null && delivery.hasPackage)
                delivery.TakeDamage(deliveryDamage);

            Destroy(gameObject);
            return;
        }

        // --------------------
        // ENVIRONMENT HIT
        // --------------------
        if (!collision.collider.isTrigger && !collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}