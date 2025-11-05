using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using TMPro;

public class ScooterMount : MonoBehaviour
{
    public Transform scooter;
    public Transform mountPoint;
    public float mountDistance = 2f;
    public Key mountKey = Key.F;
    public TextMeshProUGUI statusText;

    private CharacterController playerController;
    private FirstPersonController fpsController;
    private bool isMounted = false;
    private scooterCtrl scooterControl;
    private Camera playerCamera;
    private Transform cameraOriginalParent;
    private Quaternion cameraOriginalRotation;

    public float moveSpeed = 5f;
    public float turnSpeed = 50f;

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
        if (Keyboard.current[mountKey].wasPressedThisFrame)
        {
            if (!isMounted && Vector3.Distance(transform.position, scooter.position) < mountDistance)
                MountScooter();
            else if (isMounted)
                DismountScooter();
        }

        if (isMounted)
        {
            // --- Liikkuminen WASD ---
            float move = 0f;
            if (Keyboard.current.wKey.isPressed)
                move = 1f;
            else if (Keyboard.current.sKey.isPressed)
                move = -1f;

            float turn = 0f;
            if (Keyboard.current.aKey.isPressed)
                turn = -1f;
            else if (Keyboard.current.dKey.isPressed)
                turn = 1f;

            scooter.Translate(Vector3.forward * move * moveSpeed * Time.deltaTime);
            scooter.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
        }

        UpdateStatusText();
    }

    private void MountScooter()
    {
        isMounted = true;
        fpsController.enabled = false;
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateStatusText();
    }

    private void DismountScooter()
    {
        isMounted = false;
        fpsController.enabled = true;
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
        Cursor.visible = false;
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;

        if (isMounted)
            statusText.text = "Paina [F] poistuaksesi skuutista";
        else if (Vector3.Distance(transform.position, scooter.position) < mountDistance)
            statusText.text = "Paina [F] noustaksesi skuuttiin";
        else
            statusText.text = "";
    }
}
