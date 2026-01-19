using UnityEngine;
using TMPro;

public class UpgradeShop : MonoBehaviour
{
    [Header("Upgrade Prices")]
    public int damagePrice = 100;
    public int deliveryTimePrice = 50;
    public int scooterSpeedPrice = 50;
    public int rewardMultiplierPrice = 100;

    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rewardText;

    private PlayerStats stats;

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

    private void Update()
    {
        // Keep money text always correct while shop is open
        UpdateMoneyOnly();
    }

    // -------------------------
    // BUY METHODS
    // -------------------------

    public void BuyDamage()
    {
        if (stats.money >= damagePrice)
        {
            stats.money -= damagePrice;
            stats.weaponDamageLevel++;
            UpdateUI();
        }
    }

    public void BuyDeliveryTime()
    {
        if (stats.money >= deliveryTimePrice)
        {
            stats.money -= deliveryTimePrice;
            stats.deliveryTimeLevel++;
            UpdateUI();
        }
    }

    public void BuyScooterSpeed()
    {
        if (stats.money >= scooterSpeedPrice)
        {
            stats.money -= scooterSpeedPrice;
            stats.scooterSpeedLevel++;
            UpdateUI();
        }
    }

    public void BuyRewardMultiplier()
    {
        if (stats.money >= rewardMultiplierPrice)
        {
            stats.money -= rewardMultiplierPrice;
            stats.rewardMultiplierLevel++;
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

        damageText.text = $"Level {stats.weaponDamageLevel} (Cost ${damagePrice})";
        timeText.text = $"Level {stats.deliveryTimeLevel} (Cost ${deliveryTimePrice})";
        speedText.text = $"Level {stats.scooterSpeedLevel} (Cost ${scooterSpeedPrice})";
        rewardText.text = $"Level {stats.rewardMultiplierLevel} (Cost ${rewardMultiplierPrice})";
    }
}
