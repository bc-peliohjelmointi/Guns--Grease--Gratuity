using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PhoneUI : MonoBehaviour
{
    [Header("Links")]
    public DeliverySystem delivery;
    public PlayerInput playerInput;

    [Header("UI Panels")]
    public GameObject phoneOverlay;
    public GameObject ordersPanel;
    public GameObject activeOrderPanel;

    [Header("Order UI Slots")]
    public GameObject[] orderSlots;
    public TMP_Text[] orderNames;
    public TMP_Text[] orderRewards;
    public TMP_Text[] orderTimes;
    public Button[] acceptButtons;

    [Header("Active Order UI")]
    public TMP_Text activeOrderName;
    public TMP_Text activeOrderTimer;
    public TMP_Text activeOrderReward;
    public Button declineButton;

    [Header("Other Buttons")]
    public Button endDayButton;

    private bool isOpen = false;
    public static bool AnyOpen { get; private set; }

    private List<Order> orders = new List<Order>();
    private Order selectedOrder;

    private readonly string[] restaurantNames = {
        "BurgerKong", "TacoBill", "KC", "RonaldBRGR", "PizzaHat", "Metroway"
    };

    void Start()
    {
        phoneOverlay.SetActive(false);

        for (int i = 0; i < acceptButtons.Length; i++)
        {
            int index = i;
            acceptButtons[i].onClick.AddListener(() => AcceptOrder(index));
        }

        declineButton.onClick.AddListener(DeclineOrder);
        endDayButton.onClick.AddListener(() => DaySystem.Instance.EndDay());

        GenerateOrders();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            TogglePhone();

        if (delivery.hasActiveOrder)
            activeOrderTimer.text = Mathf.CeilToInt(delivery.currentOrderTimeRemaining) + "s";
    }

    void TogglePhone()
    {
        isOpen = !isOpen;
        AnyOpen = isOpen;
        phoneOverlay.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (isOpen)
            playerInput.actions["Look"].Disable();
        else
            playerInput.actions["Look"].Enable();
    }

    public void GenerateOrders()
    {
        orders.Clear();
        int count = Random.Range(1, 4);

        for (int i = 0; i < count; i++)
        {
            orders.Add(new Order
            {
                name = restaurantNames[Random.Range(0, restaurantNames.Length)],
                reward = Random.Range(5, 16),
                timeLimit = Random.Range(30f, 60f),
                timeRemaining = 0
            });
        }

        UpdateOrderListUI(count);
    }

    void UpdateOrderListUI(int count)
    {
        for (int i = 0; i < orderSlots.Length; i++)
        {
            bool active = i < count;
            orderSlots[i].SetActive(active);

            if (active)
            {
                orderNames[i].text = orders[i].name;
                orderRewards[i].text = "$" + orders[i].reward;
                orderTimes[i].text = Mathf.RoundToInt(orders[i].timeLimit) + "s";
            }
        }
    }

    public void CloseActiveOrderPanel()
    {
        activeOrderPanel.SetActive(false);
        ordersPanel.SetActive(true);
        GenerateOrders();
    }

    void AcceptOrder(int index)
    {
        selectedOrder = orders[index];
        activeOrderPanel.SetActive(true);
        ordersPanel.SetActive(false);

        activeOrderName.text = selectedOrder.name;
        activeOrderReward.text = "$" + selectedOrder.reward;

        delivery.AssignOrder(selectedOrder.name, selectedOrder.reward, selectedOrder.timeLimit);
        Debug.Log($"Order accepted: {selectedOrder.name}");
    }

    void DeclineOrder()
    {
        selectedOrder = null;
        activeOrderPanel.SetActive(false);
        ordersPanel.SetActive(true);

        // 🧹 Remove leftover packages
        delivery.CancelOrder();

        GenerateOrders();
    }
}

[System.Serializable]
public class Order
{
    public string name;
    public int reward;
    public float timeLimit;
    public float timeRemaining;
}
