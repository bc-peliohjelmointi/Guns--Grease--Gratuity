using UnityEngine;

public class GunSlide : MonoBehaviour
{
    public float slideDistance = 0.05f;
    public float slideSpeed = 20f;
    public float returnSpeed = 10f;

    Vector3 originalPos;
    float currentOffset;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        // Return to original position
        currentOffset = Mathf.Lerp(currentOffset, 0, Time.deltaTime * returnSpeed);

        transform.localPosition = originalPos + new Vector3(0, 0, +currentOffset);
    }

    public void Kick()
    {
        currentOffset = slideDistance;
    }
}