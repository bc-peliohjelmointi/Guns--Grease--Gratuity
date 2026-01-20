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
    public float brakeDeceleration = 30f;
    public float maxSpeed = 30f;
    public float maxReverse = -30f;
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
        float movementInput = 0f;
        if (UnityEngine.InputSystem.Keyboard.current.wKey.isPressed) movementInput = 1f;
        if (UnityEngine.InputSystem.Keyboard.current.sKey.isPressed) movementInput = -1f;

        float movementDirection = Vector3.Dot(rb.linearVelocity, transform.forward);

        // current deceleration speed to deceleration strenght
        float currentDeceleration = deceleration;

        // braking on opposing input
        if (movementInput != 0f && movementDirection != 0f && Mathf.Sign(movementInput) != Mathf.Sign(movementDirection))
        {
            currentDeceleration = brakeDeceleration;
        }

        // Accelerate when direction is the same as input or when stopped
        if (movementInput != 0f)
        {
            float inputDetermination = movementInput > 0f ? acceleration : deceleration;

            currentSpeed += movementInput * inputDetermination * Time.fixedDeltaTime;
        }

        else
        {
            // deceleration
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, maxReverse, maxSpeed);

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
