using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Day & Deliveries")]
    public int currentDay = 1;
    public int deliveriesCompleted = 0;
    public int deliveriesFailed = 0;
    public int ordersLeft;

    [Header("Economy")]
    public float money = 0f;
    public float moneyToday = 0f;

    [Header("Reputation")]
    [Range(0f, 5f)]
    public float reputation = 2.5f;   // Start neutral
    public float repGain = 0.2f;
    public float repLoss = 0.3f;

    [Header("Package Health")]
    public float baseDeliveryHP = 100f;
    public float maxDeliveryHP = 100f;
    public float lastPackageHealth = 100f;

    // ----------------------
    // UPGRADE LEVELS
    // ----------------------
    [Header("Upgrade Levels")]
    public int bodyArmorLevel = 0;       // Affects delivery time bonus
    public int weaponDamageLevel = 0;       // Affects gun damage
    public int scooterSpeedLevel = 0;       // Affects scooter speed
    public int packageHealthLevel = 0;      // Increases delivery package health


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ordersLeft = GetDailyOrderLimit();

        DontDestroyOnLoad(gameObject);
    }

    // ----------------------
    // MONEY
    // ----------------------
    public void AddMoney(float amount)
    {
        money += amount;
        moneyToday += amount;
    }

    // ----------------------
    // DELIVERY RESULT
    // ----------------------
    public void OnDeliveryCompleted(float baseReward, float packageHealth)
    {
        deliveriesCompleted++;
        lastPackageHealth = packageHealth;

        // Apply reward multiplier from upgrades
        int rank = Mathf.FloorToInt(reputation);
        float rankBonus = rank * 0.10f;

        float finalReward =
            baseReward *
            (1f + rankBonus);


        AddMoney(finalReward);

        AddReputation(repGain);
    }

    public void OnDeliveryFailed()
    {
        deliveriesFailed++;

        AddReputation(-repLoss);
    }

    public void OnOrderDeclined()
    {
        AddReputation(-repLoss);
    }

    // ----------------------
    // REPUTATION
    // ----------------------
    public void AddReputation(float amount)
    {
        reputation = Mathf.Clamp(reputation + amount, 0f, 5f);
    }

    // ----------------------
    // DAY RESET
    // ----------------------
    public void ResetDayStats()
    {
        moneyToday = 0f;
        deliveriesCompleted = 0;
        deliveriesFailed = 0;
        ordersLeft = GetDailyOrderLimit();

        RoadworkManager.Instance?.GenerateNewDayRoadworks();
    }

    public int GetDailyOrderLimit()
    {
        int rank = Mathf.FloorToInt(reputation);
        return 4; // base 3 + 1 per rank
    }
}
