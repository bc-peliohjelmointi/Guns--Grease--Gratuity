using UnityEngine;


public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float deliveryDamage = 15f;
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if hit player
        StarterAssets.FirstPersonController player = collision.gameObject.GetComponent<StarterAssets.FirstPersonController>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }

        // Check if player has delivery package
        DeliverySystem delivery = collision.gameObject.GetComponent<DeliverySystem>();
        if (delivery != null && delivery.hasPackage)
        {
            delivery.TakeDamage(deliveryDamage);
        }

        // Destroy projectile on impact
        Destroy(gameObject);
    }
}