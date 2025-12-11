using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SleepTrigger : MonoBehaviour
{
    [Header("Player")]
    public Camera playerCamera;
    public float interactDistance = 3f;

    [Header("UI")]
    public TextMeshProUGUI interactText;
    public Image fadePanel;

    [Header("Sleep Settings")]
    public float sleepCooldown = 300f;
    public float fadeDuration = 1.5f;

    private bool canSleep = true;
    private Color fadeColor;

    //  NEW
    private PhoneUI phoneUI;

    private void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        if (fadePanel != null)
        {
            fadeColor = fadePanel.color;
            fadeColor.a = 0f;
            fadePanel.color = fadeColor;
            fadePanel.gameObject.SetActive(true);
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        //  NEW — find phoneUI in scene
        phoneUI = FindObjectOfType<PhoneUI>();
    }

    private void Update()
    {
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        interactText.gameObject.SetActive(false);

        if (!canSleep) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.transform == transform)
            {
                interactText.text = "Press E to sleep";
                interactText.gameObject.SetActive(true);

                if (Keyboard.current.eKey.wasPressedThisFrame)
                    StartCoroutine(SleepRoutine());
            }
        }
    }

    private IEnumerator SleepRoutine()
    {
        canSleep = false;
        interactText.gameObject.SetActive(false);

        //  NEW — close End Day panel automatically
        if (phoneUI != null)
            phoneUI.CloseEndDayPanel();

        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        PlayerStats.Instance.currentDay++;
        PlayerStats.Instance.ResetDayStats();

        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        yield return new WaitForSeconds(sleepCooldown);
        canSleep = true;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            fadeColor.a = a;
            fadePanel.color = fadeColor;
            yield return null;
        }
    }
}
