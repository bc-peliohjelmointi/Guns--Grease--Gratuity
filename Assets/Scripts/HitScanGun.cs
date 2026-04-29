using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class GunHitscan : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public Transform cameraRoot;
    public Camera playerCamera;
    public GameObject muzzleFlash;
    public GameObject impactPrefab;
    public LayerMask hitMask = ~0;
    public PhoneUI phoneUI;
    public PullOurScript pullOurScript;
    public GunSlide slide;
    public GameObject tracerPrefab;
    public GunUI ui;

    private PlayerStats stats;

    [Header("Stats")]
    public float baseDamage = 25f;
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 10f;
    public int magazineSize = 30;
    public int totalAmmo = 30;
    public float reloadTime = 2f;
    public bool automatic = true;

    [Header("Effects")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public float tracerSpeed = 200f;

    [Header("Advanced Recoil")]
    public float recoilX = 2f;
    public float recoilY = 1f;
    public float recoilZ = 0.5f;

    public float recoilSnappiness = 6f;
    public float recoilReturnSpeed = 4f;

    Vector3 currentRecoil;
    Vector3 targetRecoil;

    [Header("Camera Punch")]
    public float punchAmount = 2f;
    public float punchRecovery = 10f;

    Vector3 punchOffset;

    AudioSource audioSource;
    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    Quaternion originalCamRot;
    public float flashTime = 0.2f;
    private int minusAmmo;

    public Animator animator;


    void Start()
    {
        stats = PlayerStats.Instance;

        StartCoroutine(StartActive());
        audioSource = GetComponent<AudioSource>();
        currentAmmo = magazineSize;
        if (playerCamera) originalCamRot = playerCamera.transform.localRotation;

        if (muzzleFlash)
            muzzleFlash.SetActive(false);

        if (ui) ui.UpdateAmmo(currentAmmo, totalAmmo);
    }

    void Update()
    {
        ApplyUpgrades();

        if (PhoneUI.AnyOpen)
            return;

        if (isReloading) return;

        bool firePressed = automatic
            ? Mouse.current != null && Mouse.current.leftButton.isPressed
            : Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (firePressed && Time.time >= nextFireTime && currentAmmo > 0 && pullOurScript.gunIsOut == true)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Fire();
        }
        else if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame && currentAmmo < magazineSize && totalAmmo !=0)
        {
            StartCoroutine(Reload());
        }

        // Smoothly return recoil
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, recoilReturnSpeed * Time.deltaTime);

        // Apply recoil movement
        currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, recoilSnappiness * Time.deltaTime);

        // Combine recoil + punch
        Vector3 finalRotation = currentRecoil + punchOffset;

        // Apply to camera
        playerCamera.transform.localRotation = originalCamRot * Quaternion.Euler(-finalRotation);
    }

    public void ApplyUpgrades()
    {
        damage = baseDamage + stats.weaponDamageLevel * 7.5f;
    }

    void Fire()
    {
        currentAmmo--;
        if (ui) ui.UpdateAmmo(currentAmmo, totalAmmo);

        StartCoroutine(ShootVFX());

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = Random.Range(0.9f, 1f);
        AudioSource.PlayClipAtPoint(fireSound, muzzle.position, 1f);

        StartCoroutine(PlayEcho());

        Vector3 origin;
        Vector3 dir;

        Ray camRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit camHit;

        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out camHit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = camHit.point;
        }
        else
        {
            targetPoint = camRay.origin + camRay.direction * range;
        }

        origin = muzzle.position;
        dir = (targetPoint - origin).normalized;



        RaycastHit hit;
        bool didHit = Physics.Raycast(origin, dir, out hit, range, hitMask, QueryTriggerInteraction.Ignore);

        Vector3 tracerEnd;

        if (didHit)
        {
            tracerEnd = hit.point;

            if (impactPrefab)
            {
                var go = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(go, 5f);
            }

            // Check for turret
            var turret = hit.collider.GetComponentInParent<TurretHealth>();
            if (turret != null)
            {
                turret.TakeDamage(Mathf.CeilToInt(damage));
            }

            // Check for enemy AI
            var enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy == null)
            {
                enemy = hit.collider.GetComponentInParent<EnemyAI>();
            }
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Hit enemy for {damage} damage!");
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(dir * 200f, hit.point);
            }
        }
        else
        {
            // Proper miss direction
            tracerEnd = origin + dir * range;
        }

        if (tracerPrefab)
        {
            StartCoroutine(SpawnTracer(muzzle.position, tracerEnd));
        }

        targetRecoil += new Vector3(
            recoilX,
            Random.Range(-recoilY, recoilY),
            Random.Range(-recoilZ, recoilZ)
        );

        GetComponentInChildren<GunSway>()?.AddRecoil(recoilX);

        punchOffset += new Vector3(
             -punchAmount,
             Random.Range(-punchAmount * 0.3f, punchAmount * 0.3f),
             0
        );
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (reloadSound) AudioSource.PlayClipAtPoint(reloadSound, muzzle.position, 1f);

        if (animator)
            animator.SetTrigger("Reload");
            Debug.Log("Reload anim triggered");

        yield return new WaitForSeconds(reloadTime);

        totalAmmo = totalAmmo - magazineSize;
        currentAmmo = magazineSize;
        if (ui) ui.UpdateAmmo(currentAmmo, totalAmmo);

        isReloading = false;
    }

    IEnumerator ShootVFX()
    {
        if (slide != null)
        {
            slide.Kick();
        }

        if (muzzleFlash)
        {
            muzzleFlash.SetActive(true);
            yield return new WaitForSeconds(flashTime);
            muzzleFlash.SetActive(false);
        }
    }

    IEnumerator SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(tracerPrefab, start, Quaternion.identity);
        LineRenderer lr = tracer.GetComponent<LineRenderer>();

        if (lr)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        Destroy(tracer, 0.05f);
        yield return null;
    }

    IEnumerator PlayEcho()
    {
        yield return new WaitForSeconds(0.1f);
        AudioSource.PlayClipAtPoint(fireSound, muzzle.position, 0.4f);
    }

    IEnumerator StartActive()
    {
        enabled = false;

        yield return new WaitForSeconds(0.1f);

        enabled = true;
    }
    public void ResetGun()
    {
        currentAmmo = magazineSize;
        isReloading = false;
    }
}