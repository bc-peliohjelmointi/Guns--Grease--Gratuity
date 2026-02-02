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
    public TurretActivator turretActivator;
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
        // Find all delivery zones in the scene and hide them
        deliveryZones = GameObject.FindGameObjectsWithTag("DeliveryZone");
        foreach (var zone in deliveryZones) zone.SetActive(false);

        // Find all exit points and hide them
        exitPoints = GameObject.FindGameObjectsWithTag("ExitPoint");
        DisableExitPoints();

        // Randomize delivery zone order
        ShuffleDeliveryZones();
        UpdateUI();
    }

    void Update()
    {
        // Update what player should navigate to
        UpdateTargets();

        // Update compass arrow direction
        UpdateCompass();

        // Update status text
        UpdateStatus();

        // Update package health bar
        UpdateHPSlider();

        // Update delivery timer
        UpdateTimer();

        // Check if player can enter delivery building
        if (hasPackage && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame && !isTeleporting)
            {
                StartCoroutine(TeleportToEntry());
            }
        }

        // Check if package is destroyed
        if (hasPackage && currentDeliveryHP <= 0)
        {
            FailDelivery("Package destroyed!");
        }
    }

    // Start a new delivery order
    public void AssignOrder(string name, int reward, float timeLimit)
    {
        pendingDelivery = false;
        playerAtExitPoint = false;

        if (startDeliverySFX) audioSource.PlayOneShot(startDeliverySFX);

        hasActiveOrder = true;
        hasPackage = false;
        currentOrderName = name;
        currentOrderReward = reward;
        currentOrderTime = timeLimit;
        currentOrderTimeRemaining = timeLimit;

        // Spawn the package in the world
        itemSpawner?.SpawnItem();

        // Choose random delivery zone and activate it
        activeDeliveryZone = deliveryZones[Random.Range(0, deliveryZones.Length)];
        foreach (var zone in deliveryZones)
            zone.SetActive(zone == activeDeliveryZone);

        activeExitPoint = exitPoints.Length > 0 ? exitPoints[0].transform : null;
        UpdateUI();
    }

    // Countdown delivery timer
    void UpdateTimer()
    {
        if (!hasActiveOrder) return;

        currentOrderTimeRemaining -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(currentOrderTimeRemaining) + "";

        // Fail delivery if time runs out
        if (currentOrderTimeRemaining <= 0)
        {
            FailDelivery("Time ran out!");
        }
    }

    // Update package health slider UI
    void UpdateHPSlider()
    {
        if (hpSlider == null) return;

        // Only show HP bar when carrying package
        hpSlider.gameObject.SetActive(hasPackage);
        if (hasPackage)
        {
            hpSlider.maxValue = maxDeliveryHP;
            hpSlider.value = currentDeliveryHP;
        }
    }

    // Determine what the player should be navigating to
    void UpdateTargets()
    {
        // No order active - point to apartment door
        if (!hasActiveOrder)
        {
            currentTarget = GameObject.FindGameObjectWithTag("ApartmentDoor");
            return;
        }

        // Order active but no package - point to package
        if (!hasPackage)
        {
            currentTarget = GameObject.FindGameObjectsWithTag("Package").FirstOrDefault();
            return;
        }

        // Has package - point to delivery zone
        currentTarget = activeDeliveryZone;
    }

    // Get the transform the compass should point to
    public Transform GetCompassTarget()
    {
        if (!hasActiveOrder) return GameObject.FindGameObjectWithTag("ApartmentDoor")?.transform;

        if (!hasPackage)
            return GameObject.FindGameObjectsWithTag("Package").FirstOrDefault()?.transform;

        // Inside building - point to exit
        if (teleportManager.isInStairwell)
            return activeExitPoint;

        // Outside - point to delivery zone
        return activeDeliveryZone?.transform;
    }

    // Update compass arrow to point at target
    void UpdateCompass()
    {
        Transform target = GetCompassTarget();
        if (compassArrow == null || target == null) return;

        // Calculate direction to target
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f; // Keep arrow flat

        // Calculate angle and rotate arrow
        float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
        compassArrow.localEulerAngles = new Vector3(0, 0, -angle);
    }

    // Update status text based on current state
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

        // At delivery exit point - show deliver prompt
        if (pendingDelivery && playerAtExitPoint && teleportManager.isInStairwell)
        {
            statusText.text = "<color=yellow>[E] Deliver!</color>";
            return;
        }

        // Near delivery zone entrance - show enter prompt
        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        statusText.text = distance <= deliveryRange
            ? "<color=yellow>[E] Enter</color>"
            : "Deliver package!";
    }

    // Damage the package (called by enemies, hazards, etc.)
    public void TakeDamage(float dmg)
    {
        if (!hasPackage) return;
        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP - dmg, 0, maxDeliveryHP);
    }

    // Complete the delivery successfully
    public void DeliverPackage()
    {
        hasPackage = false;
        hasActiveOrder = false;

        if (deliveryCompleteSFX) audioSource.PlayOneShot(deliveryCompleteSFX);

        statusText.text = $"<color=green>Delivery Completed! +{currentOrderReward}</color>";

        // Update player stats
        PlayerStats.Instance.OnDeliveryCompleted(currentOrderReward, currentDeliveryHP);
        PlayerStats.Instance.ordersLeft--;

        // Clean up
        DisableAllDeliveryZones();
        phoneUI?.CloseActiveOrderPanel();
        UpdateUI();
    }

    // Fail the current delivery
    void FailDelivery(string reason)
    {
        hasPackage = false;
        hasActiveOrder = false;

        statusText.text = $"<color=red>Delivery failed! {reason}</color>";

        // Update player stats
        PlayerStats.Instance.OnDeliveryFailed();
        PlayerStats.Instance.ordersLeft--;

        // Teleport out if inside building
        if (teleportManager.isInStairwell == true)
        {
            StartCoroutine(TeleportOutOfStairwell());
        }
    }

    // Player cancels the current order
    public void CancelOrder()
    {
        hasActiveOrder = false;
        hasPackage = false;

        // Update player stats
        PlayerStats.Instance.OnOrderDeclined();
        PlayerStats.Instance.ordersLeft--;

        // Teleport out if inside building
        if (teleportManager.isInStairwell == true)
        {
            StartCoroutine(TeleportOutOfStairwell());
            return;
        }
    }

    // Clean up after canceling order
    private void CleanupAfterCancel()
    {
        DisableAllDeliveryZones();
        ClearAllPackages();
        DisableCompass();

        statusText.text = "No active order!";
        UpdateUI();
    }

    // Remove all package objects from scene
    public void ClearAllPackages()
    {
        foreach (var pkg in GameObject.FindGameObjectsWithTag("Package"))
            Destroy(pkg);
    }

    // Reset compass arrow
    public void DisableCompass()
    {
        currentTarget = null;
        if (compassArrow != null)
            compassArrow.localEulerAngles = Vector3.zero;
    }

    // Detect when player picks up package
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

    // Update timer UI
    void UpdateUI()
    {
        timerText.text = hasActiveOrder
            ? Mathf.CeilToInt(currentOrderTimeRemaining).ToString()
            : "";
    }

    // Randomize delivery zone array order
    void ShuffleDeliveryZones()
    {
        for (int i = 0; i < deliveryZones.Length; i++)
        {
            int randomIndex = Random.Range(i, deliveryZones.Length);
            (deliveryZones[i], deliveryZones[randomIndex]) =
                (deliveryZones[randomIndex], deliveryZones[i]);
        }
    }

    // Deactivate all delivery zones
    void DisableAllDeliveryZones()
    {
        foreach (var zone in deliveryZones)
            zone.SetActive(false);

        activeDeliveryZone = null;
        activeExitPoint = null;
    }

    // Fade screen to black
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

    // Fade screen from black
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

    // Teleport player into delivery building
    private IEnumerator TeleportToEntry()
    {
        isTeleporting = true;

        yield return StartCoroutine(FadeOut());

        turretActivator.SpawnTurrets();

        teleportManager.TeleportToStairwell(transform);

        // Set up exit point for delivery
        ShuffleExitPoints();
        SelectActiveExitPoint();
        pendingDelivery = true;

        yield return StartCoroutine(FadeIn());
        isTeleporting = false;
    }

    // Teleport player out of delivery building
    private IEnumerator TeleportOutOfStairwell()
    {
        if (teleportManager.isInStairwell)
        {
            yield return StartCoroutine(FadeOut());

            turretActivator.DestroyTurrets();

            teleportManager.TeleportToDeliveryZone(transform);
            teleportManager.isInStairwell = false;

            pendingDelivery = false;
            playerAtExitPoint = false;

            CleanupAfterCancel();

            yield return StartCoroutine(FadeIn());
        }
    }

    // Find the exit point closest to active delivery zone
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

    // Get current delivery zone transform
    public Transform GetActiveDeliveryZoneTransform()
    {
        return activeDeliveryZone != null ? activeDeliveryZone.transform : null;
    }

    // Get current exit point transform
    public Transform GetActiveExitPoint()
    {
        return activeExitPoint;
    }

    // Choose and activate a random exit point
    void SelectActiveExitPoint()
    {
        DisableExitPoints();
        ShuffleExitPoints();

        if (exitPoints.Length == 0)
            return;

        activeExitPoint = exitPoints[0].transform;
        activeExitPoint.gameObject.SetActive(true);
    }

    // Randomize exit point array order
    void ShuffleExitPoints()
    {
        for (int i = 0; i < exitPoints.Length; i++)
        {
            int randomIndex = Random.Range(i, exitPoints.Length);
            (exitPoints[i], exitPoints[randomIndex]) =
                (exitPoints[randomIndex], exitPoints[i]);
        }
    }

    // Deactivate all exit points
    void DisableExitPoints()
    {
        foreach (var zone in exitPoints)
            zone.SetActive(false);
    }
}