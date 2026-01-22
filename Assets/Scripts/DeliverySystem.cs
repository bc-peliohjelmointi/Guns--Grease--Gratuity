using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

// Manages delivery orders, package state, UI, and delivery flow
public class DeliverySystem : MonoBehaviour
{
    // External system references
    [Header("References")]
    public StairwellTeleportManager stairwellTeleportManager;
    public PhoneUI phoneUI;
    public ItemSpawner itemSpawner;

    // Current order data
    [Header("Order Info")]
    public bool hasActiveOrder = false;
    public string currentOrderName;
    public int currentOrderReward;
    public float currentOrderTime;
    public float currentOrderTimeRemaining;

    // Delivery state and interaction settings
    [Header("Delivery Settings")]
    public int deliveryPoints = 0;
    public bool hasPackage = false;
    public float deliveryRange = 3f;

    // Package health values
    [Header("Delivery HP")]
    public float maxDeliveryHP = 100f;
    public float currentDeliveryHP;

    // UI references
    [Header("UI")]
    public TextMeshProUGUI statusText;
    public Slider hpSlider;
    public RectTransform compassArrow;
    public TextMeshProUGUI timerText;

    // Screen fade effect settings
    [Header("Fade Panel")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public float fadeHold = 1.5f;
    private bool isTeleporting = false;

    // Audio feedback
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip startDeliverySFX;
    public AudioClip pickupPackageSFX;
    public AudioClip deliveryCompleteSFX;

    // Delivery zone handling
    private GameObject[] deliveryZones;
    private GameObject activeDeliveryZone;

    // Current navigation target (package or delivery zone)
    private GameObject currentTarget;

    void Start()
    {
        // Cache all delivery zones and disable them initially
        deliveryZones = GameObject.FindGameObjectsWithTag("DeliveryZone");
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

        // Deliver package
        if (hasPackage && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame && !isTeleporting)
            {
                StartCoroutine(TeleportAndDeliver());
            }
        }

        // Fail delivery if package HP reaches zero
        if (hasPackage && currentDeliveryHP <= 0)
        {
            FailDelivery("Package destroyed!");
        }
    }

    // Assigns a new delivery order
    public void AssignOrder(string name, int reward, float timeLimit)
    {
        if (startDeliverySFX)
            audioSource.PlayOneShot(startDeliverySFX);

        hasActiveOrder = true;
        hasPackage = false;

        currentOrderName = name;
        currentOrderReward = reward;
        currentOrderTime = timeLimit;
        currentOrderTimeRemaining = timeLimit;

        // Spawn package
        itemSpawner?.SpawnItem();

        // Select random delivery zone
        activeDeliveryZone = deliveryZones[Random.Range(0, deliveryZones.Length)];
        foreach (var zone in deliveryZones)
            zone.SetActive(zone == activeDeliveryZone);

        UpdateUI();
    }

    // Cancels the active order
    public void CancelOrder()
    {
        hasActiveOrder = false;
        hasPackage = false;

        PlayerStats.Instance.OnOrderDeclined();
        PlayerStats.Instance.ordersLeft--;

        ClearAllPackages();
        DisableCompass();

        statusText.text = "No active order!";
        UpdateUI();
    }

    void UpdateTimer()
    {
        if (!hasActiveOrder)
            return;

        currentOrderTimeRemaining -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentOrderTimeRemaining) + "s";

        if (currentOrderTimeRemaining <= 0)
        {
            FailDelivery("Time ran out!");
        }
    }

    void UpdateHPSlider()
    {
        if (hpSlider == null)
            return;

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
            currentTarget = GameObject.FindGameObjectsWithTag("Package").FirstOrDefault();
        else
            currentTarget = activeDeliveryZone;
    }

    void UpdateCompass()
    {
        if (compassArrow == null || currentTarget == null)
            return;

        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
        dir.y = 0f;

        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        compassArrow.localEulerAngles = new Vector3(0, 0, -angle);
    }

    void UpdateStatus()
    {
        if (!hasActiveOrder)
        {
            statusText.text = "No active order!";
            return;
        }

        if (!hasPackage)
        {
            statusText.text = "Retrieve the package!";
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        statusText.text = distance <= deliveryRange
            ? "<color=yellow>[E] to deliver!!</color>"
            : "Deliver package!";
    }

    public void TakeDamage(float dmg)
    {
        if (!hasPackage)
            return;

        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP - dmg, 0, maxDeliveryHP);
    }

    void DeliverPackage()
    {
        hasPackage = false;
        hasActiveOrder = false;

        if (deliveryCompleteSFX)
            audioSource.PlayOneShot(deliveryCompleteSFX);

        StartCoroutine(FadeIn());

        statusText.text = $"<color=green>Delivery Completed! +{currentOrderReward}</color>";

        PlayerStats.Instance.OnDeliveryCompleted(currentOrderReward, currentDeliveryHP);
        PlayerStats.Instance.ordersLeft--;

        DisableAllDeliveryZones();
        phoneUI?.CloseActiveOrderPanel();

        UpdateUI();
        StartCoroutine(FadeOut());
    }

    void FailDelivery(string reason)
    {
        hasPackage = false;
        hasActiveOrder = false;

        statusText.text = $"<color=red>Delivery failed! {reason}</color>";

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

            if (pickupPackageSFX)
                audioSource.PlayOneShot(pickupPackageSFX);

            Destroy(other.gameObject);
            statusText.text = "Package retrieved!";
        }
    }

    void UpdateUI()
    {
        timerText.text = hasActiveOrder
            ? Mathf.CeilToInt(currentOrderTimeRemaining).ToString()
            : "";
    }

    void ShuffleDeliveryZones()
    {
        for (int i = 0; i < deliveryZones.Length; i++)
        {
            int randomIndex = Random.Range(i, deliveryZones.Length);
            (deliveryZones[i], deliveryZones[randomIndex]) =
                (deliveryZones[randomIndex], deliveryZones[i]);
        }
    }

    void DisableAllDeliveryZones()
    {
        foreach (var zone in deliveryZones)
            zone.SetActive(false);

        activeDeliveryZone = null;
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;
        Color c = fadePanel.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t / fadeDuration));
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(fadeHold);

        float t = 0f;
        Color c = fadePanel.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / fadeDuration));
            yield return null;
        }
    }

    private IEnumerator TeleportAndDeliver()
    {
        isTeleporting = true;

        // 1) Fade out
        yield return StartCoroutine(FadeOut());

        // 2) Teleport
        stairwellTeleportManager.TeleportPlayer(transform);

        // 3) Deliver
        DeliverPackage();

        // 4) Fade back in
        yield return StartCoroutine(FadeIn());

        isTeleporting = false;
    }
}
