using UnityEngine;

public class EnemyGunProjectile : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Stats")]
    public float projectileSpeed = 30f;
    public float timeBetweenShots = 1.0f;
    private float lastShotTime = 0f;

    public void TryShoot(Transform target)
    {
        if (Time.time < lastShotTime + timeBetweenShots)
            return;

        if (firePoint == null || projectilePrefab == null || target == null)
            return;

        lastShotTime = Time.time;

        Shoot(target.position);
    }

    private void Shoot(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = direction * projectileSpeed;
    }
}
