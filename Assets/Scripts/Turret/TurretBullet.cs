using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public float speed = 40f;
    public int damage = 10;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the bullet hits the player
        if (other.CompareTag("Player"))
        {
            // Only apply damage if the player has a package
            var deliverySystem = other.GetComponent<DeliverySystem>();
            if (deliverySystem != null && deliverySystem.hasPackage)
            {
                deliverySystem.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
