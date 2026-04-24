using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    [Header("Upgrade Prices")]
    public int damagePrice = 100;
    public int bodyArmorPrice = 50;
    public int scooterSpeedPrice = 50;
    public int rewardMultiplierPrice = 100;
    private int maxLevel = 5;

    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rewardText;

    private PlayerStats stats;

    [Header("Sliders")]
    public Slider damageSlider;
    public Slider timeSlider;
    public Slider speedSlider;
    public Slider rewardSlider;

    private void Start()
    {
        // Cache PlayerStats once it definitely exists
        stats = PlayerStats.Instance;
        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    public void RefreshUI()
    {
        if (stats == null)
            stats = PlayerStats.Instance;

        UpdateUI();
    }



    // -------------------------
    // BUY METHODS
    // -------------------------

    public void BuyBodyArmor()
    {
        if (stats.money >= bodyArmorPrice && stats.bodyArmorLevel < maxLevel)
        {
            stats.money -= bodyArmorPrice;
            stats.bodyArmorLevel++;
            UpdateUI();
        }
    }

    public void BuyDamage()
    {
        if (stats.money >= damagePrice && stats.weaponDamageLevel < maxLevel)
        {
            stats.money -= damagePrice;
            stats.weaponDamageLevel++;
            UpdateUI();
        }
    }

    public void BuyScooterSpeed()
    {
        if (stats.money >= scooterSpeedPrice && stats.scooterSpeedLevel < maxLevel)
        {
            stats.money -= scooterSpeedPrice;
            stats.scooterSpeedLevel++;
            UpdateUI();
        }
    }

    public void BuyPackageHealth()
    {
        if (stats.money >= rewardMultiplierPrice && stats.packageHealthLevel < maxLevel)
        {
            stats.money -= rewardMultiplierPrice;
            stats.packageHealthLevel++;
            UpdateUI();
        }
    }

    // -------------------------
    // UI
    // -------------------------

    void UpdateMoneyOnly()
    {
        if (stats == null) return;
        moneyText.text = $"Money: ${Mathf.FloorToInt(stats.money)}";
    }

    void UpdateUI()
    {
        if (stats == null) return;

        UpdateMoneyOnly();

        // Prices update
        damageText.text = $"${damagePrice}";
        timeText.text = $"${bodyArmorPrice}";
        speedText.text = $"${scooterSpeedPrice}";
        rewardText.text = $"${rewardMultiplierPrice}";

        // Sliders logic
        if (damageSlider != null)
        {
            damageSlider.maxValue = maxLevel;
            damageSlider.value = stats.weaponDamageLevel;
        }

        if (timeSlider != null)
        {
            timeSlider.maxValue = maxLevel;
            timeSlider.value = stats.bodyArmorLevel;
        }

        if (speedSlider != null)
        {
            speedSlider.maxValue = maxLevel;
            speedSlider.value = stats.scooterSpeedLevel;
        }

        if (rewardSlider != null)
        {
            rewardSlider.maxValue = maxLevel;
            rewardSlider.value = stats.packageHealthLevel;
        }
    }
}
