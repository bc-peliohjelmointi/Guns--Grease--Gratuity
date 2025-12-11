using UnityEngine;
using UnityEngine.InputSystem;

public class scooterCtrl : MonoBehaviour
{
    [Header("Main")]
    public Transform scooterRoot;
    public Transform visualModel;   // Scooter body for leaning

    [Header("Movement Settings")]
    public float acceleration = 5f;
    public float deceleration = 4f;
    public float maxSpeed = 12f;
    public float turnSpeed = 65f;
    public float leanAmount = 15f;

    [Header("Control")]
    public bool canControl = false;

    private float currentSpeed = 0f;

    void Update()
    {
        if (!canControl)
        {
            currentSpeed = 0;
            return;
        }

        float forward = 0f;

        if (Keyboard.current.wKey.isPressed) forward = 1f;
        if (Keyboard.current.sKey.isPressed) forward = -1f;

        // --- ACCELERATION ---
        if (forward != 0)
        {
            currentSpeed += forward * acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        // --- MOVE SCOOTER ---
        scooterRoot.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // --- TURN INPUT ---
        float turnInput = 0f;
        if (Keyboard.current.aKey.isPressed) turnInput = -1f;
        if (Keyboard.current.dKey.isPressed) turnInput = 1f;

        // --- TURN + LEAN ONLY IF MOVING ---
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            if (turnInput != 0)
                scooterRoot.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);

            float lean = -turnInput * leanAmount * Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            Quaternion targetLean = Quaternion.Euler(0, 0, lean);
            visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, targetLean, Time.deltaTime * 5f);
        }
        else
        {
            // reset lean when stopped
            visualModel.localRotation = Quaternion.Lerp(
                visualModel.localRotation,
                Quaternion.identity,
                Time.deltaTime * 5f
            );
        }
    }
}
