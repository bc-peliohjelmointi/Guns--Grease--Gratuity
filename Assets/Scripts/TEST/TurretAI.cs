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
    public AudioClip shootSound;

    private float fireCooldown = 0f;
    private Transform player;
    private AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        audioSource = GetComponent<AudioSource>();

        // Random Y rotation on spawn
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
    }

    void Update()
    {
        if (player == null) return;

        // Only shoot when a delivery is active
        if (PlayerStats.Instance.ordersLeft <= 0) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            RotateTowardsPlayer();
            Shoot();
        }

        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;
    }

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

        if (shootSound)
            audioSource.PlayOneShot(shootSound);

        fireCooldown = fireRate;
    }
}
