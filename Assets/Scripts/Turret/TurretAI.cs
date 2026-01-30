using UnityEngine;

public class TurretAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 20f;
    public float yawSpeed = 8f;
    public float pitchSpeed = 6f;
    public float fireRate = 0.5f;
    public float aimHeight = 1.2f;
    public float maxPitchAngle = 45f;

    [Header("Raycast Vision")]
    public LayerMask visionMask;
    public Transform eyePoint;

    [Header("Turret Parts")]
    public Transform yawPivot;    // Rotates on Y
    public Transform pitchPivot;  // Rotates on X

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
        if (playerObj)
        {
            player = playerObj.transform;
            deliverySystem = playerObj.GetComponent<DeliverySystem>();
        }

        audioSource = GetComponent<AudioSource>();

        // Random yaw on spawn
        if (yawPivot)
            yawPivot.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    void Update()
    {
        if (!player || !deliverySystem)
            return;

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (!deliverySystem.hasPackage)
            return;

        Vector3 targetPos = player.position + Vector3.up * aimHeight;
        float distance = Vector3.Distance(eyePoint.position, targetPos);

        if (distance > detectionRange)
            return;

        if (!HasLineOfSight(targetPos, distance))
            return;

        RotateYaw(targetPos);
        RotatePitch(targetPos);

        if (IsAimedAtTarget(targetPos))
            Shoot();
    }

    // --------------------
    // LINE OF SIGHT
    // --------------------
    bool HasLineOfSight(Vector3 targetPos, float distance)
    {
        Vector3 origin = eyePoint.position;
        Vector3 dir = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance, visionMask))
            return hit.transform.CompareTag("Player");

        return false;
    }

    // --------------------
    // ROTATION
    // --------------------
    void RotateYaw(Vector3 targetPos)
    {
        Vector3 flatDir = targetPos - yawPivot.position;
        flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(flatDir);
        yawPivot.rotation = Quaternion.Slerp(
            yawPivot.rotation,
            targetRot,
            yawSpeed * Time.deltaTime
        );
    }

    void RotatePitch(Vector3 targetPos)
    {
        Vector3 localDir = yawPivot.InverseTransformPoint(targetPos);
        float angle = -Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, -maxPitchAngle, maxPitchAngle);

        Quaternion targetRot = Quaternion.Euler(angle, 0f, 0f);
        pitchPivot.localRotation = Quaternion.Slerp(
            pitchPivot.localRotation,
            targetRot,
            pitchSpeed * Time.deltaTime
        );
    }

    bool IsAimedAtTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - firePoint.position).normalized;
        float angle = Vector3.Angle(firePoint.forward, dir);
        return angle < 5f;
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
        if (!eyePoint)
            return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (!p)
            return;

        Vector3 targetPos = p.transform.position + Vector3.up * aimHeight;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyePoint.position, targetPos);
    }
}
