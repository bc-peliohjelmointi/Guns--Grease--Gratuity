using UnityEngine;
using System.Collections;

public class ElectricTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageToPlayer = 15f;
    public float damageToDelivery = 20f;
    public float damageTickRate = 0.5f;

    [Header("Electric Pattern")]
    public float onDuration = 2f;
    public float offDuration = 2f;
    public float startDelay = 0f;
    public bool startActive = false;

    [Header("Visual Effects")]
    public Material electricMaterial;
    public Material normalMaterial;
    public GameObject electricEffect;
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.gray;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip electricSound;
    public AudioClip zapSound;

    [Header("Components")]
    public Renderer floorRenderer;
    public Light floorLight;

    [Header("Detection (Leave at default)")]
    public float detectionRadius = 5f;
    public float detectionHeight = 2f;

    private bool isActive = false;
    private bool playerOnFloor = false;
    private StarterAssets.FirstPersonController playerController;
    private DeliverySystem deliverySystem;
    private Coroutine damageCoroutine;

    private void Start()
    {
        Debug.Log("ElectricFloor: START called!");

        if (floorRenderer == null)
            floorRenderer = GetComponent<Renderer>();

        if (audioSource == null && (electricSound != null || zapSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        isActive = startActive;
        UpdateVisuals();

        StartCoroutine(ElectricCycle());
    }

    private void Update()
    {
        // ALWAYS check for player nearby
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("ElectricFloor: No player found with 'Player' tag!");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        float yDiff = Mathf.Abs(player.transform.position.y - transform.position.y);

        // Debug EVERY frame
        Debug.Log($"ElectricFloor Update: Distance={distance:F2}, YDiff={yDiff:F2}, PlayerOnFloor={playerOnFloor}, IsActive={isActive}");

        // Check if player is on floor
        bool shouldBeOnFloor = (distance < detectionRadius && yDiff < detectionHeight);

        if (shouldBeOnFloor && !playerOnFloor)
        {
            // Player just stepped on floor
            Debug.Log("ElectricFloor: PLAYER STEPPED ON FLOOR!");
            playerOnFloor = true;
            playerController = player.GetComponent<StarterAssets.FirstPersonController>();
            deliverySystem = player.GetComponent<DeliverySystem>();

            if (playerController == null)
                Debug.LogError("ElectricFloor: FirstPersonController component NOT FOUND!");
            else
                Debug.Log("ElectricFloor: FirstPersonController found!");

            // Start damaging if active
            if (isActive)
            {
                Debug.Log("ElectricFloor: Starting damage (floor is active)");
                StartDamaging();
            }
        }
        else if (!shouldBeOnFloor && playerOnFloor)
        {
            // Player left floor
            Debug.Log("ElectricFloor: Player LEFT floor");
            playerOnFloor = false;
            StopDamaging();
        }
        else if (shouldBeOnFloor && playerOnFloor && isActive && damageCoroutine == null)
        {
            // Floor became active while player was standing
            Debug.Log("ElectricFloor: Floor activated while player standing");
            StartDamaging();
        }
        else if (!isActive && damageCoroutine != null)
        {
            // Floor deactivated
            Debug.Log("ElectricFloor: Floor deactivated, stopping damage");
            StopDamaging();
        }
    }

    private void StartDamaging()
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamagePlayer());
    }

    private void StopDamaging()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator ElectricCycle()
    {
        if (startDelay > 0)
        {
            Debug.Log($"ElectricFloor: Waiting {startDelay}s before cycle");
            yield return new WaitForSeconds(startDelay);
        }

        Debug.Log("ElectricFloor: Starting ON/OFF cycle");

        while (true)
        {
            // Turn ON
            isActive = true;
            UpdateVisuals();
            Debug.Log($"ElectricFloor: Turned ON (duration {onDuration}s)");
            yield return new WaitForSeconds(onDuration);

            // Turn OFF
            isActive = false;
            UpdateVisuals();
            Debug.Log($"ElectricFloor: Turned OFF (duration {offDuration}s)");
            yield return new WaitForSeconds(offDuration);
        }
    }

    private void UpdateVisuals()
    {
        if (floorRenderer != null)
        {
            if (electricMaterial != null && normalMaterial != null)
            {
                floorRenderer.material = isActive ? electricMaterial : normalMaterial;
            }
            else
            {
                floorRenderer.material.color = isActive ? activeColor : inactiveColor;
            }

            if (isActive)
            {
                floorRenderer.material.EnableKeyword("_EMISSION");
                floorRenderer.material.SetColor("_EmissionColor", activeColor * 0.5f);
            }
            else
            {
                floorRenderer.material.DisableKeyword("_EMISSION");
            }
        }

        if (electricEffect != null)
        {
            electricEffect.SetActive(isActive);
        }

        if (floorLight != null)
        {
            floorLight.enabled = isActive;
            floorLight.color = activeColor;
        }

        if (audioSource != null && electricSound != null)
        {
            if (isActive && !audioSource.isPlaying)
                audioSource.PlayOneShot(electricSound);
            else if (!isActive && audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    private IEnumerator DamagePlayer()
    {
        Debug.Log("ElectricFloor: DamagePlayer coroutine STARTED");

        while (playerOnFloor && isActive)
        {
            if (audioSource != null && zapSound != null)
                audioSource.PlayOneShot(zapSound);

            // Damage player
            if (playerController != null)
            {
                playerController.TakeDamage(damageToPlayer);
                Debug.Log($"ElectricFloor: Dealt {damageToPlayer} damage to player! Current health: {playerController.currentHealth}");
            }
            else
            {
                Debug.LogError("ElectricFloor: PlayerController is NULL, cannot damage!");
            }

            // Damage package
            if (deliverySystem != null && deliverySystem.hasPackage)
            {
                deliverySystem.TakeDamage(damageToDelivery);
                Debug.Log($"ElectricFloor: Dealt {damageToDelivery} damage to package!");
            }

            yield return new WaitForSeconds(damageTickRate);
        }

        Debug.Log("ElectricFloor: DamagePlayer coroutine ENDED");
        damageCoroutine = null;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        UpdateVisuals();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? new Color(0f, 1f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}