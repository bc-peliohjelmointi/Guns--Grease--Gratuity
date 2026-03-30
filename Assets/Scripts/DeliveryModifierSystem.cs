using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeliveryModifierSystem : MonoBehaviour
{
    [Header("Modifier Settings")]
    [Range(0f, 1f)]
    public float modifierChance = 0.5f; // 50% chance of getting a modifier
    public bool allowMultipleModifiers = false; // Can stack multiple modifiers?
    public int maxModifiers = 2; // If multiple allowed

    [Header("UI References")]
    public TextMeshProUGUI modifierNotificationText;
    public GameObject modifierPanel;
    public float notificationDuration = 5f;

    [Header("Player References")]
    public StarterAssets.FirstPersonController playerController;
    public DeliverySystem deliverySystem;
    public GunHitscan playerGun;

    // Current active modifiers
    private List<DeliveryModifier> activeModifiers = new List<DeliveryModifier>();

    // All possible modifiers
    private DeliveryModifier[] allModifiers;

    private void Start()
    {
        // Define all possible modifiers
        allModifiers = new DeliveryModifier[]
        {
            // NEGATIVE MODIFIERS
            new DeliveryModifier(
                "Weapon Malfunction",
                "Your gun is jammed! Damage reduced by 50%.",
                ModifierType.Negative,
                ApplyWeaponMalfunction,
                RemoveWeaponMalfunction
            ),
            new DeliveryModifier(
                "Heavy Package",
                "This package is extra heavy! Movement speed reduced by 30%.",
                ModifierType.Negative,
                ApplyHeavyPackage,
                RemoveHeavyPackage
            ),
            new DeliveryModifier(
                "Fragile Cargo",
                "Handle with care! Package takes 50% more damage.",
                ModifierType.Negative,
                ApplyFragileCargo,
                RemoveFragileCargo
            ),
            new DeliveryModifier(
                "Rush Order",
                "Client wants it NOW! Delivery time reduced by 30%.",
                ModifierType.Negative,
                ApplyRushOrder,
                RemoveRushOrder
            ),
            new DeliveryModifier(
                "Injured",
                "You're not feeling well. Max health reduced by 25%.",
                ModifierType.Negative,
                ApplyInjured,
                RemoveInjured
            ),
            new DeliveryModifier(
                "Enemy Alert",
                "Enemies detected your presence! 50% more enemies spawn.",
                ModifierType.Negative,
                ApplyEnemyAlert,
                RemoveEnemyAlert
            ),
            new DeliveryModifier(
                "Low Ammo",
                "Running low on bullets! Magazine size halved.",
                ModifierType.Negative,
                ApplyLowAmmo,
                RemoveLowAmmo
            ),
            
            // POSITIVE MODIFIERS
            new DeliveryModifier(
                "Adrenaline Rush",
                "Feeling pumped! Movement speed increased by 30%.",
                ModifierType.Positive,
                ApplyAdrenalineRush,
                RemoveAdrenalineRush
            ),
            new DeliveryModifier(
                "Premium Client",
                "High-value customer! Reward increased by 50%.",
                ModifierType.Positive,
                ApplyPremiumClient,
                RemovePremiumClient
            ),
            new DeliveryModifier(
                "Armored Package",
                "Package has protective casing! Takes 50% less damage.",
                ModifierType.Positive,
                ApplyArmoredPackage,
                RemoveArmoredPackage
            ),
            new DeliveryModifier(
                "Extended Deadline",
                "Client is patient. Delivery time increased by 50%.",
                ModifierType.Positive,
                ApplyExtendedDeadline,
                RemoveExtendedDeadline
            ),
            new DeliveryModifier(
                "Power Surge",
                "Weapon overcharged! Damage increased by 50%.",
                ModifierType.Positive,
                ApplyPowerSurge,
                RemovePowerSurge
            ),
            new DeliveryModifier(
                "Lucky Day",
                "Today's your day! Health regenerates slowly.",
                ModifierType.Positive,
                ApplyLuckyDay,
                RemoveLuckyDay
            ),
            
            // NEUTRAL/MIXED MODIFIERS
            new DeliveryModifier(
                "Glass Cannon",
                "High risk, high reward! +100% damage, -50% health.",
                ModifierType.Mixed,
                ApplyGlassCannon,
                RemoveGlassCannon
            ),
            new DeliveryModifier(
                "Speed Demon",
                "Gotta go fast! +50% speed, package takes +25% damage.",
                ModifierType.Mixed,
                ApplySpeedDemon,
                RemoveSpeedDemon
            )
        };

        if (modifierPanel != null)
            modifierPanel.SetActive(false);
    }

    // Call this when a delivery starts
    public void RollModifiers()
    {
        // Clear previous modifiers
        ClearAllModifiers();

        // Roll for modifiers
        if (allowMultipleModifiers)
        {
            int modifierCount = Random.Range(0, maxModifiers + 1);
            for (int i = 0; i < modifierCount; i++)
            {
                if (Random.value <= modifierChance)
                {
                    ApplyRandomModifier();
                }
            }
        }
        else
        {
            // Single modifier chance
            if (Random.value <= modifierChance)
            {
                ApplyRandomModifier();
            }
        }

        // Show notification if any modifiers applied
        if (activeModifiers.Count > 0)
        {
            ShowModifierNotification();
        }
    }

    private void ApplyRandomModifier()
    {
        // Pick random modifier
        DeliveryModifier modifier = allModifiers[Random.Range(0, allModifiers.Length)];

        // Don't apply same modifier twice
        if (activeModifiers.Contains(modifier))
            return;

        // Apply it
        modifier.onApply?.Invoke();
        activeModifiers.Add(modifier);

        Debug.Log($"Applied modifier: {modifier.name}");
    }

    private Coroutine hideCoroutine;

    private void ShowModifierNotification()
    {
        if (modifierNotificationText == null)
        {
            return;
        }

        // Cancel previous hide coroutine if still running
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // Build notification text
        string notification = "<size=36><b>DELIVERY MODIFIERS:</b></size>\n\n";
        foreach (var mod in activeModifiers)
        {
            string color = mod.type == ModifierType.Positive ? "green" :
                          mod.type == ModifierType.Negative ? "red" : "yellow";
            notification += $"<color={color}><b>{mod.name}</b></color>\n{mod.description}\n\n";
        }

        modifierNotificationText.text = notification;
        Debug.Log($"ModifierSystem: Showing notification - {activeModifiers.Count} modifiers");

        // Show panel
        if (modifierPanel != null)
        {
            modifierPanel.SetActive(true);
            Debug.Log($"ModifierSystem: Panel shown, will hide in {notificationDuration} seconds");
            hideCoroutine = StartCoroutine(HideModifierPanelAfterDelay());
        }
    }

    private System.Collections.IEnumerator HideModifierPanelAfterDelay()
    {
        Debug.Log($"ModifierSystem: Starting hide timer ({notificationDuration}s)");
        yield return new WaitForSeconds(notificationDuration);
        Debug.Log("ModifierSystem: Hiding panel now");
        HideModifierPanel();
        hideCoroutine = null;
    }

    private void HideModifierPanel()
    {
        if (modifierPanel != null)
        {
            modifierPanel.SetActive(false);
            Debug.Log("ModifierSystem: Panel hidden");
        }
    }

    public void ClearAllModifiers()
    {
        foreach (var modifier in activeModifiers)
        {
            modifier.onRemove?.Invoke();
        }
        activeModifiers.Clear();
        Debug.Log("Cleared all modifiers");
    }

    // Public methods for checking modifiers
    public bool HasModifier(string modifierName)
    {
        foreach (var mod in activeModifiers)
        {
            if (mod.name == modifierName)
                return true;
        }
        return false;
    }

    public bool IsEnemyAlertActive()
    {
        return HasModifier("Enemy Alert");
    }

    public float GetEnemySpawnMultiplier()
    {
        if (IsEnemyAlertActive())
            return 1.5f; // 50% more enemies
        return 1.0f;
    }

    public List<string> GetActiveModifierNames()
    {
        List<string> names = new List<string>();
        foreach (var mod in activeModifiers)
        {
            names.Add(mod.name);
        }
        return names;
    }

    // ==========================================
    // MODIFIER EFFECTS - NEGATIVE
    // ==========================================

    private float originalGunDamage;
    private void ApplyWeaponMalfunction()
    {
        if (playerGun != null)
        {
            originalGunDamage = playerGun.damage;
            playerGun.damage *= 0.5f;
        }
    }
    private void RemoveWeaponMalfunction()
    {
        if (playerGun != null)
            playerGun.damage = originalGunDamage;
    }

    private float originalMoveSpeed;
    private void ApplyHeavyPackage()
    {
        if (playerController != null)
        {
            originalMoveSpeed = playerController.MoveSpeed;
            playerController.MoveSpeed *= 0.7f;
            playerController.SprintSpeed *= 0.7f;
        }
    }
    private void RemoveHeavyPackage()
    {
        if (playerController != null)
        {
            playerController.MoveSpeed = originalMoveSpeed;
            playerController.SprintSpeed = originalMoveSpeed * 1.5f;
        }
    }

    private float packageDamageMultiplier = 1f;
    private void ApplyFragileCargo()
    {
        packageDamageMultiplier = 1.5f;
    }
    private void RemoveFragileCargo()
    {
        packageDamageMultiplier = 1f;
    }

    private float originalDeliveryTime;
    private void ApplyRushOrder()
    {
        if (deliverySystem != null)
        {
            originalDeliveryTime = deliverySystem.currentOrderTime;
            deliverySystem.currentOrderTimeRemaining *= 0.7f;
        }
    }
    private void RemoveRushOrder()
    {
        // Time doesn't restore
    }

    private float originalMaxHealth;
    private void ApplyInjured()
    {
        if (playerController != null)
        {
            originalMaxHealth = playerController.maxHealth;
            playerController.maxHealth *= 0.75f;
            playerController.currentHealth = Mathf.Min(playerController.currentHealth, playerController.maxHealth);
        }
    }
    private void RemoveInjured()
    {
        if (playerController != null)
            playerController.maxHealth = originalMaxHealth;
    }

    private bool enemyAlertActive = false;

    private void ApplyEnemyAlert()
    {
        enemyAlertActive = true;
        Debug.Log("Enemy Alert ACTIVE - More enemies will spawn!");
    }

    private void RemoveEnemyAlert()
    {
        enemyAlertActive = false;
        Debug.Log("Enemy Alert removed");
    }

    private int originalMagSize;
    private void ApplyLowAmmo()
    {
        if (playerGun != null)
        {
            originalMagSize = playerGun.magazineSize;
            playerGun.magazineSize /= 2;
        }
    }
    private void RemoveLowAmmo()
    {
        if (playerGun != null)
            playerGun.magazineSize = originalMagSize;
    }

    // ==========================================
    // MODIFIER EFFECTS - POSITIVE
    // ==========================================

    private void ApplyAdrenalineRush()
    {
        if (playerController != null)
        {
            originalMoveSpeed = playerController.MoveSpeed;
            playerController.MoveSpeed *= 1.3f;
            playerController.SprintSpeed *= 1.3f;
        }
    }
    private void RemoveAdrenalineRush()
    {
        if (playerController != null)
        {
            playerController.MoveSpeed = originalMoveSpeed;
            playerController.SprintSpeed = originalMoveSpeed * 1.5f;
        }
    }

    private void ApplyPremiumClient()
    {
        if (deliverySystem != null)
        {
            deliverySystem.currentOrderReward = Mathf.RoundToInt(deliverySystem.currentOrderReward * 1.5f);
        }
    }
    private void RemovePremiumClient()
    {
        // Reward already modified, no need to remove
    }

    private void ApplyArmoredPackage()
    {
        packageDamageMultiplier = 0.5f;
    }
    private void RemoveArmoredPackage()
    {
        packageDamageMultiplier = 1f;
    }

    private void ApplyExtendedDeadline()
    {
        if (deliverySystem != null)
        {
            deliverySystem.currentOrderTimeRemaining *= 1.5f;
        }
    }
    private void RemoveExtendedDeadline()
    {
        // Time already extended
    }

    private void ApplyPowerSurge()
    {
        if (playerGun != null)
        {
            originalGunDamage = playerGun.damage;
            playerGun.damage *= 1.5f;
        }
    }
    private void RemovePowerSurge()
    {
        if (playerGun != null)
            playerGun.damage = originalGunDamage;
    }

    private Coroutine regenCoroutine;
    private void ApplyLuckyDay()
    {
        regenCoroutine = StartCoroutine(HealthRegen());
    }
    private void RemoveLuckyDay()
    {
        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);
    }

    private System.Collections.IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (playerController != null && playerController.currentHealth < playerController.maxHealth)
            {
                playerController.currentHealth += 2f;
                playerController.currentHealth = Mathf.Min(playerController.currentHealth, playerController.maxHealth);
            }
        }
    }

    // ==========================================
    // MODIFIER EFFECTS - MIXED
    // ==========================================

    private void ApplyGlassCannon()
    {
        if (playerGun != null)
        {
            originalGunDamage = playerGun.damage;
            playerGun.damage *= 2f;
        }
        if (playerController != null)
        {
            originalMaxHealth = playerController.maxHealth;
            playerController.maxHealth *= 0.5f;
            playerController.currentHealth = Mathf.Min(playerController.currentHealth, playerController.maxHealth);
        }
    }
    private void RemoveGlassCannon()
    {
        if (playerGun != null)
            playerGun.damage = originalGunDamage;
        if (playerController != null)
            playerController.maxHealth = originalMaxHealth;
    }

    private void ApplySpeedDemon()
    {
        if (playerController != null)
        {
            originalMoveSpeed = playerController.MoveSpeed;
            playerController.MoveSpeed *= 1.5f;
            playerController.SprintSpeed *= 1.5f;
        }
        packageDamageMultiplier = 1.25f;
    }
    private void RemoveSpeedDemon()
    {
        if (playerController != null)
        {
            playerController.MoveSpeed = originalMoveSpeed;
            playerController.SprintSpeed = originalMoveSpeed * 1.5f;
        }
        packageDamageMultiplier = 1f;
    }

    // Public method for DeliverySystem to apply package damage multiplier
    public float GetPackageDamageMultiplier()
    {
        return packageDamageMultiplier;
    }
}

// Modifier data structure
[System.Serializable]
public class DeliveryModifier
{
    public string name;
    public string description;
    public ModifierType type;
    public System.Action onApply;
    public System.Action onRemove;

    public DeliveryModifier(string name, string desc, ModifierType type, System.Action apply, System.Action remove)
    {
        this.name = name;
        this.description = desc;
        this.type = type;
        this.onApply = apply;
        this.onRemove = remove;
    }
}

public enum ModifierType
{
    Positive,
    Negative,
    Mixed
}