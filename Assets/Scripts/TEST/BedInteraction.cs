using UnityEngine;
using UnityEngine.InputSystem;

public class BedInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public float interactRange = 3f;
    public string interactKey = "e";

    [Header("Prompt UI")]
    public GameObject sleepPrompt; // Small "Press E to sleep" text

    private bool isPlayerNearby = false;

    void Update()
    {
        if (player == null || DaySystem.Instance == null)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactRange;

        if (sleepPrompt != null)
            sleepPrompt.SetActive(inRange);

        if (inRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Sleep();
        }
    }

    void Sleep()
    {
        Debug.Log("🛏️ Player went to bed.");
        if (sleepPrompt != null)
            sleepPrompt.SetActive(false);

        DaySystem.Instance.SleepAndStartNextDay();
    }
}
