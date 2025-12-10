using UnityEngine;

public class EnemyGunProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 30f;
    public float shootCooldown = 1f;

    private float lastShotTime;

    public void TryShoot(Transform target)
    {
        // cooldown
        if (Time.time < lastShotTime + shootCooldown)
            return;

        lastShotTime = Time.time;

        if (projectilePrefab == null || firePoint == null)
            return;

        // suunta pelaajaan
        Vector3 dir = (target.position + Vector3.up * 1.2f - firePoint.position).normalized;

        // luodaan projektiili
        GameObject proj = GameObject.Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        // annetaan velocity
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
    }
}
