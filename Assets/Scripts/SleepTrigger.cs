using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SleepTrigger : MonoBehaviour
{
    [Header("UI & Fade")]
    public TextMeshProUGUI interactText;  // "Press E to sleep"
    public GameObject fadePanel;          // Assign your full-screen fade panel

    [Header("Sleep Settings")]
    public float sleepCooldown = 300f;    // 5 minutes
    public float fadeDuration = 3f;       // 3 seconds fade

    private bool playerInRange = false;
    private bool canSleep = true;

    private void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        if (fadePanel != null)
            fadePanel.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && canSleep && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(SleepRoutine());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canSleep)
        {
            playerInRange = true;
            if (interactText != null)
            {
                interactText.text = "Press E to sleep";
                interactText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }

    private IEnumerator SleepRoutine()
    {
        canSleep = false;
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        if (fadePanel != null)
            fadePanel.SetActive(true);

        // Wait for fade duration
        yield return new WaitForSeconds(fadeDuration);

        // Advance the day and reset daily stats
        PlayerStats.Instance.currentDay++;
        PlayerStats.Instance.ResetDayStats();

        // Remove fade
        if (fadePanel != null)
            fadePanel.SetActive(false);

        // Start cooldown
        yield return new WaitForSeconds(sleepCooldown);

        canSleep = true;
    }
}
