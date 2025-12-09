using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 5f;
    public float damage = 10f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        DeliverySystem delivery = other.GetComponent<DeliverySystem>();
        if (delivery != null && delivery.hasPackage)
        {
            delivery.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
