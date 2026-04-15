using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeliveryModifierSystem : MonoBehaviour
{
    [Header("Modifier Settings")]
    [Range(0f, 1f)]
    public float modifierChance = 0.5f;
    public bool allowMultipleModifiers = false;
    public int maxModifiers = 2;

    [Header("UI References")]
    public TextMeshProUGUI modifierNotificationText;
    public GameObject modifierPanel;
    public float notificationDuration = 5f;

    [Header("Player References")]
    public StarterAssets.FirstPersonController playerController;
    public PlayerHealth playerHealth; // NEW: Reference to PlayerHealth component
    public DeliverySystem deliverySystem;
    public GunHitscan playerGun;

    private List<DeliveryModifier> activeModifiers = new List<DeliveryModifier>();
    private DeliveryModifier[] allModifiers;

    private void Start()
    {
        // Auto-find PlayerHealth if not assigned
        if (playerHealth == null && playerController != null)
        {
            playerHealth = playerController.GetComponent<PlayerHealth>();
        }

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
            
            // MIXED MODIFIERS
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

    public void RollModifiers()
    {
        ClearAllModifiers();

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
            if (Random.value <= modifierChance)
            {
                ApplyRandomModifier();
            }
        }

        if (activeModifiers.Count > 0)
        {
            ShowModifierNotification();
        }
    }

    private void ApplyRandomModifier()
    {
        DeliveryModifier modifier = allModifiers[Random.Range(0, allModifiers.Length)];

        if (activeModifiers.Contains(modifier))
            return;

        modifier.onApply?.Invoke();
        activeModifiers.Add(modifier);

        Debug.Log($"Applied modifier: {modifier.name}");
    }

    private Coroutine hideCoroutine;

    private void ShowModifierNotification()
    {
        if (modifierNotificationText == null)
            return;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        string notification = "<size=36><b>DELIVERY MODIFIERS:</b></size>\n\n";
        foreach (var mod in activeModifiers)
        {
            string color = mod.type == ModifierType.Positive ? "green" :
                          mod.type == ModifierType.Negative ? "red" : "yellow";
            notification += $"<color={color}><b>{mod.name}</b></color>\n{mod.description}\n\n";
        }

        modifierNotificationText.text = notification;

        if (modifierPanel != null)
        {
            modifierPanel.SetActive(true);
            hideCoroutine = StartCoroutine(HideModifierPanelAfterDelay());
        }
    }

    private System.Collections.IEnumerator HideModifierPanelAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        HideModifierPanel();
        hideCoroutine = null;
    }

    private void HideModifierPanel()
    {
        if (modifierPanel != null)
            modifierPanel.SetActive(false);
    }

    public void ClearAllModifiers()
    {
        foreach (var modifier in activeModifiers)
        {
            modifier.onRemove?.Invoke();
        }
        activeModifiers.Clear();
    }

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
            return 1.5f;
        return 1.0f;
    }

    public float GetPackageDamageMultiplier()
    {
        return packageDamageMultiplier;
    }

    // ==========================================
    // MODIFIER EFFECTS
    // ==========================================

    private float originalGunDamage;
    private float originalMoveSpeed;
    private float originalMaxHealth;
    private int originalMagSize;
    private float packageDamageMultiplier = 1f;

    // WEAPON MALFUNCTION
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

    // HEAVY PACKAGE
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

    // FRAGILE CARGO
    private void ApplyFragileCargo()
    {
        packageDamageMultiplier = 1.5f;
    }
    private void RemoveFragileCargo()
    {
        packageDamageMultiplier = 1f;
    }

    // RUSH ORDER
    private void ApplyRushOrder()
    {
        if (deliverySystem != null)
        {
            deliverySystem.currentOrderTimeRemaining *= 0.7f;
        }
    }
    private void RemoveRushOrder()
    {
        // Time doesn't restore
    }

    // INJURED - UPDATED for PlayerHealth
    private void ApplyInjured()
    {
        if (playerHealth != null)
        {
            originalMaxHealth = playerHealth.maxHealth;
            playerHealth.SetMaxHealth(playerHealth.maxHealth * 0.75f);
        }
    }
    private void RemoveInjured()
    {
        if (playerHealth != null)
            playerHealth.SetMaxHealth(originalMaxHealth);
    }

    // ENEMY ALERT
    private void ApplyEnemyAlert()
    {
        Debug.Log("Enemy Alert ACTIVE - More enemies will spawn!");
    }
    private void RemoveEnemyAlert()
    {
        Debug.Log("Enemy Alert removed");
    }

    // LOW AMMO
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

    // ADRENALINE RUSH
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

    // PREMIUM CLIENT
    private void ApplyPremiumClient()
    {
        if (deliverySystem != null)
        {
            deliverySystem.currentOrderReward = Mathf.RoundToInt(deliverySystem.currentOrderReward * 1.5f);
        }
    }
    private void RemovePremiumClient()
    {
        // Reward already modified
    }

    // ARMORED PACKAGE
    private void ApplyArmoredPackage()
    {
        packageDamageMultiplier = 0.5f;
    }
    private void RemoveArmoredPackage()
    {
        packageDamageMultiplier = 1f;
    }

    // EXTENDED DEADLINE
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

    // POWER SURGE
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

    // LUCKY DAY - UPDATED for PlayerHealth
    private void ApplyLuckyDay()
    {
        if (playerHealth != null)
        {
            playerHealth.enableHealthRegen = true;
            playerHealth.regenRate = 2f;
            playerHealth.regenDelay = 3f;
        }
    }
    private void RemoveLuckyDay()
    {
        if (playerHealth != null)
        {
            playerHealth.enableHealthRegen = false;
        }
    }

    // GLASS CANNON - UPDATED for PlayerHealth
    private void ApplyGlassCannon()
    {
        if (playerGun != null)
        {
            originalGunDamage = playerGun.damage;
            playerGun.damage *= 2f;
        }
        if (playerHealth != null)
        {
            originalMaxHealth = playerHealth.maxHealth;
            playerHealth.SetMaxHealth(playerHealth.maxHealth * 0.5f);
        }
    }
    private void RemoveGlassCannon()
    {
        if (playerGun != null)
            playerGun.damage = originalGunDamage;
        if (playerHealth != null)
            playerHealth.SetMaxHealth(originalMaxHealth);
    }

    // SPEED DEMON
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
}

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