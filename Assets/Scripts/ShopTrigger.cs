using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject shopCanvas;         // Shop UI panel
    public TextMeshProUGUI interactText;  // "Press E" text in HUD
    public GameObject phoneCanvas;        // Optional phone UI to hide
    public GameObject guiCanvas;          // Player HUD to hide

    [Header("Player Controller")]
    public FirstPersonController controller; // Reference to player controller

    private bool playerInRange = false;

    private void Start()
    {
        // Hide shop and prompt at start
        if (shopCanvas != null)
            shopCanvas.SetActive(false);

        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Open or close shop when player presses E
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (shopCanvas != null)
            {
                if (shopCanvas.activeSelf)
                    CloseShop();
                else
                    OpenShop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactText != null)
            {
                interactText.text = "[E] to upgrade";
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

            CloseShop();
        }
    }

    private void OpenShop()
    {
        // Hide other UI
        if (guiCanvas != null) guiCanvas.SetActive(false);
        if (phoneCanvas != null) phoneCanvas.SetActive(false);

        // Show shop
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(true);

            // Update the upgrade shop UI every time it's opened
            UpgradeShop upgradeShop = shopCanvas.GetComponent<UpgradeShop>();
        }

        if (interactText != null)
            interactText.gameObject.SetActive(false);

        // Freeze player
        if (controller != null)
            controller.canMove = false;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseShop()
    {
        // Hide shop UI
        if (shopCanvas != null)
            shopCanvas.SetActive(false);

        // Restore other UI
        if (guiCanvas != null) guiCanvas.SetActive(true);
        if (phoneCanvas != null) phoneCanvas.SetActive(true);

        // Unfreeze player
        if (controller != null)
            controller.canMove = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
