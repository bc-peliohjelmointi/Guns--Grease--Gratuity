using UnityEngine;

public class TurretAI : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 20f;
    public float rotationSpeed = 8f;
    public float fireRate = 0.5f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSFX;
    public AudioClip hitSFX;
    public AudioClip deathSFX;

    private Transform player;
    private float fireCooldown = 0f;
    private TurretHealth health;

    void Start()
    {
        // Random rotation on spawn (Y only, so turret stays upright)
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        health = GetComponent<TurretHealth>();

        if (health != null)
        {
            health.onHit += PlayHitSound;
            health.onDeath += PlayDeathSound;
        }
    }


    void Update()
    {
        if (!DeliveryIsActive()) return;               // Only shoot during deliveries
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            RotateTowardsPlayer();
            Shoot();
        }

        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;
    }

    // -------------------------
    // DELIVERY CHECK
    // -------------------------
    bool DeliveryIsActive()
    {
        DeliverySystem ds = FindObjectOfType<DeliverySystem>();
        return ds != null && ds.hasActiveOrder;
    }

    // -------------------------
    // TURRET BEHAVIOR
    // -------------------------
    void RotateTowardsPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (fireCooldown > 0) return;

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (shootSFX && audioSource)
            audioSource.PlayOneShot(shootSFX);

        fireCooldown = fireRate;
    }

    // -------------------------
    // SOUND EVENTS
    // -------------------------
    void PlayHitSound()
    {
        if (hitSFX && audioSource)
            audioSource.PlayOneShot(hitSFX);
    }

    void PlayDeathSound()
    {
        if (deathSFX && audioSource)
            audioSource.PlayOneShot(deathSFX);
    }
}
