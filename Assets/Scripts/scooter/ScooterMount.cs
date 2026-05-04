using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using TMPro;

public class ScooterMount : MonoBehaviour
{
    [Header("References")]
    public Transform scooter;
    public Transform mountPoint;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI statusTxt;

    [Header("Settings")]
    // mount
    public float mountDistance = 2f;
    public Key mountKey = Key.F;
    // power
    public Key powerKey = Key.E;
    // battery
    public float chargeRange = 3.0f;
    public Key chargeKey = Key.Y;

    private CharacterController playerController;
    private FirstPersonController fpsController;
    private scooterCtrl scooterControl;
    private Camera playerCamera;
    private Transform cameraOriginalParent;
    private Quaternion cameraOriginalRotation;

    [HideInInspector] public bool isMounted = false;
    [HideInInspector] private bool hasBatteryPack = false;
    [HideInInspector] private float storedChargeAmount = 0f;

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
        //HandlePowerInput();
        UpdateStatusText();
        HandleScooterCharging();
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
        scooter.tag = "PlayerScooter";

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
            scooterControl.powerOn = false; // power off on mount

        // Lock cursor for riding
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Disable gun while riding
        var gunScript = GetComponentInChildren<PullOurScript>();
        // if (gunScript != null)
        // {
        //     gunScript.gunIsOut = false;
        //     gunScript.animator.SetBool("GunOut", false);
        // }
    }

    private void DismountScooter()
    {
        scooter.tag = "Scooter"; // or "Scooter"

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
            //scooterControl.SetPower(false); // power off on dismount

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    // Scooter power based on if mounted and if key is pressed
    private void HandlePowerInput()
    {
        if (statusTxt == null)
            return;

        if (!isMounted || scooterControl == null)
        {
            statusTxt.text = "";
            return;
        }

        if (Keyboard.current[powerKey].wasPressedThisFrame)
        {
            scooterControl.SetPower(!scooterControl.powerOn);
        }

        statusTxt.text = scooterControl.powerOn
            ? ""
            : "[E] Turn Scooter On";
    }

    // battery pack collection
    public void GetBattery(float amount)
    {
        hasBatteryPack = true;
        storedChargeAmount = amount;

        if (batteryText.text != null)
            batteryText.text = "Battery pack collected";
    }

    public bool HasBattery()
    {
        return hasBatteryPack;
    }

    // Scooter charge control and battery pack usage
    private void HandleScooterCharging()
    {
        if (scooterControl == null) return;

        float distance = Vector3.Distance(transform.position, scooter.position);

        if (!isMounted && hasBatteryPack && distance <= chargeRange)
        {
            if (batteryText != null)
                batteryText.text = "[Y] Charge Scooter";

            if (Keyboard.current[chargeKey].wasPressedThisFrame)
            {
                scooterControl.ChargeBattery(storedChargeAmount);

                hasBatteryPack = false;
                storedChargeAmount = 0f;
            }
        }
    }

    // .
    private void UpdateStatusText()
    {
        if (batteryText == null || scooterControl == null)
            return;

        if (isMounted)
        {
            // Show battery
            batteryText.text = $"{Mathf.RoundToInt(scooterControl.currentBattery)}";

            // CLEAR status text when mounted
            statusTxt.text = "";
            return;
        }

        float distance = Vector3.Distance(transform.position, scooter.position);

        if (distance < mountDistance)
        {
            statusTxt.text = "[F] Mount";
        }
        else
        {
            statusTxt.text = "";
        }
    }
}
