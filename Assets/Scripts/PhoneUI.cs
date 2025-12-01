using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

    [Header("Order Panel Extra UI")]
    public TMP_Text ordersLeftText;
    public TMP_Text currentRankText;

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
        SwitchPanel(homePanel);
    }

    // --------------------------
    // APPS
    // --------------------------

    void OpenDeliveryApp()
    {
        SwitchPanel(deliveryPanel);
        UpdateExtraOrderUI();

        if (delivery.hasActiveOrder)
        {
            // Show active order UI inside delivery panel
            activeOrderPanel.SetActive(true);

            // Populate info
            activeOrderName.text = delivery.currentOrderName;
            activeOrderReward.text = "$" + delivery.currentOrderReward;
            activeOrderTimer.text = Mathf.CeilToInt(delivery.currentOrderTimeRemaining) + "s";
        }
        else
        {
            // No active order → show order list
            activeOrderPanel.SetActive(false);
            GenerateOrders();
        }
    }


    void OpenStatsApp()
    {
        SwitchPanel(statsPanel);

        statsMoneyText.text = "$" + PlayerStats.Instance.money.ToString("0.00");
        statsReputationText.text = PlayerStats.Instance.reputation.ToString("0.0");
        statsOrdersCompletedText.text = PlayerStats.Instance.deliveriesCompleted.ToString();
    }

    void OpenMapApp()
    {
        SwitchPanel(mapPanel);
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

    void UpdateExtraOrderUI()
    {
        // Orders Left
        ordersLeftText.text = PlayerStats.Instance.ordersLeft.ToString();

        // Rank = rounded reputation (floor)
        int rank = Mathf.FloorToInt(PlayerStats.Instance.reputation);
        currentRankText.text = rank.ToString();
    }


    void AcceptOrder(int index)
    {
        selectedOrder = orders[index];
        deliveryPanel.SetActive(true);
        activeOrderPanel.SetActive(true);

        activeOrderName.text = selectedOrder.name;
        activeOrderReward.text = "$" + selectedOrder.reward;

        delivery.AssignOrder(selectedOrder.name, selectedOrder.reward, selectedOrder.timeLimit);
    }

    void DeclineOrder()
    {
        selectedOrder = null;
        activeOrderPanel.SetActive(false);
        deliveryPanel.SetActive(true);

        PlayerStats.Instance.ordersLeft--;

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

    //----------------
    //  Animations
    //----------------

    IEnumerator PlayPopTransition(GameObject panel)
    {
        panel.SetActive(true);

        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float duration = 0.15f;
        float time = 0f;

        Vector3 startScale = new Vector3(0.9f, 0.9f, 1f);
        Vector3 endScale = Vector3.one;

        rt.localScale = startScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            t = 1f - Mathf.Pow(1f - t, 3f);

            rt.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        rt.localScale = endScale;
    }

    void SwitchPanel(GameObject target)
    {
        homePanel.SetActive(false);
        deliveryPanel.SetActive(false);
        activeOrderPanel.SetActive(false);
        statsPanel.SetActive(false);
        mapPanel.SetActive(false);

        StartCoroutine(PlayPopTransition(target));
    }
}




[System.Serializable]
public class Order
{
    public string name;
    public int reward;
    public float timeLimit;
}
