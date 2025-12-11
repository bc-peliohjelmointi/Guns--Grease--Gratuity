using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class DeliverySystem : MonoBehaviour
{
    [Header("References")]
    public PhoneUI phoneUI;
    public ItemSpawner itemSpawner;

    [Header("Order Info")]
    public bool hasActiveOrder = false;
    public string currentOrderName;
    public int currentOrderReward;
    public float currentOrderTime;
    public float currentOrderTimeRemaining;

    [Header("Delivery Settings")]
    public int deliveryPoints = 0;
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

    [Header("Fade Panel")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public float fadeHold = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip startDeliverySFX;
    public AudioClip pickupPackageSFX;
    public AudioClip deliveryCompleteSFX;

    private GameObject[] deliveryZones;
    private GameObject activeDeliveryZone;  // only the selected zone

    private GameObject currentTarget;

    void Start()
    {
        deliveryZones = GameObject.FindGameObjectsWithTag("DeliveryZone");

        // Make sure all zones are disabled at start
        foreach (var zone in deliveryZones)
            zone.SetActive(false);

        ShuffleDeliveryZones();
        UpdateUI();
    }


    void Update()
    {
        UpdateTargets();
        UpdateCompass();
        UpdateStatus();
        UpdateHPSlider();
        UpdateTimer();

        if (hasPackage && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame)
                DeliverPackage();
        }

        if (hasPackage && currentDeliveryHP <= 0)
            FailDelivery("Paketti tuhoutui!");
    }

    public void AssignOrder(string name, int reward, float timeLimit)
    {
        if (startDeliverySFX) audioSource.PlayOneShot(startDeliverySFX);

        hasActiveOrder = true;
        hasPackage = false;
        currentOrderName = name;
        currentOrderReward = reward;
        currentOrderTime = timeLimit;
        currentOrderTimeRemaining = timeLimit;

        // Spawn the package
        itemSpawner?.SpawnItem();

        // Pick a random delivery zone
        activeDeliveryZone = deliveryZones[Random.Range(0, deliveryZones.Length)];

        // Enable only that one
        foreach (var zone in deliveryZones)
            zone.SetActive(zone == activeDeliveryZone);

        UpdateUI();
    }


    public void CancelOrder()
    {
        hasActiveOrder = false;
        hasPackage = false;
        currentOrderName = "";
        currentOrderReward = 0;
        currentOrderTime = 0;
        currentOrderTimeRemaining = 0;

        PlayerStats.Instance.OnOrderDeclined();
        PlayerStats.Instance.ordersLeft--;

        ClearAllPackages();
        DisableCompass();
        statusText.text = "Ei aktiivista tilausta!";
        UpdateUI();
    }


    void UpdateTimer()
    {
        if (!hasActiveOrder) return;

        currentOrderTimeRemaining -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentOrderTimeRemaining) + "s";

        if (currentOrderTimeRemaining <= 0)
            FailDelivery("Aika loppui!");
    }

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
        if (!hasActiveOrder)
        {
            currentTarget = GameObject.FindGameObjectWithTag("ApartmentDoor");
            return;
        }

        if (!hasPackage)
        {
            var packages = GameObject.FindGameObjectsWithTag("Package");
            currentTarget = packages.FirstOrDefault();
        }
        else
        {
            currentTarget = activeDeliveryZone;
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

    void UpdateStatus()
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
        statusText.text = distance <= deliveryRange
            ? "<color=yellow>Paina [E] toimittaaksesi!</color>"
            : "Toimita paketti!";
    }

    public void TakeDamage(float dmg)
    {
        if (!hasPackage) return;
        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP - dmg, 0, maxDeliveryHP);
    }

    void DeliverPackage()
    {
        hasPackage = false;
        hasActiveOrder = false;

        if (deliveryCompleteSFX) audioSource.PlayOneShot(deliveryCompleteSFX);

        StartCoroutine(FadeEffect());

        statusText.text = $"<color=green>Toimitus onnistui! +{currentOrderReward}</color>";

        PlayerStats.Instance.OnDeliveryCompleted(currentOrderReward, currentDeliveryHP);
        PlayerStats.Instance.ordersLeft--;

        DisableAllDeliveryZones();
        phoneUI?.CloseActiveOrderPanel();
        UpdateUI();
    }


    void FailDelivery(string reason)
    {
        hasPackage = false;
        hasActiveOrder = false;
        statusText.text = $"<color=red>Toimitus epäonnistui! {reason}</color>";

        PlayerStats.Instance.OnDeliveryFailed();
        PlayerStats.Instance.ordersLeft--;

        DisableAllDeliveryZones();
        ClearAllPackages();
        phoneUI?.CloseActiveOrderPanel();
        UpdateUI();
    }

    public void ClearAllPackages()
    {
        foreach (var pkg in GameObject.FindGameObjectsWithTag("Package"))
            Destroy(pkg);
    }

    public void DisableCompass()
    {
        currentTarget = null;
        if (compassArrow != null)
            compassArrow.localEulerAngles = Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package") && !hasPackage)
        {
            hasPackage = true;
            currentDeliveryHP = maxDeliveryHP;

            if (pickupPackageSFX) audioSource.PlayOneShot(pickupPackageSFX);

            Destroy(other.gameObject);
            statusText.text = "Paketti kerätty!";
        }
    }


    void UpdateUI()
    {
        timerText.text = hasActiveOrder
            ? Mathf.CeilToInt(currentOrderTimeRemaining) + ""
            : "";
    }

    void ShuffleDeliveryZones()
    {
        for (int i = 0; i < deliveryZones.Length; i++)
        {
            int randomIndex = Random.Range(i, deliveryZones.Length);
            (deliveryZones[i], deliveryZones[randomIndex]) = (deliveryZones[randomIndex], deliveryZones[i]);
        }
    }

    void DisableAllDeliveryZones()
    {
        foreach (var zone in deliveryZones)
            zone.SetActive(false);

        activeDeliveryZone = null;
    }

    IEnumerator FadeEffect()
    {
        // Fade to black
        float t = 0f;
        Color c = fadePanel.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadePanel.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(fadeHold);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadePanel.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
    }
}
