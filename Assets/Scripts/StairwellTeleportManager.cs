using UnityEngine;

public class StairwellTeleportManager : MonoBehaviour
{
    [Header("Teleport Targets")]
    public Transform stairwellTarget; // entry destination
    public DeliverySystem deliverySystem;
    public bool isInStairwell;


    private void Start()
    {
        isInStairwell = false;
    }

    public void TeleportToStairwell(Transform player)
    {
        Teleport(player, stairwellTarget);
        isInStairwell = true;
        Debug.Log("Player In Stairwell");
    }

    public void TeleportToDeliveryZone(Transform player)
    {
        if (deliverySystem == null) return;

        Transform zone = deliverySystem.GetActiveDeliveryZoneTransform();
        if (zone == null) return;

        Teleport(player, zone);
        Debug.Log("Player Not In Stairwell");
    }

    private void Teleport(Transform player, Transform target)
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
