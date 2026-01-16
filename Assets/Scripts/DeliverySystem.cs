using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

// Manages delivery orders, package state, UI, and delivery flow
public class DeliverySystem : MonoBehaviour
{
    // External system references
    [Header("References")]
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
        // Update gameplay systems each frame
        UpdateTargets();
        UpdateCompass();
        UpdateStatus();
        UpdateHPSlider();
        UpdateTimer();

        // Deliver package when close enough and E is pressed
        if (hasPackage && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame)
                DeliverPackage();
        }

        // Fail delivery if package HP reaches zero
        if (hasPackage && currentDeliveryHP <= 0)
            FailDelivery("Package destroyed!");
    }

    // Assigns a new delivery order
    public void AssignOrder(string name, int reward, float timeLimit)
    {
        if (startDeliverySFX) audioSource.PlayOneShot(startDeliverySFX);

        hasActiveOrder = true;
        hasPackage = false;
        currentOrderName = name;
        currentOrderReward = reward;
        currentOrderTime = timeLimit;
        currentOrderTimeRemaining = timeLimit;

        // Spawn package pickup
        itemSpawner?.SpawnItem();

        // Select and enable one random delivery zone
        activeDeliveryZone = deliveryZones[Random.Range(0, deliveryZones.Length)];
        foreach (var zone in deliveryZones)
            zone.SetActive(zone == activeDeliveryZone);

        UpdateUI();
    }

    // Cancels the active order and applies penalties
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

    // Updates remaining delivery time
    void UpdateTimer()
    {
        if (!hasActiveOrder) return;

        currentOrderTimeRemaining -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentOrderTimeRemaining) + "s";

        if (currentOrderTimeRemaining <= 0)
            FailDelivery("Time ran out!");
    }

    // Updates the package HP slider
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

    // Chooses the current navigation target
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

    // Rotates compass arrow toward current target
    void UpdateCompass()
    {
        if (compassArrow == null || currentTarget == null) return;

        Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
        dir.y = 0f;
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        compassArrow.localEulerAngles = new Vector3(0, 0, -angle);
    }

    // Updates on-screen status text
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

    // Applies damage to the package
    public void TakeDamage(float dmg)
    {
        if (!hasPackage) return;
        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP - 10, 0, maxDeliveryHP);
    }

    // Completes the delivery successfully
    void DeliverPackage()
    {
        hasPackage = false;
        hasActiveOrder = false;

        if (deliveryCompleteSFX) audioSource.PlayOneShot(deliveryCompleteSFX);
        StartCoroutine(FadeEffect());

        statusText.text = $"<color=green>Delivery Completed! +{currentOrderReward}</color>";

        PlayerStats.Instance.OnDeliveryCompleted(currentOrderReward, currentDeliveryHP);
        PlayerStats.Instance.ordersLeft--;

        DisableAllDeliveryZones();
        phoneUI?.CloseActiveOrderPanel();
        UpdateUI();
    }

    // Fails the delivery with a reason
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

    // Removes all package objects from the scene
    public void ClearAllPackages()
    {
        foreach (var pkg in GameObject.FindGameObjectsWithTag("Package"))
            Destroy(pkg);
    }

    // Resets compass direction
    public void DisableCompass()
    {
        currentTarget = null;
        if (compassArrow != null)
            compassArrow.localEulerAngles = Vector3.zero;
    }

    // Handles package pickup
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package") && !hasPackage)
        {
            hasPackage = true;
            currentDeliveryHP = maxDeliveryHP;

            if (pickupPackageSFX) audioSource.PlayOneShot(pickupPackageSFX);

            Destroy(other.gameObject);
            statusText.text = "Package retrieved!";
        }
    }

    // Updates simple UI elements
    void UpdateUI()
    {
        timerText.text = hasActiveOrder
            ? Mathf.CeilToInt(currentOrderTimeRemaining).ToString()
            : "";
    }

    // Randomizes delivery zone order
    void ShuffleDeliveryZones()
    {
        for (int i = 0; i < deliveryZones.Length; i++)
        {
            int randomIndex = Random.Range(i, deliveryZones.Length);
            (deliveryZones[i], deliveryZones[randomIndex]) =
                (deliveryZones[randomIndex], deliveryZones[i]);
        }
    }

    // Disables all delivery zones
    void DisableAllDeliveryZones()
    {
        foreach (var zone in deliveryZones)
            zone.SetActive(false);

        activeDeliveryZone = null;
    }

    // Handles screen fade in/out effect
    IEnumerator FadeEffect()
    {
        float t = 0f;
        Color c = fadePanel.color;

        // Fade to black
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t / fadeDuration));
            yield return null;
        }

        yield return new WaitForSeconds(fadeHold);

        // Fade back in
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, t / fadeDuration));
            yield return null;
        }
    }
}
