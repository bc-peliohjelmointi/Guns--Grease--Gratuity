using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class UpgradeShopTrigger : MonoBehaviour
{
    public GameObject shopUI;                  // Assign UpgradeShop canvas panel
    public TextMeshProUGUI promptText;         // Floating "Press E" text
    public float interactRange = 2.0f;

    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        if (promptText) promptText.gameObject.SetActive(false);
        if (shopUI) shopUI.SetActive(false);
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactRange;

        if (inRange && !playerInRange)
        {
            playerInRange = true;
            if (promptText) promptText.gameObject.SetActive(true);
        }
        else if (!inRange && playerInRange)
        {
            playerInRange = false;
            if (promptText) promptText.gameObject.SetActive(false);
            if (shopUI) shopUI.SetActive(false);
        }

        // Open shop
        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (shopUI)
                shopUI.SetActive(!shopUI.activeSelf);  // toggle
        }
    }
}
