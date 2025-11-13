using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class DoorTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportTarget;
    public KeyCode interactKey = KeyCode.E;
    public bool requireKeyPress = true;

    [Header("UI")]
    public TextMeshProUGUI interactText;   // "Press E to enter"
    public Image fadePanel;                 // Black fullscreen UI image

    [Header("Fade Settings")]
    public float fadeDuration = 0.75f;

    private CharacterController playerController;
    private bool playerInTrigger = false;
    private bool isFading = false;

    void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c; // fully transparent at start
        }
    }

    void Update()
    {
        if (playerInTrigger && requireKeyPress && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeTeleport());
        }

        if (playerInTrigger && !requireKeyPress)
        {
            StartCoroutine(FadeTeleport());
        }
    }

    IEnumerator FadeTeleport()
    {
        if (isFading || fadePanel == null) yield break;
        isFading = true;

        // ✅ Fade to black
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            Color c = fadePanel.color;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        // ✅ Actual teleport
        TeleportNow();

        // ✅ Fade back to clear
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            Color c = fadePanel.color;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        isFading = false;
    }

    void TeleportNow()
    {
        if (playerController == null || teleportTarget == null)
        {
            Debug.LogWarning("Teleport failed: missing references.");
            return;
        }

        // Disable CharacterController before teleporting
        playerController.enabled = false;

        playerController.transform.position = teleportTarget.position;
        playerController.transform.rotation = teleportTarget.rotation;

        playerController.enabled = true;

        // Hide the UI text after teleport
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        playerInTrigger = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterController>(out var cc))
        {
            playerController = cc;
            playerInTrigger = true;

            if (interactText != null)
                interactText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null)
        {
            playerInTrigger = false;

            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }
}
