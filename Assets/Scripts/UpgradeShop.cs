using UnityEngine;
using TMPro;

public class UpgradeShop : MonoBehaviour
{
    // Prices for each upgrade type
    [Header("Upgrade Prices")]
    public int damagePrice = 100;
    public int deliveryTimePrice = 50;
    public int scooterSpeedPrice = 50;
    public int rewardMultiplierPrice = 100;

    // UI text references
    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rewardText;

    // Called whenever the shop UI becomes active
    private void OnEnable()
    {
        UpdateUI();
    }

    // Purchase weapon damage upgrade
    public void BuyDamage()
    {
        if (PlayerStats.Instance.money >= damagePrice)
        {
            PlayerStats.Instance.money -= damagePrice;
            PlayerStats.Instance.weaponDamageLevel++;
            UpdateUI();
        }
    }

    // Purchase delivery time upgrade
    public void BuyDeliveryTime()
    {
        if (PlayerStats.Instance.money >= deliveryTimePrice)
        {
            PlayerStats.Instance.money -= deliveryTimePrice;
            PlayerStats.Instance.deliveryTimeLevel++;
            UpdateUI();
        }
    }

    // Purchase scooter speed upgrade
    public void BuyScooterSpeed()
    {
        if (PlayerStats.Instance.money >= scooterSpeedPrice)
        {
            PlayerStats.Instance.money -= scooterSpeedPrice;
            PlayerStats.Instance.scooterSpeedLevel++;
            UpdateUI();
        }
    }

    // Purchase reward multiplier upgrade
    public void BuyRewardMultiplier()
    {
        if (PlayerStats.Instance.money >= rewardMultiplierPrice)
        {
            PlayerStats.Instance.money -= rewardMultiplierPrice;
            PlayerStats.Instance.rewardMultiplierLevel++;
            UpdateUI();
        }
    }

    // Refreshes all shop UI text values
    public void UpdateUI()
    {
        // Safety check in case PlayerStats is not initialized
        if (PlayerStats.Instance == null) return;

        moneyText.text = "Money: $" + PlayerStats.Instance.money;

        damageText.text = "Level " + PlayerStats.Instance.weaponDamageLevel +
                          " (Cost $" + damagePrice + ")";

        timeText.text = "Level " + PlayerStats.Instance.deliveryTimeLevel +
                        " (Cost $" + deliveryTimePrice + ")";

        speedText.text = "Level " + PlayerStats.Instance.scooterSpeedLevel +
                         " (Cost $" + scooterSpeedPrice + ")";

        rewardText.text = "Level " + PlayerStats.Instance.rewardMultiplierLevel +
                          " (Cost $" + rewardMultiplierPrice + ")";
    }
}
