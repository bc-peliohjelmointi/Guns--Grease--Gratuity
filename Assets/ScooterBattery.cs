using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ScooterBattery : MonoBehaviour
{
    // how much the battery charges at once (tier 1)
    public float chargeAmount = 30.0f;

    private void OnTriggerEnter(Collider other)
    {
        ScooterMount player = other .GetComponent<ScooterMount>();

        if (player != null && !player.isMounted && !player.HasBattery())
        {
            player.GetBattery(chargeAmount);
            Destroy(gameObject);
        }
    }
}
