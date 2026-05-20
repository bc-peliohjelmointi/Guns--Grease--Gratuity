using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class scooterCtrl : MonoBehaviour
{
    [Header("Main")]
    public Transform scooterRoot;
    public Transform visualModel;
    public Transform scooterSpawnPoint;

    private PlayerStats stats;

    [Header("Movement Settings")]
    public float acceleration = 20f;
    public float deceleration = 15f;
    public float brakeDeceleration = 30f;

    public float maxSpeed = 30f;
    public float maxReverse = -30f;

    public float turnSpeed = 30f;
    public float leanAmount = 30f;

    public bool isResetting = false;
    public float upgradeAmount = 0.25f;

    [Header("Ground")]
    public LayerMask groundMask;
    public float heightOffset = 0.15f;

    [Header("Control")]
    public bool canControl = false;
    public bool powerOn = false;
    private bool wasMountedLastFrame = false;

    [Header("Battery")]
    public float maxBattery = 100f;
    public float movingDrainPM = 20f;
    public float idleDrainPM = 2f;
    public float currentBattery;

    [Header("Audio")]
    public AudioSource engineSource;
    public AudioSource sfxSource;

    public AudioClip powerOnClip;
    public AudioClip powerOffClip;
    public AudioClip brakeClip;
    public AudioClip engineLoopClip;

    [Range(0.5f, 2f)]
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    public bool hasBattery => currentBattery > 0.1f;

    public float currentSpeed = 0f;
    private Rigidbody rb;

    [Header("Base Movement (For upgrades)")]
    private float baseAcceleration;
    private float baseDeceleration;
    private float baseBrakeDeceleration;
    private float baseMaxSpeed;

    private void Start()
    {
        stats = PlayerStats.Instance;

        rb = scooterRoot.GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        currentBattery = maxBattery;

        baseAcceleration = acceleration;
        baseDeceleration = deceleration;
        baseBrakeDeceleration = brakeDeceleration;
        baseMaxSpeed = maxSpeed;

        if (engineSource != null && engineLoopClip != null)
        {
            engineSource.clip = engineLoopClip;
            engineSource.loop = true;
        }
    }

    private void Update()
    {
        ApplyUpgrades();
        HandlePowerInput();
    }

    private void FixedUpdate()
    {
        if (isResetting) return;

        if (!canControl || !powerOn || !hasBattery)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);

            Vector3 stopVel = scooterRoot.forward * currentSpeed;
            stopVel.y = rb.linearVelocity.y;
            rb.linearVelocity = stopVel;
            return;
        }

        HandleMovement();
        ApplyGroundAssist();
        HandleTurningAndLean();
        DrainBattery();
        UpdateEngineSound();
    }

    private void ApplyUpgrades()
    {
        if (stats == null) return;

        float multiplier = 1f + (stats.scooterSpeedLevel * upgradeAmount);

        acceleration = baseAcceleration * multiplier;
        deceleration = baseDeceleration * multiplier;
        brakeDeceleration = baseBrakeDeceleration * multiplier;
        maxSpeed = baseMaxSpeed * multiplier;
    }

    private void HandleMovement()
    {
        float input = 0f;
        if (Keyboard.current.wKey.isPressed) input = 1f;
        if (Keyboard.current.sKey.isPressed) input = -1f;

        bool hardBrake = Keyboard.current.spaceKey.isPressed;

        if (hardBrake)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                brakeDeceleration * 3f * Time.fixedDeltaTime
            );
        }
        else
        {
            if (input != 0f)
            {
                currentSpeed += input * acceleration * Time.fixedDeltaTime;
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0f,
                    deceleration * Time.fixedDeltaTime
                );
            }
        }

        currentSpeed = Mathf.Clamp(currentSpeed, maxReverse, maxSpeed);

        Vector3 velocity = scooterRoot.forward * currentSpeed;
        velocity.y = rb.linearVelocity.y; // keep gravity working
        rb.linearVelocity = velocity;
    }

    private void ApplyGroundAssist()
    {
        Vector3 origin = rb.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1.5f, groundMask))
        {
            Vector3 pos = rb.position;

            // gently follow ground but NEVER hard snap
            float targetY = hit.point.y + heightOffset;

            pos.y = Mathf.Lerp(pos.y, targetY, 0.05f);

            rb.position = pos;
        }
    }

    private void HandleTurningAndLean()
    {
        if (Mathf.Abs(currentSpeed) < 0.1f)
        {
            visualModel.localRotation = Quaternion.Lerp(
                visualModel.localRotation,
                Quaternion.identity,
                Time.fixedDeltaTime * 5f
            );
            return;
        }

        float turnInput = 0f;
        if (Keyboard.current.aKey.isPressed) turnInput = -1f;
        if (Keyboard.current.dKey.isPressed) turnInput = 1f;

        if (turnInput != 0f)
        {
            scooterRoot.Rotate(Vector3.up, turnInput * turnSpeed * Time.fixedDeltaTime);
        }

        float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        float directionMultiplier = currentSpeed >= 0 ? 1f : -1f;

        float lean = -turnInput * directionMultiplier * leanAmount * speedPercent;

        Quaternion targetLean = Quaternion.Euler(0f, 0f, lean);

        visualModel.localRotation = Quaternion.Lerp(
            visualModel.localRotation,
            targetLean,
            Time.fixedDeltaTime * 5f
        );
    }

    private void HandlePowerInput()
    {
        if (wasMountedLastFrame && !canControl)
        {
            SetPower(false);
        }

        wasMountedLastFrame = canControl;

        if (!canControl) return;

        if (Keyboard.current.wKey.wasPressedThisFrame && !powerOn && hasBattery)
            SetPower(true);

        if (Keyboard.current.eKey.wasPressedThisFrame && powerOn)
            SetPower(false);
    }

    private void DrainBattery()
    {
        if (!powerOn || !canControl) return;
        if (currentBattery <= 0f) return;

        float drainRate = Mathf.Abs(currentSpeed) > 1f ? movingDrainPM : idleDrainPM;
        float drainPerSecond = drainRate / 60f;

        currentBattery -= drainPerSecond * Time.fixedDeltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        if (currentBattery <= 0f)
            powerOn = false;
    }

    public void ChargeBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0f, maxBattery);
    }

    public void SetPower(bool state)
    {
        if (powerOn == state) return;

        powerOn = state;

        if (powerOn)
        {
            if (sfxSource && powerOnClip)
                sfxSource.PlayOneShot(powerOnClip);

            if (engineSource && engineLoopClip)
                engineSource.Play();
        }
        else
        {
            if (sfxSource && powerOffClip)
                sfxSource.PlayOneShot(powerOffClip);

            if (engineSource)
                engineSource.Stop();
        }
    }

    private void UpdateEngineSound()
    {
        if (engineSource == null || !engineSource.isPlaying)
            return;

        float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);
    }
}