using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class GunHitscan : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public Camera playerCamera;
    public GameObject muzzleFlash;
    public GameObject impactPrefab;
    public LayerMask hitMask = ~0;
    public PhoneUI phoneUI;
    public PullOurScript pullOurScript;

    [Header("Stats")]
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
    public float recoilAmount = 1.5f;
    public float recoilSmooth = 6f;

    AudioSource audioSource;
    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    Quaternion originalCamRot;
    public float flashTime = 0.2f;
    private int minusAmmo;

    public GameObject tracerPrefab;
    public GunUI ui;
    public float tracerSpeed = 200f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = magazineSize;
        if (playerCamera) originalCamRot = playerCamera.transform.localRotation;

        if (muzzleFlash)
            muzzleFlash.SetActive(false);

        if (ui) ui.UpdateAmmo(currentAmmo, totalAmmo);
    }

    void Update()
    {
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

        if (playerCamera)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(
                playerCamera.transform.localRotation,
                originalCamRot,
                Time.deltaTime * recoilSmooth
            );
        }
    }

    void Fire()
    {
        currentAmmo--;
        if (ui) ui.UpdateAmmo(currentAmmo, totalAmmo);

        StartCoroutine(Flash());

        if (fireSound) audioSource.PlayOneShot(fireSound);

        Vector3 origin = muzzle ? muzzle.position : transform.position;
        Vector3 dir;

        if (playerCamera)
        {
            Ray camRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            dir = camRay.direction;
            origin = muzzle ? muzzle.position : origin;
        }
        else
        {
            dir = muzzle ? muzzle.forward : transform.forward;
        }

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

        if (playerCamera)
        {
            playerCamera.transform.localRotation *= Quaternion.Euler(-recoilAmount, 0f, 0f);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (reloadSound) audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        totalAmmo = totalAmmo - magazineSize;
        currentAmmo = magazineSize;
        if (ui) ui.UpdateAmmo(currentAmmo, totalAmmo);

        isReloading = false;
    }

    IEnumerator Flash()
    {
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
}