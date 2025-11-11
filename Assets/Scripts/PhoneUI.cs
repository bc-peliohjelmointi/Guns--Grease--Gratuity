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

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    public static bool AnyOpen { get; private set; }

    private List<Order> orders = new List<Order>();
    private Order selectedOrder;

    // ✅ RESTAURANT LIST
    private string[] restaurantNames = new string[]
    {
        "BurgerKong",
        "TacoBill",
        "KC",
        "RonaldBRGR",
        "PizzaHat",
        "Metroway"
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

        GenerateOrders();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            TogglePhone();

        if (delivery.hasActiveOrder)
        {
            activeOrderTimer.text = Mathf.CeilToInt(delivery.currentOrderTimeRemaining) + "s";
        }
    }

    void TogglePhone()
    {
        isOpen = !isOpen;
        AnyOpen = isOpen;
        phoneOverlay.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        // ✅ Disable camera rotation
        if (isOpen)
        {
            playerInput.actions["Look"].Disable();
        }
        else
        {
            playerInput.actions["Look"].Enable();
        }
    }


    // ✅ Generate 1–3 random orders
    void GenerateOrders()
    {
        orders.Clear();

        int count = Random.Range(1, 4);

        for (int i = 0; i < count; i++)
        {
            Order o = new Order();
            o.name = restaurantNames[Random.Range(0, restaurantNames.Length)];
            o.reward = Random.Range(5, 16); // 5–15
            o.timeLimit = Random.Range(30f, 60f); // 30–60 sec
            o.timeRemaining = o.timeLimit;

            orders.Add(o);
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
        ordersPanel.SetActive(true);       // show the list again
        GenerateOrders();                  // create new random orders
    }

    void AcceptOrder(int index)
    {
        selectedOrder = orders[index];

        activeOrderPanel.SetActive(true);
        ordersPanel.SetActive(false);

        activeOrderName.text = selectedOrder.name;
        activeOrderReward.text = "$" + selectedOrder.reward;

        delivery.AssignOrder(selectedOrder.name, selectedOrder.reward, selectedOrder.timeLimit);

        Debug.Log("Order accepted: " + selectedOrder.name);
    }

    void DeclineOrder()
    {
        selectedOrder = null;

        activeOrderPanel.SetActive(false);
        ordersPanel.SetActive(true);

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
