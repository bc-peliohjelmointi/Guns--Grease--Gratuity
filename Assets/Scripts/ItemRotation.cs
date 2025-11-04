using UnityEngine;

public class ItemRotation : MonoBehaviour
{
    public float rotationSpeed = 50f; // degrees per second

    void Update()
    {
        // Rotate the item around the Y axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
