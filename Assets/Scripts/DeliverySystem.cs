using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.UI;

public class DeliverySystem : MonoBehaviour
{
    [Header("References")]
    public PhoneUI phoneUI;

    [Header("Active Order")]
    public bool hasActiveOrder = false;
    public string currentOrderName;
    public int currentOrderReward;
    public float currentOrderTime;
    public float currentOrderTimeRemaining;

    [Header("Delivery Settings")]
    public int deliveryPoints = 0;
    public int reputation = 100;
    public bool hasPackage = false;
    public float deliveryRange = 3f;

    [Header("Delivery HP")]
    public float maxDeliveryHP = 100f;
    public float currentDeliveryHP;

    [Header("UI")]
    public TextMeshProUGUI statusText;
    public Slider hpSlider;
    public RectTransform compassArrow;
    public TextMeshProUGUI timerText;

    private GameObject[] deliveryZones;
    private GameObject[] packages;
    private GameObject currentTarget;

    [Header("Spawner")]
    public ItemSpawner itemSpawner;

    void Start()
    {
        deliveryZones = GameObject.FindGameObjectsWithTag("DeliveryZone");
        packages = GameObject.FindGameObjectsWithTag("Package");
        UpdateUI();
    }

    void Update()
    {
        UpdateTargets();
        UpdateCompass();
        UpdateDistanceAndStatus();
        UpdateHPSlider();
        UpdateOrderTimer();

        // Deliver
        if (hasPackage && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame)
            {
                DeliverPackage();
            }
        }

        // Package destroyed
        if (hasPackage && currentDeliveryHP <= 0)
        {
            FailDelivery("Paketti tuhoutui!");        }
    }

    // ========================
    //    ORDER SYSTEM LINK
    // ========================
    public void AssignOrder(string name, int reward, float timeLimit)
    {
        hasActiveOrder = true;
        currentOrderName = name;
        currentOrderReward = reward;
        currentOrderTime = timeLimit;
        currentOrderTimeRemaining = timeLimit;

        // ✅ Spawn package at random spawner
        if (itemSpawner != null)
            itemSpawner.SpawnItem();

        UpdateUI();
    }


    void UpdateOrderTimer()
    {
        if (!hasActiveOrder) return;

        currentOrderTimeRemaining -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(currentOrderTimeRemaining) + "s";

        if (currentOrderTimeRemaining <= 0)
        {
            FailDelivery("Aika loppui!");
        }
    }

    // ========================

    void UpdateHPSlider()
    {
        if (hpSlider == null) return;

        hpSlider.gameObject.SetActive(hasPackage);
        if (hasPackage)
        {
            hpSlider.maxValue = maxDeliveryHP;
            hpSlider.value = currentDeliveryHP;
        }
    }

    void UpdateTargets()
    {
        if (!hasPackage) // find packages
        {
            packages = GameObject.FindGameObjectsWithTag("Package");
            currentTarget = (packages.Length > 0) ?
                packages.OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).First() :
                null;
        }
        else // find delivery zones
        {
            currentTarget = (deliveryZones.Length > 0) ?
                deliveryZones.OrderBy(z => Vector3.Distance(transform.position, z.transform.position)).First() :
                null;
        }
    }

    void UpdateCompass()
    {
        if (compassArrow == null || currentTarget == null) return;

        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
        dir.y = 0f;

        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        compassArrow.localEulerAngles = new Vector3(0, 0, -angle);
    }

    void UpdateDistanceAndStatus()
    {
        if (!hasActiveOrder)
        {
            statusText.text = "Ei aktiivista tilausta!";
            return;
        }

        if (!hasPackage)
        {
            statusText.text = "Hae paketti!";
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distance <= deliveryRange)
            statusText.text = "<color=yellow>Paina [E] toimittaaksesi!</color>";
        else
            statusText.text = "Toimita paketti!";
    }

    public void TakeDamage(float dmg)
    {
        if (!hasPackage) return;

        currentDeliveryHP -= dmg;
        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP, 0, maxDeliveryHP);
    }

    void DeliverPackage()
    {
        hasPackage = false;
        hasActiveOrder = false;

        deliveryPoints += currentOrderReward;

        statusText.text = "<color=green>Toimitus onnistui! +" + currentOrderReward + "</color>";

        if (phoneUI != null)
            phoneUI.CloseActiveOrderPanel();

        UpdateUI();
    }

    void FailDelivery(string reason)
    {
        hasPackage = false;
        hasActiveOrder = false;

        statusText.text = "<color=red>Toimitus epäonnistui! " + reason + "</color>";

        if (phoneUI != null)
            phoneUI.CloseActiveOrderPanel();

        UpdateUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package") && !hasPackage)
        {
            hasPackage = true;
            currentDeliveryHP = maxDeliveryHP;
            Destroy(other.gameObject);

            statusText.text = "Paketti kerätty!";
        }
    }

    void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = hasActiveOrder ? Mathf.CeilToInt(currentOrderTimeRemaining) + "s" : "";
        }
    }
}