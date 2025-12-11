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

    private void OnEnable()
    {
        // Called every time the shop UI is opened
        UpdateUI();
    }

    public void BuyDamage()
    {
        if (PlayerStats.Instance.money >= damagePrice)
        {
            PlayerStats.Instance.money -= damagePrice;
            PlayerStats.Instance.weaponDamageLevel++;
            UpdateUI();
        }
    }

    public void BuyDeliveryTime()
    {
        if (PlayerStats.Instance.money >= deliveryTimePrice)
        {
            PlayerStats.Instance.money -= deliveryTimePrice;
            PlayerStats.Instance.deliveryTimeLevel++;
            UpdateUI();
        }
    }

    public void BuyScooterSpeed()
    {
        if (PlayerStats.Instance.money >= scooterSpeedPrice)
        {
            PlayerStats.Instance.money -= scooterSpeedPrice;
            PlayerStats.Instance.scooterSpeedLevel++;
            UpdateUI();
        }
    }

    public void BuyRewardMultiplier()
    {
        if (PlayerStats.Instance.money >= rewardMultiplierPrice)
        {
            PlayerStats.Instance.money -= rewardMultiplierPrice;
            PlayerStats.Instance.rewardMultiplierLevel++;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (PlayerStats.Instance == null) return;

        moneyText.text = "Money: $" + PlayerStats.Instance.money;

        damageText.text = "Level " + PlayerStats.Instance.weaponDamageLevel
                          + " (Cost $" + damagePrice + ")";

        timeText.text = "Level " + PlayerStats.Instance.deliveryTimeLevel
                        + " (Cost $" + deliveryTimePrice + ")";

        speedText.text = "Level " + PlayerStats.Instance.scooterSpeedLevel
                         + " (Cost $" + scooterSpeedPrice + ")";

        rewardText.text = "Level " + PlayerStats.Instance.rewardMultiplierLevel
                          + " (Cost $" + rewardMultiplierPrice + ")";
    }
}
