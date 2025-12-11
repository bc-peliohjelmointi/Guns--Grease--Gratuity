using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using TMPro;

public class ScooterMount : MonoBehaviour
{
    [Header("References")]
    public Transform scooter;
    public Transform mountPoint;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public float mountDistance = 2f;
    public Key mountKey = Key.F;

    private CharacterController playerController;
    private FirstPersonController fpsController;
    private scooterCtrl scooterControl;
    private Camera playerCamera;
    private Transform cameraOriginalParent;
    private Quaternion cameraOriginalRotation;

    [HideInInspector] public bool isMounted = false;

    private void Start()
    {
        playerController = GetComponent<CharacterController>();
        fpsController = GetComponent<FirstPersonController>();
        scooterControl = scooter.GetComponentInChildren<scooterCtrl>();
        playerCamera = Camera.main;

        if (playerCamera != null)
            cameraOriginalParent = playerCamera.transform.parent;

        UpdateStatusText();
    }

    private void Update()
    {
        HandleMountInput();
        UpdateStatusText();
    }

    private void HandleMountInput()
    {
        if (Keyboard.current[mountKey].wasPressedThisFrame)
        {
            if (!isMounted && Vector3.Distance(transform.position, scooter.position) < mountDistance)
                MountScooter();
            else if (isMounted)
                DismountScooter();
        }

        if (isMounted)
        {
            // WASD movement handled by scooterCtrl, so we just pass control
            // Optional: could add minor forward/backward movement here if needed
        }
    }

    private void MountScooter()
    {
        isMounted = true;
        fpsController.canMove = false;
        playerController.enabled = false;

        transform.SetParent(scooter);
        transform.position = mountPoint.position;
        transform.rotation = mountPoint.rotation;

        if (playerCamera != null)
        {
            cameraOriginalRotation = playerCamera.transform.localRotation;
            playerCamera.transform.SetParent(mountPoint);
            playerCamera.transform.localPosition = Vector3.zero;
            playerCamera.transform.localRotation = Quaternion.identity;
        }

        if (scooterControl != null)
            scooterControl.canControl = true;

        // Lock cursor for riding
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Disable gun while riding
        var gunScript = GetComponentInChildren<PullOurScript>();
        if (gunScript != null)
        {
            gunScript.gunIsOut = false;
            gunScript.animator.SetBool("GunOut", false);
        }
    }

    private void DismountScooter()
    {
        isMounted = false;
        fpsController.canMove = true;
        playerController.enabled = true;

        transform.SetParent(null);
        transform.position = scooter.position + scooter.right * 1f;

        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(cameraOriginalParent);
            playerCamera.transform.localRotation = cameraOriginalRotation;
        }

        if (scooterControl != null)
            scooterControl.canControl = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;

        if (isMounted)
            statusText.text = "[F] Mount";
        else if (Vector3.Distance(transform.position, scooter.position) < mountDistance)
            statusText.text = "[F] Discmount";
        else
            statusText.text = "";
    }
}
