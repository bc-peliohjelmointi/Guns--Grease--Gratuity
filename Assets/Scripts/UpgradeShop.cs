using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    [Header("Upgrade Prices")]
    public int damagePrice = 100;
    public int bodyArmorPrice = 50;
    public int scooterSpeedPrice = 50;
    public int rewardMultiplierPrice = 100;

    [SerializeField] private int maxLevel = 5;

    [Header("UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI rewardText;

    [Header("Sliders")]
    public Slider damageSlider;
    public Slider timeSlider;
    public Slider speedSlider;
    public Slider rewardSlider;

    [Header("Confirmation Popup")]
    public GameObject confirmPanel;
    public Button yesButton;
    public Button noButton;
    public TMP_Text confirmText;

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip buySuccessSFX;
    public AudioClip buyFailSFX;
    public AudioClip popupOpenSFX;

    private PlayerStats stats;

    // FIXED: Missing variable
    private System.Action pendingPurchase;

    private void Start()
    {
        stats = PlayerStats.Instance;

        UpdateUI();

        // Hide popup at start
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Button listeners
        if (yesButton != null)
            yesButton.onClick.AddListener(ConfirmPurchase);

        if (noButton != null)
            noButton.onClick.AddListener(CancelPurchase);
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (stats == null)
            stats = PlayerStats.Instance;

        UpdateUI();
    }

    // -------------------------
    // BUY METHODS
    // -------------------------

    public void BuyDamage()
    {
        AskForConfirmation("Weapon Damage", () =>
        {
            if (stats.weaponDamageLevel >= maxLevel)
            {
                PlaySound(buyFailSFX);
                return;
            }

            if (stats.money >= damagePrice)
            {
                stats.money -= damagePrice;
                stats.weaponDamageLevel++;

                PlaySound(buySuccessSFX);
                UpdateUI();
            }
            else
            {
                PlaySound(buyFailSFX);
            }
        });
    }

    public void BuyBodyArmor()
    {
        AskForConfirmation("Body Armor", () =>
        {
            if (stats.bodyArmorLevel >= maxLevel)
            {
                PlaySound(buyFailSFX);
                return;
            }

            if (stats.money >= bodyArmorPrice)
            {
                stats.money -= bodyArmorPrice;
                stats.bodyArmorLevel++;

                PlaySound(buySuccessSFX);
                UpdateUI();
            }
            else
            {
                PlaySound(buyFailSFX);
            }
        });
    }

    public void BuyScooterSpeed()
    {
        AskForConfirmation("Scooter Speed", () =>
        {
            if (stats.scooterSpeedLevel >= maxLevel)
            {
                PlaySound(buyFailSFX);
                return;
            }

            if (stats.money >= scooterSpeedPrice)
            {
                stats.money -= scooterSpeedPrice;
                stats.scooterSpeedLevel++;

                PlaySound(buySuccessSFX);
                UpdateUI();
            }
            else
            {
                PlaySound(buyFailSFX);
            }
        });
    }

    public void BuyPackageHealth()
    {
        AskForConfirmation("Package Health", () =>
        {
            if (stats.packageHealthLevel >= maxLevel)
            {
                PlaySound(buyFailSFX);
                return;
            }

            if (stats.money >= rewardMultiplierPrice)
            {
                stats.money -= rewardMultiplierPrice;
                stats.packageHealthLevel++;

                PlaySound(buySuccessSFX);
                UpdateUI();
            }
            else
            {
                PlaySound(buyFailSFX);
            }
        });
    }

    // -------------------------
    // UI
    // -------------------------

    void UpdateMoneyOnly()
    {
        if (stats == null) return;

        moneyText.text = $"Money: ${Mathf.FloorToInt(stats.money)}";
    }

    void UpdateUI()
    {
        if (stats == null) return;

        UpdateMoneyOnly();

        // Price labels
        damageText.text = $"${damagePrice}";
        timeText.text = $"${bodyArmorPrice}";
        speedText.text = $"${scooterSpeedPrice}";
        rewardText.text = $"${rewardMultiplierPrice}";

        // Sliders
        if (damageSlider != null)
        {
            damageSlider.maxValue = maxLevel;
            damageSlider.value = stats.weaponDamageLevel;
        }

        if (timeSlider != null)
        {
            timeSlider.maxValue = maxLevel;
            timeSlider.value = stats.bodyArmorLevel;
        }

        if (speedSlider != null)
        {
            speedSlider.maxValue = maxLevel;
            speedSlider.value = stats.scooterSpeedLevel;
        }

        if (rewardSlider != null)
        {
            rewardSlider.maxValue = maxLevel;
            rewardSlider.value = stats.packageHealthLevel;
        }
    }

    // -------------------------
    // CONFIRMATION POPUP
    // -------------------------

    void AskForConfirmation(string upgradeName, System.Action purchaseAction)
    {
        pendingPurchase = purchaseAction;

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (confirmText != null)
            confirmText.text = $"Buy {upgradeName}?";

        PlaySound(popupOpenSFX);
    }

    void ConfirmPurchase()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        pendingPurchase?.Invoke();

        pendingPurchase = null;
    }

    void CancelPurchase()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        pendingPurchase = null;
    }

    // -------------------------
    // AUDIO
    // -------------------------

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}