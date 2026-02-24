using UnityEngine;
using System.Collections;

public class ElectricTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 20f; // Continuous damage while standing on it
    public float damageToPlayer = 15f; // Per tick
    public float damageToDelivery = 20f; // Per tick
    public float damageTickRate = 0.5f; // How often damage is applied (seconds)

    [Header("Electric Pattern")]
    public float onDuration = 2f; // How long electricity is ON
    public float offDuration = 2f; // How long electricity is OFF
    public float startDelay = 0f; // Delay before first activation (for syncing multiple traps)
    public bool startActive = false; // Start with electricity on?

    [Header("Visual Effects")]
    public Material electricMaterial; // Material when active
    public Material normalMaterial; // Material when inactive
    public GameObject electricEffect; // Particle effect (lightning, sparks, etc.)
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.gray;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip electricSound; // Continuous buzzing sound
    public AudioClip zapSound; // Sound when player gets zapped

    [Header("Components")]
    public Renderer floorRenderer; // The visual floor mesh
    public Light floorLight; // Optional light that pulses

    private bool isActive = false;
    private bool playerOnFloor = false;
    private StarterAssets.FirstPersonController playerController;
    private DeliverySystem deliverySystem;
    private Coroutine damageCoroutine;

    private void Start()
    {
        // Auto-find renderer if not assigned
        if (floorRenderer == null)
            floorRenderer = GetComponent<Renderer>();

        // Auto-add audio source if needed
        if (audioSource == null && (electricSound != null || zapSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        // Set initial state
        isActive = startActive;
        UpdateVisuals();

        // Start the on/off cycle
        StartCoroutine(ElectricCycle());
    }

    // Main electric on/off cycle
    private IEnumerator ElectricCycle()
    {
        // Initial delay
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // Turn ON
            isActive = true;
            UpdateVisuals();
            yield return new WaitForSeconds(onDuration);

            // Turn OFF
            isActive = false;
            UpdateVisuals();
            yield return new WaitForSeconds(offDuration);
        }
    }

    // Update visual and audio feedback
    private void UpdateVisuals()
    {
        // Update material
        if (floorRenderer != null)
        {
            if (electricMaterial != null && normalMaterial != null)
            {
                floorRenderer.material = isActive ? electricMaterial : normalMaterial;
            }
            else
            {
                // Use color tinting if no materials assigned
                floorRenderer.material.color = isActive ? activeColor : inactiveColor;
            }

            // Make it glow when active
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

        // Update particle effect
        if (electricEffect != null)
        {
            if (isActive && !electricEffect.activeSelf)
                electricEffect.SetActive(true);
            else if (!isActive && electricEffect.activeSelf)
                electricEffect.SetActive(false);
        }

        // Update light
        if (floorLight != null)
        {
            floorLight.enabled = isActive;
            floorLight.color = activeColor;
        }

        // Update audio
        if (audioSource != null && electricSound != null)
        {
            if (isActive && !audioSource.isPlaying)
                audioSource.PlayOneShot(electricSound);
            else if (!isActive && audioSource.isPlaying)
                audioSource.Stop();
        }
    }

    // Detect player entering electric floor
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnFloor = true;
            playerController = other.GetComponent<StarterAssets.FirstPersonController>();
            deliverySystem = other.GetComponent<DeliverySystem>();

            // Start damaging if electricity is active
            if (isActive)
            {
                if (damageCoroutine != null)
                    StopCoroutine(damageCoroutine);
                damageCoroutine = StartCoroutine(DamagePlayer());
            }
        }
    }

    // Detect player staying on electric floor
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Start damage if electricity turned on while player was standing
            if (isActive && damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(DamagePlayer());
            }
        }
    }

    // Detect player leaving electric floor
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnFloor = false;

            // Stop damaging
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    // Continuously damage player while on active floor
    private IEnumerator DamagePlayer()
    {
        while (playerOnFloor && isActive)
        {
            // Play zap sound
            if (audioSource != null && zapSound != null)
                audioSource.PlayOneShot(zapSound);

            // Damage player
            if (playerController != null)
            {
                playerController.TakeDamage(damageToPlayer);
                Debug.Log($"Electric floor dealt {damageToPlayer} damage to player!");
            }

            // Damage package
            if (deliverySystem != null && deliverySystem.hasPackage)
            {
                deliverySystem.TakeDamage(damageToDelivery);
                Debug.Log($"Electric floor dealt {damageToDelivery} damage to package!");
            }

            // Wait before next damage tick
            yield return new WaitForSeconds(damageTickRate);
        }

        damageCoroutine = null;
    }

    // Public method to sync multiple electric floors
    public void SetActive(bool active)
    {
        isActive = active;
        UpdateVisuals();
    }

    // Visualize trigger area in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? new Color(0f, 1f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f, 0.3f);

        // Draw bounds
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}