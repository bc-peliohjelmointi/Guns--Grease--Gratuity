using UnityEngine;

public class ShopInteract : MonoBehaviour
{
    public GameObject shopUI;      
    public string playerTag = "Player";

    private bool playerInRange = false;

    private void Start()
    {
        if (shopUI != null)
            shopUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            CloseShop();
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }

    void ToggleShop()
    {
        bool active = !shopUI.activeSelf;
        shopUI.SetActive(active);

        // freeze player movement if you use a controller
        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void CloseShop()
    {
        shopUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
