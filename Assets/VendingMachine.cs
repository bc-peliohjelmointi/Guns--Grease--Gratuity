using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class VendingMachine : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 3f;
    public Key interactKey = Key.E;
    public int batteryCost = 5;

    [Header("Battery Spawn")]
    public GameObject batteryPrefab;
    public Transform batterySpawnPoint;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    private Transform player;
    private PlayerStats stats;

    private bool playerInRange = false;
    private bool isShowingMessage = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stats = PlayerStats.Instance;
    }

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        playerInRange = dist <= interactDistance;

        if (!playerInRange)
        {
            statusText.text = "";
            return;
        }

        if (!isShowingMessage)
        {
            statusText.text = $"[E] Buy battery pack ({batteryCost}$)";
        }

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
            DispenseBattery();
        }

        else
        {
            StartCoroutine(ShowTempMessage("Not enough money."));
        }
    }

    private void DispenseBattery()
    {
        Instantiate(batteryPrefab, batterySpawnPoint.position, batterySpawnPoint.rotation);
    }

    private IEnumerator ShowTempMessage(string message)
    {
        isShowingMessage = true;
        statusText.text = message;

        yield return new WaitForSeconds(1.5f);

        isShowingMessage = false;
    }
}
