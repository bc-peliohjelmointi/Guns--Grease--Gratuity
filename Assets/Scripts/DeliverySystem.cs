using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class DeliverySystem : MonoBehaviour
{
    [Header("Delivery Settings")]
    public int deliveryPoints = 0;
    public int reputation = 100; // maine
    public bool hasPackage = false;
    public float deliveryBonus = 100f;
    public float deliveryRange = 3f;

    [Header("Delivery HP")]
    public float maxDeliveryHP = 100f;
    public float currentDeliveryHP;

    [Header("UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI hpText;
    public RectTransform compassArrow;

    private GameObject[] deliveryZones;
    private GameObject[] packages;
    private GameObject currentTarget;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        deliveryZones = GameObject.FindGameObjectsWithTag("DeliveryZone");
        packages = GameObject.FindGameObjectsWithTag("Package");
        UpdateUI();
    }

    void Update()
    {
        UpdateTargets();
        UpdateCompass();
        UpdateDistanceAndStatus();

        if (hasPackage && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= deliveryRange && Keyboard.current.eKey.wasPressedThisFrame)
            {
                DeliverPackage();
            }
        }

        // Päivitä HP UI
        if (hpText != null)
        {
            if (hasPackage)
                hpText.text = "Toimitus HP: " + currentDeliveryHP.ToString("F0") + " / " + maxDeliveryHP.ToString("F0");
            else
                hpText.text = "";
        }

        // Tarkista, jos paketti tuhoutuu
        if (hasPackage && currentDeliveryHP <= 0)
        {
            FailDelivery();
        }
    }

    void UpdateTargets()
    {
        if (!hasPackage)
        {
            packages = GameObject.FindGameObjectsWithTag("Package");
            if (packages.Length > 0)
                currentTarget = packages.OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).FirstOrDefault();
            else
                currentTarget = null;
        }
        else
        {
            if (deliveryZones.Length > 0)
                currentTarget = deliveryZones.OrderBy(z => Vector3.Distance(transform.position, z.transform.position)).FirstOrDefault();
        }
    }

    void UpdateCompass()
    {
        if (compassArrow == null || currentTarget == null) return;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        direction.y = 0f;

        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        compassArrow.localEulerAngles = new Vector3(0, 0, -angle);
    }

    void UpdateDistanceAndStatus()
    {
        if (currentTarget == null)
        {
            if (statusText != null) statusText.text = "Ei aktiivista tilausta!";
            if (distanceText != null) distanceText.text = "";
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceText != null)
            distanceText.text = "Etäisyys kohteeseen: " + distance.ToString("F1") + " m";

        if (statusText != null)
        {
            if (!hasPackage)
                statusText.text = "Hae tilaus! Suuntaa kompassin mukaan.";
            else if (distance <= deliveryRange)
                statusText.text = "<color=yellow>Paina [E] toimittaaksesi tilauksen!</color>";
            else
                statusText.text = "Toimita paketti alueelle!";
        }
    }

    public void TakeDamage(float damage)
    {
        if (!hasPackage) return;

        currentDeliveryHP -= damage;
        currentDeliveryHP = Mathf.Clamp(currentDeliveryHP, 0, maxDeliveryHP);

        if (hpText != null)
            hpText.text = "Toimitus HP: " + currentDeliveryHP.ToString("F0") + " / " + maxDeliveryHP.ToString("F0");

        if (currentDeliveryHP <= 0)
        {
            FailDelivery();
        }
    }

    void DeliverPackage()
    {
        hasPackage = false;
        deliveryPoints += Mathf.RoundToInt(deliveryBonus);
        Debug.Log("Toimitus suoritettu! Pisteet: " + deliveryPoints);

        if (statusText != null)
            statusText.text = "<color=green>Toimitus suoritettu!</color>";

        UpdateUI();
    }

    void FailDelivery()
    {
        hasPackage = false;
        Debug.Log("Toimitus epäonnistui! HP loppui.");
        deliveryPoints -= 50;
        reputation -= 10;
        if (statusText != null)
            statusText.text = "<color=red>Toimitus epäonnistui! Menetit rahaa ja mainetta.</color>";

        UpdateUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package") && !hasPackage)
        {
            hasPackage = true;
            Destroy(other.gameObject);
            currentDeliveryHP = maxDeliveryHP; // resetoi HP
            Debug.Log("Toimitus kerätty!");
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (statusText != null)
        {
            if (hasPackage)
                statusText.text = "Tilaus kerätty – Toimita se!";
            else
                statusText.text = "Nouda tilaus!";
        }

        if (hpText != null)
        {
            if (hasPackage)
                hpText.text = "Toimitus HP: " + currentDeliveryHP.ToString("F0") + " / " + maxDeliveryHP.ToString("F0");
            else
                hpText.text = "";
        }
    }
}
