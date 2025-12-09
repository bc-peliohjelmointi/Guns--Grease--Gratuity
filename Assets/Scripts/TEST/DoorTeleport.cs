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
    public float interactDistance = 3f;
    public bool requireKeyPress = true;

    [Header("Look Settings")]
    public Camera playerCamera;
    public LayerMask doorLayer;

    [Header("UI")]
    public TextMeshProUGUI interactText;
    public Image fadePanel;

    [Header("Fade Settings")]
    public float fadeDuration = 0.75f;
    public float blackHoldTime = 1f;   // ✅ EXTRA 1 SECOND BLACK SCREEN

    private CharacterController playerController;
    private bool isFading = false;

    void Start()
    {
        playerController = FindObjectOfType<CharacterController>();

        if (interactText != null)
            interactText.gameObject.SetActive(false);

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
        }
    }

    void Update()
    {
        bool lookingAtDoor = IsLookingAtDoor();

        if (interactText != null)
            interactText.gameObject.SetActive(lookingAtDoor);

        if (lookingAtDoor && requireKeyPress && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeTeleport());
        }

        if (lookingAtDoor && !requireKeyPress)
        {
            StartCoroutine(FadeTeleport());
        }
    }

    bool IsLookingAtDoor()
    {
        if (playerCamera == null) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, doorLayer))
        {
            return hit.transform == transform;
        }

        return false;
    }

    IEnumerator FadeTeleport()
    {
        if (isFading || fadePanel == null) yield break;
        isFading = true;

        // ✅ FADE TO BLACK
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            Color c = fadePanel.color;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        // ✅ HOLD BLACK FOR 1 FULL SECOND
        yield return new WaitForSeconds(blackHoldTime);

        // ✅ TELEPORT
        TeleportNow();

        // ✅ FADE BACK IN
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

        playerController.enabled = false;

        playerController.transform.position = teleportTarget.position;
        playerController.transform.rotation = teleportTarget.rotation;

        playerController.enabled = true;

        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }
}
