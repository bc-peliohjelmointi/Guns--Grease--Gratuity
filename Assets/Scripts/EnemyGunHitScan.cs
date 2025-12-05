using UnityEngine;

public class EnemyGunHitscan : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 10f;
    public float fireRate = 1f;
    public float range = 50f;
    public LayerMask hitMask;
    public Transform firePoint;
    public GameObject muzzleFlash;
    public float flashTime = 0.1f;

    private float nextFireTime = 0f;

    public void TryShoot(DeliverySystem target)
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + 1f / fireRate;

        Shoot(target);
    }

    private void Shoot(DeliverySystem target)
    {
        if (firePoint == null) return;

        // Muzzle flash
        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
            Destroy(flash, flashTime);
        }

        // Hitscan Raycast
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range, hitMask))
        {
            DeliverySystem delivery = hit.collider.GetComponent<DeliverySystem>();
            if (delivery != null && delivery.hasPackage)
            {
                delivery.TakeDamage(damage);
            }
        }
    }
}
