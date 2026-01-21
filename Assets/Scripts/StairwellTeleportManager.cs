using UnityEngine;

public class StairwellTeleportManager : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform stairwellTarget; // Shared stairwell destination

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger teleport when entering a DeliveryZone
        if (other.CompareTag("DeliveryZone"))
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;

            if (player != null)
            {
                TeleportPlayer(player);
            }
        }
    }

    public void TeleportPlayer(Transform playerTransform)
    {
        if (playerTransform == null || stairwellTarget == null) return;

        // Handle CharacterController
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            playerTransform.position = stairwellTarget.position;
            playerTransform.rotation = stairwellTarget.rotation;
            cc.enabled = true;
            return;
        }

        // Handle Rigidbody
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.MovePosition(stairwellTarget.position);
            rb.MoveRotation(stairwellTarget.rotation);
            return;
        }

        // Default teleport
        playerTransform.position = stairwellTarget.position;
        playerTransform.rotation = stairwellTarget.rotation;
    }
}
