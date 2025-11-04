using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class DeliverySystem : MonoBehaviour
{
    [Header("Settings")]
    public int deliveryPoints = 0;
    public bool hasPackage = false;
    public float deliveryBonus = 100f;
    public float deliveryRange = 3f;

    [Header("UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI distanceText;
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
    }

    void UpdateTargets()
    {
        if (!hasPackage)
        {
            // Etsi lähin paketti
            packages = GameObject.FindGameObjectsWithTag("Package");
            if (packages.Length > 0)
                currentTarget = packages.OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).FirstOrDefault();
            else
                currentTarget = null;
        }
        else
        {
            // Etsi lähin toimitusalue
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
            if (statusText != null) statusText.text = "Ei aktiivista kohdetta!";
            if (distanceText != null) distanceText.text = "";
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceText != null)
            distanceText.text = "Etäisyys kohteeseen: " + distance.ToString("F1") + " m";

        if (statusText != null)
        {
            if (!hasPackage)
            {
                statusText.text = "Etsi paketti! Suuntaa kompassin mukaan.";
            }
            else if (distance <= deliveryRange)
            {
                statusText.text = "<color=yellow>Paina [E] toimittaaksesi tilauksen!</color>";
            }
            else
            {
                statusText.text = "Toimita paketti alueelle!";
            }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Package") && !hasPackage)
        {
            hasPackage = true;
            Destroy(other.gameObject);
            Debug.Log("Paketti kerätty!");
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (statusText != null)
        {
            if (hasPackage)
                statusText.text = "Paketti kerätty – vie se toimitusalueelle!";
            else
                statusText.text = "Etsi paketti toimitettavaksi!";
        }
    }
}
