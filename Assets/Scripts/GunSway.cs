using UnityEngine;
using UnityEngine.InputSystem;

public class GunSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 2f;
    public float maxSway = 5f;
    public float smoothSpeed = 8f;

    [Header("Position Sway")]
    public float positionAmount = 0.02f;
    public float maxPosition = 0.05f;

    Vector3 currentRotation;
    Vector3 currentPosition;

    void Update()
    {
        // Get mouse input (same system you're using)
        float mouseX = Mouse.current.delta.ReadValue().x * 0.01f;
        float mouseY = Mouse.current.delta.ReadValue().y * 0.01f;

        // --- ROTATION SWAY ---
        float rotX = -mouseY * swayAmount;
        float rotY = mouseX * swayAmount;

        Vector3 targetRot = new Vector3(rotX, rotY, rotY);

        targetRot.x = Mathf.Clamp(targetRot.x, -maxSway, maxSway);
        targetRot.y = Mathf.Clamp(targetRot.y, -maxSway, maxSway);

        currentRotation = Vector3.Lerp(currentRotation, targetRot, Time.deltaTime * smoothSpeed);

        // --- POSITION SWAY ---
        float posX = -mouseX * positionAmount;
        float posY = -mouseY * positionAmount;

        Vector3 targetPos = new Vector3(posX, posY, 0);

        targetPos.x = Mathf.Clamp(targetPos.x, -maxPosition, maxPosition);
        targetPos.y = Mathf.Clamp(targetPos.y, -maxPosition, maxPosition);

        currentPosition = Vector3.Lerp(currentPosition, targetPos, Time.deltaTime * smoothSpeed);

        // Apply
        transform.localRotation = Quaternion.Euler(currentRotation);
        transform.localPosition = currentPosition;
    }
}