using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

public class VendingMachine : MonoBehaviour
{
    [Header("Settings")]
    public float interactDistance = 3f;
    public Key interactKey = Key.E;
    public int cost = 5;

    [Header("Battery Spawn")]
    public GameObject batteryPrefab;
    public Transform spawnPoint;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    private Transform player;
    private PlayerStats stats;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        stats = PlayerStats.Instance;
    }

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactDistance)
        {
            if (statusText != null)
                statusText.text = $"[E] Buy battery pack ({cost}$)";

            if (Keyboard.current[interactKey].wasPressedThisFrame)
            {
                TryPurchase();
            }
        }
    }

    private void TryPurchase()
    {
        if (stats.money < cost)
        {
            statusText.text = "Not enough money.";
            return;
        }

        stats.money -= cost;
        DispenseBattery();
    }

    private void DispenseBattery()
    {
        Instantiate(batteryPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
