using UnityEngine;
using TMPro;

public class UpgradeShop : MonoBehaviour
{
    [Header("Upgrade Prices")]
    public int damagePrice = 200;
    public int deliveryTimePrice = 150;
    public int scooterSpeedPrice = 250;
    public int rewardMultiplierPrice = 300;

    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rewardText;

    private void Start()
    {
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

    void UpdateUI()
    {
        moneyText.text = "Money: $" + PlayerStats.Instance.money;

        damageText.text = "Damage Lv. " + PlayerStats.Instance.weaponDamageLevel 
                          + " (Cost $" + damagePrice + ")";

        timeText.text = "Delivery Time Lv. " + PlayerStats.Instance.deliveryTimeLevel
                        + " (Cost $" + deliveryTimePrice + ")";

        speedText.text = "Scooter Speed Lv. " + PlayerStats.Instance.scooterSpeedLevel
                         + " (Cost $" + scooterSpeedPrice + ")";

        rewardText.text = "Reward Multiplier Lv. " + PlayerStats.Instance.rewardMultiplierLevel
                          + " (Cost $" + rewardMultiplierPrice + ")";
    }
}
