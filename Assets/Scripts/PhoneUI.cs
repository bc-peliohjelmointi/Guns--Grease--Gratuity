using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public TMP_Text[] orderRanks;

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

    [Header("Instructions App")]
    public Button instructionsAppButton;
    public GameObject instructionsPanel;

    [Header("End Day Panel")]
    public Button endDayButton;
    public GameObject endDayPanel;
    public TMP_Text summaryMoneyText;
    public TMP_Text summaryReputationText;
    public Button closeEndDayPanelButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip phoneOpenSFX;
    public AudioClip phoneCloseSFX;
    public AudioClip appOpenSFX;
    public AudioClip appCloseSFX;
    public AudioClip buttonHoverSFX;
    public AudioClip buttonClickSFX;

    [Header("Phone Animation")]
    public RectTransform phoneTransform;
    public float phoneAnimDuration = 0.2f;
    public Vector2 phoneHiddenPos = new Vector2(0, -800);
    public Vector2 phoneShownPos = Vector2.zero;



    private bool isOpen = false;
    private bool ordersGeneratedToday = false;

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
        instructionsAppButton.onClick.AddListener(OpenInstructionsApp);

        homeButton.onClick.AddListener(ShowHomeScreen);

        // End day panel
        endDayPanel.SetActive(false);
        endDayButton.onClick.AddListener(ShowEndDayPanel);
        closeEndDayPanelButton.onClick.AddListener(CloseEndDayPanel);

        AddHoverSoundsToButtons();
        AddClickSoundsToButtons();

        ShowHomeScreen();
        GenerateOrdersOnce();
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

        StopAllCoroutines();
        StartCoroutine(AnimatePhone(isOpen));

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (isOpen)
        {
            playerInput.actions["Look"].Disable();
            PlaySound(phoneOpenSFX);
        }
        else
        {
            playerInput.actions["Look"].Enable();
            PlaySound(phoneCloseSFX);
        }
    }



    // --------------------------
    // HOME SCREEN
    // --------------------------

    void ShowHomeScreen()
    {
        PlaySound(appCloseSFX);
        SwitchPanel(homePanel);
    }


    // --------------------------
    // APPS
    // --------------------------

    void OpenDeliveryApp()
    {
        PlaySound(appOpenSFX);
        SwitchPanel(deliveryPanel);
        UpdateExtraOrderUI();

        if (delivery.hasActiveOrder)
        {
            activeOrderPanel.SetActive(true);
            activeOrderName.text = delivery.currentOrderName;
            activeOrderReward.text = "$" + delivery.currentOrderReward;
            activeOrderTimer.text = Mathf.CeilToInt(delivery.currentOrderTimeRemaining) + "s";
        }
        else
        {
            activeOrderPanel.SetActive(false);
            UpdateOrderListUI(orders.Count);
        }
    }

    void OpenStatsApp()
    {
        PlaySound(appOpenSFX);
        SwitchPanel(statsPanel);

        statsMoneyText.text = "$" + PlayerStats.Instance.money.ToString("0.00");
        statsReputationText.text = PlayerStats.Instance.reputation.ToString("0.0");
        statsOrdersCompletedText.text = PlayerStats.Instance.deliveriesCompleted.ToString();
    }

    void OpenMapApp()
    {
        PlaySound(appOpenSFX);
        SwitchPanel(mapPanel);
    }

    void OpenInstructionsApp()
    {
        PlaySound(appOpenSFX);
        SwitchPanel(instructionsPanel);
    }

    // --------------------------
    // ORDERS
    // --------------------------

    public void GenerateOrders()
    {
        orders.Clear();

        int baseMin = 1;
        int baseMax = 3;

        int playerRank = Mathf.FloorToInt(PlayerStats.Instance.reputation);

        // Rank affects order count chance (+10% per rank)
        float bonusChance = playerRank * 0.10f;
        int extraOrder = Random.value < bonusChance ? 1 : 0;

        int count = Random.Range(baseMin, baseMax + 1) + extraOrder;

        for (int i = 0; i < count; i++)
        {
            int orderRank;

            // 66% chance = current rank
            // 33% chance = previous rank (minimum 1)
            if (Random.value <= 0.66f)
            {
                orderRank = Mathf.Max(1, playerRank);
            }
            else
            {
                orderRank = Mathf.Max(1, playerRank - 1);
            }

            
            float rankMultiplier = 1f + (orderRank * 0.10f);
            int baseReward = Random.Range(8, 20);
            int finalReward = Mathf.RoundToInt(baseReward * rankMultiplier);

            orders.Add(new Order
            {
                name = restaurantNames[Random.Range(0, restaurantNames.Length)],
                reward = finalReward,
                timeLimit = Random.Range(70f, 160f),
                rank = orderRank
            });
        }

        UpdateOrderListUI(count);
    }



    void GenerateOrdersOnce()
    {
        if (ordersGeneratedToday) return;

        ordersGeneratedToday = true;
        GenerateOrders();
    }

    public void ResetOrdersForNewCycle()
    {
        ordersGeneratedToday = false;
        GenerateOrdersOnce();
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

                orderRanks[i].text = "Rank " + orders[i].rank;
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

        PlayerStats.Instance.ordersLeft--;          // FIX
        UpdateExtraOrderUI();                       // FIX

        delivery.AssignOrder(
            selectedOrder.name,
            selectedOrder.reward,
            selectedOrder.timeLimit
        );
    }


    void DeclineOrder()
    {
        selectedOrder = null;
        activeOrderPanel.SetActive(false);
        deliveryPanel.SetActive(true);


        delivery.CancelOrder();

        ordersGeneratedToday = false;
        GenerateOrdersOnce();
    }


    // --------------------------
    // END DAY
    // --------------------------

    void ShowEndDayPanel()
    {
        endDayPanel.SetActive(true);

        summaryMoneyText.text = "$" + PlayerStats.Instance.moneyToday.ToString("0.00");
        summaryReputationText.text = PlayerStats.Instance.reputation.ToString("0.0");
    }


    public void CloseEndDayPanel()
    {
        endDayPanel.SetActive(false);

        PlayerStats.Instance.currentDay++;
        PlayerStats.Instance.ResetDayStats();

        ResetOrdersForNewCycle(); // already in your script

        ShowHomeScreen();
    }

    public void CloseActiveOrderPanel()
    {
        activeOrderPanel.SetActive(false);
        deliveryPanel.SetActive(true);
        GenerateOrders();
    }

    //----------------
    //  Animations & Sound
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
        instructionsPanel.SetActive(false);

        StartCoroutine(PlayPopTransition(target));
    }

    IEnumerator AnimatePhone(bool opening)
    {
        phoneOverlay.SetActive(true);

        Vector2 start = opening ? phoneHiddenPos : phoneShownPos;
        Vector2 end = opening ? phoneShownPos : phoneHiddenPos;

        float time = 0f;

        while (time < phoneAnimDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / phoneAnimDuration;

            // Smooth ease-out
            t = 1f - Mathf.Pow(1f - t, 3f);

            phoneTransform.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        phoneTransform.anchoredPosition = end;

        if (!opening)
            phoneOverlay.SetActive(false);
    }



    //--------------
    // ---SOUNDS----
    //--------------
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    void AddHoverSoundsToButtons()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            UIButtonHoverSound hover = btn.gameObject.GetComponent<UIButtonHoverSound>();

            if (hover == null)
                hover = btn.gameObject.AddComponent<UIButtonHoverSound>();

            hover.audioSource = audioSource;
            hover.hoverSound = buttonHoverSFX;
        }
    }

    void AddClickSoundsToButtons()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            UIButtonClickSound click = btn.gameObject.GetComponent<UIButtonClickSound>();

            if (click == null)
                click = btn.gameObject.AddComponent<UIButtonClickSound>();

            click.audioSource = audioSource;
            click.clickSound = buttonClickSFX;
        }
    }



}




[System.Serializable]
public class Order
{
    public string name;
    public int reward;
    public float timeLimit;
    public int rank;
}
