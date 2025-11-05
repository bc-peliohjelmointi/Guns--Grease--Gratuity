using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class GunHitscan : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;                // where bullets originate
    public Camera playerCamera;             // FPS camera; if null we'll use muzzle.forward
    public GameObject muzzleFlash; // assign in Inspector
    public GameObject impactPrefab;         // small prefab spawned on hit (sparks)
    public LayerMask hitMask = ~0;          // layers bullets will hit

    [Header("Stats")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 10f;            // rounds per second
    public int magazineSize = 30;
    public float reloadTime = 2f;
    public bool automatic = true;

    [Header("Effects")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public float recoilAmount = 1.5f;       // camera recoil angle
    public float recoilSmooth = 6f;

    AudioSource audioSource;
    int currentAmmo;
    float nextFireTime;
    bool isReloading;
    Quaternion originalCamRot;
    public float flashTime = 0.2f; // how long flash stays visible

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = magazineSize;
        if (playerCamera) originalCamRot = playerCamera.transform.localRotation;
        if (muzzleFlash)
            muzzleFlash.SetActive(false);
    }

    void Update()
    {
        if (isReloading) return;

        bool firePressed = automatic ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetButtonDown("Fire1");

        if (firePressed && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + 1f / fireRate;
            Fire();
        }
        else if ((Keyboard.current.rKey.wasPressedThisFrame) && (currentAmmo < magazineSize))
        {
            StartCoroutine(Reload());
        }

        // simple recoil recovery
        if (playerCamera)
        {
            playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation,
                originalCamRot, Time.deltaTime * recoilSmooth);
        }
    }

    void Fire()
    {
        currentAmmo--;

        // muzzle flash
        StartCoroutine(Flash());

        // audio
        if (fireSound) audioSource.PlayOneShot(fireSound);

        // direction
        Vector3 origin = muzzle ? muzzle.position : transform.position;
        Vector3 dir;
        if (playerCamera)
        {
            // shoot from camera center so aim is correct
            Ray camRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            dir = camRay.direction;
            // optional: set origin to muzzle but aim toward camera center point at range:
            origin = muzzle ? muzzle.position : origin;
        }
        else
        {
            dir = muzzle ? muzzle.forward : transform.forward;
        }

        // raycast
        if (Physics.Raycast(origin, dir, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // spawn impact
            if (impactPrefab)
            {
                var go = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(go, 5f);
            }

            // damage handling: try to find a health component
            var health = hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            // optional: apply force
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(dir * 200f, hit.point); // tune force
            }
        }

        // simple camera recoil: rotate camera up a bit
        if (playerCamera)
        {
            playerCamera.transform.localRotation *= Quaternion.Euler(-recoilAmount, 0f, 0f);
        }

        // TODO: play firing animation by triggering an Animator if you have one
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (reloadSound) audioSource.PlayOneShot(reloadSound);
        // optionally trigger reload animation here (Animator.SetTrigger)
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
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

}