using UnityEngine;

/// <summary>
/// Handles player-selected compass navigation targets (Home, Shop, etc.)
/// DeliverySystem can override this when needed.
/// </summary>
public class CompassNavigation : MonoBehaviour
{
    public enum NavMode
    {
        None,
        Home,
        Shop,
        Delivery   // overridden externally
    }

    [Header("References")]
    public DeliverySystem deliverySystem;

    [Header("Current Navigation")]
    public NavMode currentMode = NavMode.None;

    private Transform cachedTarget;

    void Update()
    {
        // Delivery ALWAYS overrides manual navigation
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
                cachedTarget = FindTargetWithTag("Home");
                break;

            case NavMode.Shop:
                cachedTarget = FindTargetWithTag("Shop");
                break;

            case NavMode.Delivery:
                cachedTarget = deliverySystem?.GetDeliveryCompassTarget();
                break;

            default:
                cachedTarget = null;
                break;
        }
    }

    Transform FindTargetWithTag(string tag)
    {
        GameObject obj = GameObject.FindGameObjectWithTag(tag);
        return obj != null ? obj.transform : null;
    }

    /// <summary>
    /// Used by DeliverySystem to get the final compass target
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