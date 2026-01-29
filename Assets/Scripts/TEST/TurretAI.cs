using UnityEngine;

public class TurretAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 20f;
    public float rotationSpeed = 8f;
    public float fireRate = 0.5f;
    public float aimHeight = 1.2f;

    [Header("Raycast Vision")]
    public LayerMask visionMask;     // Player + Walls
    public Transform eyePoint;       // Where raycast starts
    public Transform turretHead;     // Rotating part of the turret

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Audio")]
    public AudioClip shootSound;

    private float fireCooldown;
    private Transform player;
    private DeliverySystem deliverySystem;
    private AudioSource audioSource;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            deliverySystem = playerObj.GetComponent<DeliverySystem>();
        }

        audioSource = GetComponent<AudioSource>();

        // Random Y rotation on spawn (head only)
        if (turretHead)
            turretHead.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    void Update()
    {
        if (player == null || deliverySystem == null)
            return;

        // Tick cooldown FIRST
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        // Only active while player has package
        if (!deliverySystem.hasPackage)
            return;

        Vector3 targetPos = player.position + Vector3.up * aimHeight;
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > detectionRange)
            return;

        if (!HasLineOfSight(targetPos, distance))
            return;

        RotateTowards(targetPos);

        // Optional: only shoot when mostly aimed
        if (IsAimedAtTarget(targetPos))
            Shoot();
    }

    // --------------------
    // LINE OF SIGHT CHECK
    // --------------------
    bool HasLineOfSight(Vector3 targetPos, float distance)
    {
        Vector3 origin = eyePoint ? eyePoint.position : transform.position;
        Vector3 dir = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, visionMask))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }

    // --------------------
    // ROTATION
    // --------------------
    void RotateTowards(Vector3 targetPos)
    {
        if (!turretHead)
            return;

        Vector3 dir = (targetPos - turretHead.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    bool IsAimedAtTarget(Vector3 targetPos)
    {
        if (!turretHead)
            return true;

        Vector3 dir = (targetPos - turretHead.position).normalized;
        float angle = Vector3.Angle(turretHead.forward, dir);

        return angle < 5f; // degrees
    }

    // --------------------
    // SHOOTING
    // --------------------
    void Shoot()
    {
        if (fireCooldown > 0f)
            return;

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (shootSound && audioSource)
            audioSource.PlayOneShot(shootSound);

        fireCooldown = fireRate;
    }

    // --------------------
    // DEBUG
    // --------------------
    void OnDrawGizmosSelected()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (!p)
            return;

        Vector3 targetPos = p.transform.position + Vector3.up * aimHeight;
        Vector3 origin = eyePoint ? eyePoint.position : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, targetPos);
    }
}
