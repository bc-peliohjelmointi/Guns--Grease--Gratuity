using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float lifetime = 6f;
    public float damage = 10f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Jos osuu pelaajaan (DeliverySystem), tee vahinko vain jos paketti on mukana
        DeliverySystem delivery = other.GetComponent<DeliverySystem>();
        if (delivery != null)
        {
            if (delivery.hasPackage)
                delivery.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // Voit lisätä muita törmäystapauksia (esim. ympäristö)
        // Jos haluat, että projektiili tuhoutuu seinään:
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
