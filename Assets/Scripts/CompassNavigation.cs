using UnityEngine;

/// <summary>
/// Handles player-selected compass navigation targets
/// </summary>
public class CompassNavigation : MonoBehaviour
{
    public enum NavMode
    {
        None,
        Home,
        Shop,
        Delivery
    }

    [Header("References")]
    public DeliverySystem deliverySystem;
    public Transform player;

    [Header("Current Navigation")]
    public NavMode currentMode = NavMode.None;

    private Transform cachedTarget;

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        // Delivery overrides everything
        if (deliverySystem != null && deliverySystem.hasActiveOrder)
        {
            currentMode = NavMode.Delivery;
        }

        UpdateTarget();
    }

    void UpdateTarget()
    {
        switch (currentMode)
        {
            case NavMode.Home:
                cachedTarget = FindClosestTargetWithTag("Home");
                break;

            case NavMode.Shop:
                cachedTarget = FindClosestTargetWithTag("Shop");
                break;

            case NavMode.Delivery:
                cachedTarget = deliverySystem?.GetDeliveryCompassTarget();
                break;

            default:
                cachedTarget = null;
                break;
        }
    }

    Transform FindClosestTargetWithTag(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

        if (objs.Length == 0 || player == null)
            return null;

        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in objs)
        {
            float dist = Vector3.Distance(
                player.position,
                obj.transform.position
            );

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = obj.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// Used by DeliverySystem
    /// </summary>
    public Transform GetCurrentTarget()
    {
        return cachedTarget;
    }

    // ------------------
    // Called by Map App
    // ------------------

    public void NavigateHome()
    {
        currentMode = NavMode.Home;
    }

    public void NavigateShop()
    {
        currentMode = NavMode.Shop;
    }

    public void ClearNavigation()
    {
        currentMode = NavMode.None;
    }
}