using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DaySystem : MonoBehaviour
{
    public static DaySystem Instance;

    [Header("Day Info")]
    public int currentDay = 1;
    public int deliveriesCompleted = 0;
    public float totalMoneyEarned = 0f;

    [Header("References")]
    public PhoneUI phoneUI;
    public DeliverySystem delivery;

    [Header("Fade Panel")]
    public GameObject fadePanel;  // Assign a full-screen UI panel
    private Image fadeImage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (fadePanel != null)
        {
            fadeImage = fadePanel.GetComponent<Image>();
            if (fadeImage == null)
                Debug.LogWarning("Fade panel needs an Image component!");

            SetAlpha(0f);
            fadePanel.SetActive(true); // Always active for fading
        }
    }

    // ===========================
    //  Called from phone “End Day”
    // ===========================
    public void EndDay()
    {
        Debug.Log("Ending Day " + currentDay);
        delivery.hasActiveOrder = false;
    }

    // ===========================
    //  Called when sleeping in bed
    // ===========================
    public void SleepAndStartNextDay()
    {
        StartCoroutine(FadeToMorning());
    }

    private IEnumerator FadeToMorning()
    {
        // Fade to black
        yield return StartCoroutine(Fade(1f, 1f));

        currentDay++;
        deliveriesCompleted = 0;
        totalMoneyEarned = 0f;

        if (phoneUI != null)
            phoneUI.GenerateOrders();

        Debug.Log("🌞 Starting Day " + currentDay);

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Fade(0f, 1f));
    }

    // ===========================
    //  Fade helpers
    // ===========================
    private IEnumerator Fade(float targetAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
