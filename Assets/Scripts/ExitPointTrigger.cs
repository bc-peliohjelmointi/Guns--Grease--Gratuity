using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ExitPointTrigger : MonoBehaviour
{
    [SerializeField] private StairwellTeleportManager teleportManager;
    [SerializeField] private DeliverySystem deliverySystem;
    [SerializeField] private TurretActivator turretActivator;

    private bool playerInside = false;
    private Transform player;
    private float processingSeconds = 0.1f;

    private void Awake()
    {
        if (teleportManager == null)
            teleportManager = FindObjectOfType<StairwellTeleportManager>();

        if (deliverySystem == null)
            deliverySystem = FindObjectOfType<DeliverySystem>();
        if (turretActivator == null)
            turretActivator = FindObjectOfType<TurretActivator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        player = other.transform;

        deliverySystem.playerAtExitPoint = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        player = null;

        deliverySystem.playerAtExitPoint = false;
    }

    private void Update()
    {
        if (!playerInside) return;
        if (!deliverySystem.pendingDelivery) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        StartCoroutine(ExitAndDeliver());
    }

    private IEnumerator ExitAndDeliver()
    {
        deliverySystem.pendingDelivery = false;

        yield return deliverySystem.StartCoroutine(deliverySystem.FadeOut());
        teleportManager.TeleportToDeliveryZone(player);
        yield return new WaitForSeconds(processingSeconds);
        deliverySystem.DeliverPackage();
        turretActivator.DestroyTurrets();
        yield return deliverySystem.StartCoroutine(deliverySystem.FadeIn());
    }
}
