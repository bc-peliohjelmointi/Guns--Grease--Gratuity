using UnityEngine;

public class StairwellTeleportManager : MonoBehaviour
{
    [Header("Teleport Targets")]
    public Transform stairwellTarget;
    public DeliverySystem deliverySystem;
    public bool isInStairwell;

    private void Start()
    {
        isInStairwell = false;
    }

    public void TeleportToStairwell(Transform player)       // Teleports to the entrypoint inside the stairwell (Teleport in to the building)
    {
        Teleport(player, stairwellTarget);
        TutorialManager.Instance
        ?.NotifyTrigger(TutorialTriggerType.InBuilding);
        isInStairwell = true;
        Debug.Log("Player In Stairwell");
    }

    public void TeleportToDeliveryZone(Transform player)    // Teleports the player to the delivery zone outside (Out of the building teleport)
    {
        if (deliverySystem == null) return;

        Transform exit = deliverySystem.GetActiveDeliveryZoneTransform();
        if (exit == null) return;

        Teleport(player, exit);
        isInStairwell = false;
        Debug.Log("Player Not In Stairwell");
    }

    private void Teleport(Transform player, Transform target)   // Teleports the player resetting the rigibody and characterController to avoid bugs and conflicts
    {
        if (player == null || target == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            player.position = target.position;
            player.rotation = target.rotation;
            cc.enabled = true;
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.MovePosition(target.position);
            rb.MoveRotation(target.rotation);
            return;
        }

        player.position = target.position;
        player.rotation = target.rotation;
    }
}
