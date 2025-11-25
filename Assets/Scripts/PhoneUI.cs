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

    [Header("Navigation Buttons")]
    public Button homeButton;

    [Header("UI Panels")]
    public GameObject phoneOverlay;

    public GameObject homePanel;          // NEW home screen  
    public GameObject deliveryPanel;      // was ordersPanel  
    public GameObject activeOrderPanel;
    public GameObject statsPanel;         // NEW stats app  
    public GameObject mapPanel;           // NEW map app

    [Header("Home Screen App Buttons")]
    public Button deliveryAppButton;
    public Button statsAppButton;
    public Button mapAppButton;

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

    [Header("Stats App UI")]
    public TMP_Text statsMoneyText;
    public TMP_Text statsReputationText;
    public TMP_Text statsOrdersCompletedText;

    [Header("Map App UI")]
    public Image mapImage;   // You assign a sprite in inspector

    [Header("End Day Panel")]
    public Button endDayButton;
    public GameObject endDayPanel;
    public TMP_Text summaryMoneyText;
    public TMP_Text summaryReputationText;
    public Button closeEndDayPanelButton;

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

        // Accept order buttons
        for (int i = 0; i < acceptButtons.Length; i++)
        {
            int index = i;
            acceptButtons[i].onClick.AddListener(() => AcceptOrder(index));
        }

        declineButton.onClick.AddListener(DeclineOrder);

        // App buttons
        deliveryAppButton.onClick.AddListener(OpenDeliveryApp);
        statsAppButton.onClick.AddListener(OpenStatsApp);
        mapAppButton.onClick.AddListener(OpenMapApp);
        homeButton.onClick.AddListener(ShowHomeScreen);

        // End day panel
        endDayPanel.SetActive(false);
        endDayButton.onClick.AddListener(ShowEndDayPanel);
        closeEndDayPanelButton.onClick.AddListener(CloseEndDayPanel);

        ShowHomeScreen();
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

    // --------------------------
    // HOME SCREEN
    // --------------------------

    void ShowHomeScreen()
    {
        homePanel.SetActive(true);
        deliveryPanel.SetActive(false);
        activeOrderPanel.SetActive(false);
        statsPanel.SetActive(false);
        mapPanel.SetActive(false);
    }

    // --------------------------
    // APPS
    // --------------------------

    void OpenDeliveryApp()
    {
        homePanel.SetActive(false);
        statsPanel.SetActive(false);
        mapPanel.SetActive(false);

        deliveryPanel.SetActive(true);
        activeOrderPanel.SetActive(false);

        GenerateOrders();
    }

    void OpenStatsApp()
    {
        homePanel.SetActive(false);
        deliveryPanel.SetActive(false);
        activeOrderPanel.SetActive(false);
        mapPanel.SetActive(false);

        statsPanel.SetActive(true);

        statsMoneyText.text = "$" + PlayerStats.Instance.money.ToString("0.00");
        statsReputationText.text = PlayerStats.Instance.reputation.ToString("0.0");
        statsOrdersCompletedText.text = PlayerStats.Instance.deliveriesCompleted.ToString();
    }

    void OpenMapApp()
    {
        homePanel.SetActive(false);
        deliveryPanel.SetActive(false);
        activeOrderPanel.SetActive(false);
        statsPanel.SetActive(false);

        mapPanel.SetActive(true);
    }

    // --------------------------
    // ORDERS
    // --------------------------

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
                timeLimit = Random.Range(30f, 60f)
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

    void AcceptOrder(int index)
    {
        selectedOrder = orders[index];
        activeOrderPanel.SetActive(true);
        deliveryPanel.SetActive(false);

        activeOrderName.text = selectedOrder.name;
        activeOrderReward.text = "$" + selectedOrder.reward;

        delivery.AssignOrder(selectedOrder.name, selectedOrder.reward, selectedOrder.timeLimit);
    }

    void DeclineOrder()
    {
        selectedOrder = null;
        activeOrderPanel.SetActive(false);
        deliveryPanel.SetActive(true);

        delivery.CancelOrder();
        GenerateOrders();
    }

    // --------------------------
    // END DAY
    // --------------------------

    void ShowEndDayPanel()
    {
        endDayPanel.SetActive(true);

        summaryMoneyText.text = "$" + PlayerStats.Instance.moneyToday.ToString("0.00");
        summaryReputationText.text = PlayerStats.Instance.reputation.ToString("0.0");

        deliveryPanel.SetActive(false);
        activeOrderPanel.SetActive(false);
        statsPanel.SetActive(false);
        mapPanel.SetActive(false);
        homePanel.SetActive(false);
    }

    public void CloseEndDayPanel()
    {
        endDayPanel.SetActive(false);
        ShowHomeScreen();
    }

    public void CloseActiveOrderPanel()
    {
        activeOrderPanel.SetActive(false);
        deliveryPanel.SetActive(true);
        GenerateOrders();
    }
}




[System.Serializable]
public class Order
{
    public string name;
    public int reward;
    public float timeLimit;
}
