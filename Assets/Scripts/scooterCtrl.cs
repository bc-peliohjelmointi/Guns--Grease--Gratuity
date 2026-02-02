using UnityEngine;
using UnityEngine.InputSystem;

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
    public bool canControl = false; // player mount
    public bool powerOn = false; // scooter ignition

    [Header("Battery")]
    public float maxBattery = 100f;
    public float movingDrainPM = 20f;
    public float idleDrainPM = 2f;  // when not moving but engine is on
    public float currentBattery;

    public bool hasBattery => currentBattery > 0.1f;

    private float currentSpeed = 0f;
    private Rigidbody rb;
    
    private void Start()
    {
        rb = scooterRoot.GetComponent<Rigidbody>();
        rb.freezeRotation = true; // We will handle rotation manually
        currentBattery = maxBattery;
    }

    private void FixedUpdate()
    {
        // when not mounted nor powered, scooter will coast to stop
        if (!canControl || !powerOn || !hasBattery)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);

            Vector3 stopVel = scooterRoot.forward * currentSpeed;
            stopVel.y = rb.linearVelocity.y;
            rb.linearVelocity = stopVel;
            return;
        }

        HandleMovement();
        HandleTurningAndLean();
    }

    private void HandleMovement()
    {
        DrainBattery();

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
        if (Mathf.Abs(currentSpeed) < 0.1f) // Only turn when moving
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
        // Speed sterngth
        float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);

        // Lean direction flips when reversing
        float directionMultiplier = currentSpeed >= 0 ? 1f : -1f;

        float lean = -turnInput * directionMultiplier * leanAmount * speedPercent;

        Quaternion targetLean = Quaternion.Euler(0f, 0f, lean);
        visualModel.localRotation = Quaternion.Lerp(visualModel.localRotation, targetLean, Time.fixedDeltaTime * 5f);

        // float lean = -turnInput * leanAmount * Mathf.Clamp01(currentSpeed / maxSpeed);
    }

    private void DrainBattery()
    {
        if (!powerOn || !canControl) return;

        float drainRate = Mathf.Abs(currentSpeed) > 1f ? movingDrainPM : idleDrainPM;

        float drainPerSecond = drainRate / 60f;

        currentBattery -= drainPerSecond * Time.fixedDeltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f,maxBattery);

        if (currentBattery <= 0f && powerOn)
        {
            powerOn = false;
        }
    }

    private void ChargeBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
    }
}
