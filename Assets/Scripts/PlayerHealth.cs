using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Regeneration")]
    public bool enableHealthRegen = false;
    public float regenRate = 5f; // HP per second
    public float regenDelay = 3f; // Seconds after taking damage before regen starts
    private float lastDamageTime;

    [Header("UI References")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthBarFill;

    [Header("Health Bar Colors")]
    public Color healthyColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color lowColor = Color.red;
    public float mediumHealthThreshold = 0.5f; // 50%
    public float lowHealthThreshold = 0.25f; // 25%

    [Header("Screen Effects")]
    public Image bloodOverlay; // Red vignette/blood splatter
    public Image damageFlash; // Full screen damage flash
    public float flashDuration = 0.2f;
    public float bloodFadeSpeed = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    public AudioClip lowHealthHeartbeat;

    [Header("Camera Shake")]
    public bool enableCameraShake = true;
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.2f;

    [Header("References")]
    public StarterAssets.FirstPersonController playerController;
    public Respawn respawn;

    // Private
    private float targetBloodAlpha = 0f;
    private bool isPlayingHeartbeat = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Setup screen effects
        if (bloodOverlay != null)
        {
            Color c = bloodOverlay.color;
            c.a = 0f;
            bloodOverlay.color = c;
        }

        if (damageFlash != null)
        {
            Color c = damageFlash.color;
            c.a = 0f;
            damageFlash.color = c;
        }

        // Auto-find audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateHealthUI();
    }

    private void Update()
    {
        if (isDead) return;

        // Health regeneration
        if (enableHealthRegen && currentHealth < maxHealth)
        {
            if (Time.time - lastDamageTime >= regenDelay)
            {
                Heal(regenRate * Time.deltaTime);
            }
        }

        // Update blood overlay fade
        UpdateBloodOverlay();

        // Low health heartbeat
        UpdateLowHealthEffects();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        lastDamageTime = Time.time;

        // Visual feedback
        StartCoroutine(DamageFlashEffect());
        UpdateBloodIntensity();

        // Audio feedback
        PlayHurtSound();

        // Camera shake
        if (enableCameraShake)
            StartCoroutine(CameraShake());

        // Update UI
        UpdateHealthUI();

        // Check death
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthSlider != null)
            healthSlider.maxValue = maxHealth;

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        // Update slider
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // Update text
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {maxHealth}";

        // Update health bar color
        UpdateHealthBarColor();
    }

    private void UpdateHealthBarColor()
    {
        if (healthBarFill == null) return;

        float healthPercent = currentHealth / maxHealth;

        if (healthPercent <= lowHealthThreshold)
        {
            healthBarFill.color = lowColor;
        }
        else if (healthPercent <= mediumHealthThreshold)
        {
            // Lerp between low and medium color
            float t = (healthPercent - lowHealthThreshold) / (mediumHealthThreshold - lowHealthThreshold);
            healthBarFill.color = Color.Lerp(lowColor, mediumColor, t);
        }
        else
        {
            // Lerp between medium and healthy color
            float t = (healthPercent - mediumHealthThreshold) / (1f - mediumHealthThreshold);
            healthBarFill.color = Color.Lerp(mediumColor, healthyColor, t);
        }
    }

    private IEnumerator DamageFlashEffect()
    {
        if (damageFlash == null) yield break;

        // Flash in
        Color c = damageFlash.color;
        c.a = 0.3f;
        damageFlash.color = c;

        // Fade out
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.3f, 0f, elapsed / flashDuration);
            damageFlash.color = c;
            yield return null;
        }

        c.a = 0f;
        damageFlash.color = c;
    }

    private void UpdateBloodIntensity()
    {
        float healthPercent = currentHealth / maxHealth;

        // More blood when lower health
        targetBloodAlpha = Mathf.Clamp01(1f - healthPercent) * 0.6f;
    }

    private void UpdateBloodOverlay()
    {
        if (bloodOverlay == null) return;

        Color c = bloodOverlay.color;
        c.a = Mathf.Lerp(c.a, targetBloodAlpha, Time.deltaTime * bloodFadeSpeed);
        bloodOverlay.color = c;
    }

    private void UpdateLowHealthEffects()
    {
        float healthPercent = currentHealth / maxHealth;

        // Play heartbeat when health is low
        if (healthPercent <= lowHealthThreshold && !isPlayingHeartbeat)
        {
            if (audioSource != null && lowHealthHeartbeat != null)
            {
                audioSource.clip = lowHealthHeartbeat;
                audioSource.loop = true;
                audioSource.Play();
                isPlayingHeartbeat = true;
            }
        }
        else if (healthPercent > lowHealthThreshold && isPlayingHeartbeat)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
                isPlayingHeartbeat = false;
            }
        }
    }

    private void PlayHurtSound()
    {
        if (audioSource == null || hurtSounds.Length == 0) return;

        AudioClip randomHurt = hurtSounds[Random.Range(0, hurtSounds.Length)];
        audioSource.PlayOneShot(randomHurt);
    }

    private IEnumerator CameraShake()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            camTransform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        // Stop heartbeat
        if (audioSource != null && isPlayingHeartbeat)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // Play death sound
        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        // Full blood screen
        if (bloodOverlay != null)
        {
            Color c = bloodOverlay.color;
            c.a = 0.8f;
            bloodOverlay.color = c;
        }

        // Hide health UI
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(false);

        // Disable player movement
        if (playerController != null)
            playerController.canMove = false;

        Debug.Log("Player died");

        // Trigger respawn/game over
        if (respawn != null)
            respawn.GameOver();
    }

    // Public helper methods
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    public bool IsLowHealth()
    {
        return GetHealthPercent() <= lowHealthThreshold;
    }

    // Reset player health (for respawn/restart)
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        targetBloodAlpha = 0f;

        // Clear blood overlay immediately
        if (bloodOverlay != null)
        {
            Color c = bloodOverlay.color;
            c.a = 0f;
            bloodOverlay.color = c;
        }

        // Clear damage flash
        if (damageFlash != null)
        {
            Color c = damageFlash.color;
            c.a = 0f;
            damageFlash.color = c;
        }

        // Stop heartbeat
        if (audioSource != null && isPlayingHeartbeat)
        {
            audioSource.Stop();
            audioSource.loop = false;
            isPlayingHeartbeat = false;
        }

        // Show health UI
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(true);

        // Update UI
        UpdateHealthUI();

        Debug.Log("PlayerHealth: Reset complete");
    }
}