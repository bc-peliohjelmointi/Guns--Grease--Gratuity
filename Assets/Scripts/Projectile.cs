using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;


    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Enemy ottaa vahinkoa
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Player / Delivery ottaa vahinkoa
        DeliverySystem delivery = other.GetComponent<DeliverySystem>();
        if (delivery != null)
        {
            delivery.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }
}
