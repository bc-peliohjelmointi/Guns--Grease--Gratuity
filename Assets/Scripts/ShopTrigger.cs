using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem; // Uusi input system

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopCanvas;        // Shop Canvas
    public GameObject promptCanvas;      // "Press E" teksti
    public GameObject phoneCanvas;
    public GameObject guiCanvas;
    public FirstPersonController controller;
    private bool playerInRange = false;

    void Start()
    {
        shopCanvas.SetActive(false);
        promptCanvas.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenShop();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame && shopCanvas.activeSelf)
        {
            CloseShop();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            promptCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            promptCanvas.SetActive(false);
            CloseShop();
        }
    }

    public void OpenShop()
    {
        guiCanvas.SetActive(false);
        phoneCanvas.SetActive(false);
        shopCanvas.SetActive(true);
        promptCanvas.SetActive(false);

        controller.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseShop()
    {
        shopCanvas.SetActive(false);
        guiCanvas.SetActive(true);

        controller.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
