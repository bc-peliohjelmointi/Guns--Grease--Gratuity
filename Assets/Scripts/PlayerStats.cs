using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Day & Deliveries")]
    public int currentDay = 1;
    public int deliveriesCompleted = 0;
    public int deliveriesFailed = 0;

    [Header("Economy")]
    public float money = 0f;
    public float moneyToday = 0f;

    [Header("Reputation")]
    [Range(0f, 5f)]
    public float reputation = 2.5f;   // Start neutral
    public float repGain = 0.1f;
    public float repLoss = 0.3f;

    [Header("Package Health")]
    public float lastPackageHealth = 100f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // ----------------------
    // Stats modifications
    // ----------------------

    public void AddMoney(float amount)
    {
        money += amount;
        moneyToday += amount;
    }

    public void OnDeliveryCompleted(float reward, float packageHealth)
    {
        deliveriesCompleted++;
        lastPackageHealth = packageHealth;

        AddMoney(reward);
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

    public void AddReputation(float amount)
    {
        reputation = Mathf.Clamp(reputation + amount, 0f, 5f);
    }

    public void ResetDayStats()
    {
        moneyToday = 0f;
        deliveriesCompleted = 0;
        deliveriesFailed = 0;
    }
}
