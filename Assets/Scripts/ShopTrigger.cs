using UnityEngine;
using StarterAssets;
using TMPro;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject shopCanvas;       // Shop UI
    public TextMeshProUGUI interactText; // "Press E" text in HUD
    public GameObject phoneCanvas;      // Optional phone UI
    public GameObject guiCanvas;        // Player HUD

    [Header("Player Controller")]
    public FirstPersonController controller;

    private bool playerInRange = false;

    private void Start()
    {
        shopCanvas.SetActive(false);

        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (shopCanvas.activeSelf)
                CloseShop();
            else
                OpenShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactText != null)
            {
                interactText.text = "Press E";
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
        guiCanvas.SetActive(false);
        phoneCanvas.SetActive(false);
        shopCanvas.SetActive(true);

        if (interactText != null)
            interactText.gameObject.SetActive(false);

        controller.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseShop()
    {
        shopCanvas.SetActive(false);
        guiCanvas.SetActive(true);

        controller.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
