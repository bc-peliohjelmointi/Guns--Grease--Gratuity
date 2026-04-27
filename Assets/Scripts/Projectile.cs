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


        // Try parent first
        var player = collision.gameObject.GetComponentInParent<StarterAssets.FirstPersonController>();

        // If not found, try children
        if (player == null)
        {
            player = collision.gameObject.GetComponentInChildren<StarterAssets.FirstPersonController>();
        }

        if (player != null)
        {
            player.TakeDamage(damage);

            var delivery = player.GetComponent<DeliverySystem>();
            if (delivery != null && delivery.hasPackage)
            {
                delivery.TakeDamage(deliveryDamage);
            }

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