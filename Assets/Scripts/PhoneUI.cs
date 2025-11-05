using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PhoneUI : MonoBehaviour
{
    [Header("References")]
    public DeliverySystem deliverySystem;

    [Header("UI Panels")]
    public GameObject phoneOverlay;
    public GameObject ordersPanel;
    public GameObject activeOrderPanel;

    [Header("Order Slots (Full Panels)")]
    public GameObject[] orderSlots; // each slot = full panel
    public TMP_Text[] orderNames;
    public TMP_Text[] orderRewards;
    public TMP_Text[] orderTimes;
    public Button[] acceptButtons;

    [Header("Active Order Elements")]
    public TMP_Text activeOrderName;
    public TMP_Text activeOrderTimer;
    public TMP_Text activeOrderReward;
    public Button declineButton;

    private bool isOpen = false;
    private bool hasActiveOrder = false;

    private List<Order> currentOrders = new List<Order>();
    private Order selectedOrder;

    void Start()
    {
        phoneOverlay.SetActive(false);

        for (int i = 0; i < acceptButtons.Length; i++)
        {
            int index = i;
            acceptButtons[i].onClick.AddListener(() => AcceptOrder(index));
        }

        declineButton.onClick.AddListener(DeclineOrder);

        GenerateOrders(); // start with random 1–3
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            TogglePhone();

        if (hasActiveOrder && selectedOrder != null)
        {
            selectedOrder.timeRemaining -= Time.deltaTime;
            UpdateActiveOrderUI();
        }
    }

    void TogglePhone()
    {
        isOpen = !isOpen;
        phoneOverlay.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    // 🔹 Generates 1–3 random orders
    void GenerateOrders()
    {
        currentOrders.Clear();

        int numberOfOrders = Random.Range(1, 4); // 1–3 inclusive
        Debug.Log("Generated " + numberOfOrders + " orders.");

        for (int i = 0; i < numberOfOrders; i++)
        {
            float timeLimit = Random.Range(60, 180);
            Order newOrder = new Order
            {
                name = "Order #" + Random.Range(100, 999),
                reward = Random.Range(80, 150),
                timeLimit = timeLimit,
                timeRemaining = timeLimit
            };
            currentOrders.Add(newOrder);
        }

        UpdateOrderListUI(numberOfOrders);
    }

    void UpdateOrderListUI(int visibleCount)
    {
        StopAllCoroutines(); // stop any previous fades

        for (int i = 0; i < orderNames.Length; i++)
        {
            bool isActive = i < visibleCount;
            orderSlots[i].SetActive(isActive);

            if (isActive)
            {
                var order = currentOrders[i];
                orderNames[i].text = order.name;
                orderRewards[i].text = "$" + order.reward;
                orderTimes[i].text = "Time: " + Mathf.RoundToInt(order.timeLimit) + "s";
                acceptButtons[i].interactable = true;

                // Start fade-in animation with small delay
                StartCoroutine(FadeInSlot(orderSlots[i], 0.25f, 0.05f * i));
            }
        }
    }

    IEnumerator FadeInSlot(GameObject slot, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        CanvasGroup cg = slot.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = slot.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        cg.alpha = 1f;
    }

    void AcceptOrder(int index)
    {
        if (index < 0 || index >= currentOrders.Count)
            return;

        selectedOrder = currentOrders[index];
        hasActiveOrder = true;

        ordersPanel.SetActive(false);
        activeOrderPanel.SetActive(true);

        UpdateActiveOrderUI();

        Debug.Log("Accepted order: " + selectedOrder.name);
    }

    void UpdateActiveOrderUI()
    {
        if (selectedOrder == null) return;

        activeOrderName.text = selectedOrder.name;
        activeOrderReward.text = "$" + selectedOrder.reward;
        activeOrderTimer.text = "Time: " + Mathf.CeilToInt(selectedOrder.timeRemaining) + "s";
    }

    void DeclineOrder()
    {
        if (selectedOrder != null)
            Debug.Log("Declined order: " + selectedOrder.name);

        hasActiveOrder = false;
        selectedOrder = null;

        activeOrderPanel.SetActive(false);
        ordersPanel.SetActive(true);

        GenerateOrders(); // regenerate random orders
    }
}

[System.Serializable]
public class Order
{
    public string name;
    public float reward;
    public float timeLimit;
    public float timeRemaining;
}
