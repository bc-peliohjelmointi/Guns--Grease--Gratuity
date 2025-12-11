using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Osuuko pelaajaan?
        if (collision.collider.CompareTag("Player"))
        {
            DeliverySystem delivery = collision.collider.GetComponent<DeliverySystem>();
            if (delivery != null)
            {
                delivery.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
