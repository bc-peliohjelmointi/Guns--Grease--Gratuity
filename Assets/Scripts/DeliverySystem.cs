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
    public StairwellTeleportManager teleportManager;
    public PhoneUI phoneUI;
    public ItemSpawner itemSpawner;
    [HideInInspector] public bool playerAtExitPoint = false;

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

    // Exitpoint handling
    private GameObject[] exitPoints;
    private Transform activeExitPoint;

    // Current navigation target (package or delivery zone)
    private GameObject currentTarget;

    public bool pendingDelivery = false;

    void Start()
    {
        deliveryZones = GameObject.FindGameObjectsWithTag("DeliveryZone");
        foreach (var zone in deliveryZones) zone.SetActive(false);

        exitPoints = GameObject.FindGameObjectsWithTag("ExitPoint");
        DisableExitPoints();

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
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame && !isTeleporting)
            {
                StartCoroutine(TeleportToEntry());
            }
        }

        if (hasPackage && currentDeliveryHP <= 0)
        {
            FailDelivery("Package destroyed!");
        }
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

        itemSpawner?.SpawnItem();

        activeDeliveryZone = deliveryZones[Random.Range(0, deliveryZones.Length)];
        foreach (var zone in deliveryZones)
            zone.SetActive(zone == activeDeliveryZone);

        activeExitPoint = exitPoints.Length > 0 ? exitPoints[0].transform : null;
        UpdateUI();
    }

    void UpdateTimer()
    {
        if (!hasActiveOrder) return;

        currentOrderTimeRemaining -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentOrderTimeRemaining) + "s";

        if (currentOrderTimeRemaining <= 0)
        {
            FailDelivery("Time ran out!");
        }
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
            currentTarget = GameObject.FindGameObjectsWithTag("Package").FirstOrDefault();
            return;
        }

        // when player has package, currentTarget is delivery zone
        currentTarget = activeDeliveryZone;
    }

    public Transform GetCompassTarget()
    {
        if (!hasActiveOrder) return GameObject.FindGameObjectWithTag("ApartmentDoor")?.transform;

        if (!hasPackage)
            return GameObject.FindGameObjectsWithTag("Package").FirstOrDefault()?.transform;

        if (teleportManager.isInStairwell)
            return activeExitPoint; // points to exit point while inside

        return activeDeliveryZone?.transform;
    }

    void UpdateCompass()
    {
        Transform target = GetCompassTarget();
        if (compassArrow == null || target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
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

        // Exit point prompt
        if (pendingDelivery && playerAtExitPoint && teleportManager.isInStairwell)
        {
            statusText.text = "<color=yellow>[E] Deliver</color>";
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        statusText.text = distance <= deliveryRange
            ? "<color=yellow>[E] Enter</color>"
            : "Deliver package!";
    }


    public void TakeDamage(float dmg)
    {
        if (!hasPackage) return;
        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP - dmg, 0, maxDeliveryHP);
    }

    public void DeliverPackage()
    {
        hasPackage = false;
        hasActiveOrder = false;

        if (deliveryCompleteSFX) audioSource.PlayOneShot(deliveryCompleteSFX);

        statusText.text = $"<color=green>Delivery Completed! +{currentOrderReward}</color>";

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

        statusText.text = $"<color=red>Delivery failed! {reason}</color>";

        PlayerStats.Instance.OnDeliveryFailed();
        PlayerStats.Instance.ordersLeft--;

        if (teleportManager.isInStairwell == true)
        {
            StartCoroutine(TeleportOutOfStairwell());
        }
    }

    public void CancelOrder()
    {
        hasActiveOrder = false;
        hasPackage = false;

        PlayerStats.Instance.OnOrderDeclined();
        PlayerStats.Instance.ordersLeft--;

        if (teleportManager.isInStairwell == true)
        {
            StartCoroutine(TeleportOutOfStairwell());
            return;
        }
    }

    private void CleanupAfterCancel()
    {
        DisableAllDeliveryZones();
        ClearAllPackages();
        DisableCompass();

        statusText.text = "No active order!";
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
        activeExitPoint = null;
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

    private IEnumerator TeleportToEntry()
    {
        isTeleporting = true;

        yield return StartCoroutine(FadeOut());
        teleportManager.TeleportToStairwell(transform);

        ShuffleExitPoints();
        SelectActiveExitPoint();
        pendingDelivery = true;

        yield return StartCoroutine(FadeIn());
        isTeleporting = false;
    }

    private IEnumerator TeleportOutOfStairwell()
    {
        if (teleportManager.isInStairwell)
        {
            yield return StartCoroutine(FadeOut());

            teleportManager.TeleportToDeliveryZone(transform);
            teleportManager.isInStairwell = false;
            CleanupAfterCancel();

            yield return StartCoroutine(FadeIn());
        }
    }

    public Transform GetActiveDeliveryExitPoint()
    {
        if (activeDeliveryZone == null) return null;

        GameObject[] exitPoints = GameObject.FindGameObjectsWithTag("ExitPoint");
        if (exitPoints.Length == 0) return null;

        Transform closest = null;
        float closestDist = float.MaxValue;
        Vector3 zonePos = activeDeliveryZone.transform.position;

        foreach (GameObject ep in exitPoints)
        {
            float dist = Vector3.Distance(zonePos, ep.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = ep.transform;
            }
        }

        return closest;
    }

    public Transform GetActiveDeliveryZoneTransform()
    {
        return activeDeliveryZone != null ? activeDeliveryZone.transform : null;
    }

    public Transform GetActiveExitPoint()
    {
        return activeExitPoint;
    }

    void SelectActiveExitPoint()
    {
        DisableExitPoints();
        ShuffleExitPoints();

        if (exitPoints.Length == 0)
            return;

        activeExitPoint = exitPoints[0].transform;
        activeExitPoint.gameObject.SetActive(true);
    }

    void ShuffleExitPoints()
    {
        for (int i = 0; i < exitPoints.Length; i++)
        {
            int randomIndex = Random.Range(i, exitPoints.Length);
            (exitPoints[i], exitPoints[randomIndex]) =
                (exitPoints[randomIndex], exitPoints[i]);
        }
    }

    void DisableExitPoints()
    {
        foreach (var zone in exitPoints)
            zone.SetActive(false);
    }
}
