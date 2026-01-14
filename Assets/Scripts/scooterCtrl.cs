using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class scooterCtrl : MonoBehaviour
{
    [Header("Main")]
    public Transform scooterRoot;
    public Transform visualModel; // Scooter body for leaning

    [Header("Movement Settings")]
    public float acceleration = 20f;
    public float deceleration = 15f;
    public float maxSpeed = 30f;
    public float turnSpeed = 65f;
    public float leanAmount = 12f;

    [Header("Control")]
    public bool canControl = false;

    private float currentSpeed = 0f;
    private Rigidbody rb;
    
    private void Start()
    {
        rb = scooterRoot.GetComponent<Rigidbody>();
        rb.freezeRotation = true; // We will handle rotation manually
    }

    private void FixedUpdate()
    {
        if (!canControl)
        {
            currentSpeed = 0f;
            return;
        }

        HandleMovement();
        HandleTurningAndLean();
    }

    private void HandleMovement()
    {
        float forwardInput = 0f;
        if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed) forwardInput = 1f;
        if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed) forwardInput = -1f;

        // Accelerate or brake
        if (forwardInput > 0f)
        {
            currentSpeed += forwardInput * acceleration * Time.fixedDeltaTime;
        }
        else if (forwardInput < 0f)
        {
            // Braking only, no reversing
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2f * Time.fixedDeltaTime);
        }
        else
        {
            // Natural deceleration
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // Move scooter with Rigidbody for collision
        // PHYSICS-BASED movement (collision-safe)
        Vector3 velocity = transform.forward * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    private void HandleTurningAndLean()
    {
        if (currentSpeed < 0.1f) // Only turn when moving
        {
            // Reset lean
            visualModel.localRotation = Quaternion.Lerp(
                visualModel.localRotation,
                Quaternion.identity,
                Time.fixedDeltaTime * 5f
            );
            return;
        }

        float turnInput = 0f;
        if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed) turnInput = -1f;
        if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed) turnInput = 1f;

        // Turn scooter
        if (turnInput != 0f)
        {
            scooterRoot.Rotate(Vector3.up, turnInput * turnSpeed * Time.fixedDeltaTime);
        }

        // Lean visual model
        float lean = -turnInput * leanAmount * Mathf.Clamp01(currentSpeed / maxSpeed);
        Quaternion targetLean = Quaternion.Euler(0f, 0f, lean);
        visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, targetLean, Time.fixedDeltaTime * 5f);
    }
}
