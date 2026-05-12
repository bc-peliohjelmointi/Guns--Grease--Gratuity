using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class VendingMachine : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 3f;
    public int batteryCost = 5;

    [Header("Battery Spawn")]
    public GameObject batteryPrefab;
    public Transform batterySpawnPoint;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    private Transform player;
    private PlayerStats stats;

    private bool isShowingMessage = false;

    // Tracks the current active vending machine
    private static VendingMachine currentMachine;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        stats = PlayerStats.Instance;

        if (statusText != null)
            statusText.text = "";
    }

    private void Update()
    {
        if (player == null || statusText == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        bool inRange = distance <= interactDistance;

        // If not in range and THIS machine owns the UI, clear it
        if (!inRange)
        {
            if (currentMachine == this)
            {
                statusText.text = "";
                currentMachine = null;
            }

            return;
        }

        // If another closer machine is already active, ignore
        if (currentMachine != null && currentMachine != this)
        {
            float currentDist =
                Vector3.Distance(player.position, currentMachine.transform.position);

            // Only replace if THIS machine is closer
            if (distance >= currentDist)
                return;
        }

        // Become active machine
        currentMachine = this;

        // Show prompt
        if (!isShowingMessage)
        {
            statusText.text = $"[E] Buy Battery (${batteryCost})";
        }

        // Purchase
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryBatteryPurchase();
        }
    }

    private void TryBatteryPurchase()
    {
        if (stats.money >= batteryCost)
        {
            stats.money -= batteryCost;

            Instantiate(
                batteryPrefab,
                batterySpawnPoint.position,
                batterySpawnPoint.rotation
            );

            StartCoroutine(ShowTempMessage("Battery Purchased!"));
        }
        else
        {
            StartCoroutine(ShowTempMessage("Not enough money."));
        }
    }

    private IEnumerator ShowTempMessage(string message)
    {
        isShowingMessage = true;

        statusText.text = message;

        yield return new WaitForSeconds(1.5f);

        isShowingMessage = false;
    }
}