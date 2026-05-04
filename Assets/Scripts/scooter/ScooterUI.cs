using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ScooterUI : MonoBehaviour
{
    [Header("References")]
    public scooterCtrl scooter;

    [Header("Speedometer")]
    public RectTransform speedArrow;
    public float minRotation = 120f;
    public float maxRotation = -120f;
    public float maxDisplaySpeed = 35f;

    [Header("UI")]
    public GameObject scooterUI;
    public TextMeshProUGUI speedText;
    public Slider batterySlider;

    private void Update()
    {
        if (scooter == null) return;

        bool showUI = scooter.canControl;

        if (scooterUI != null)
            scooterUI.gameObject.SetActive(showUI);

        if (!showUI) return;

        UpdateSpeedometer();
        UpdateSpeedText();
        UpdateBattery();
    }

    private void UpdateSpeedometer()
    {
        float speed = Mathf.Abs(scooter.currentSpeed);

        // Clamp to display max
        float clampedSpeed = Mathf.Clamp(speed, 0f, maxDisplaySpeed);

        float t = clampedSpeed / maxDisplaySpeed;

        float rotationZ = Mathf.Lerp(minRotation, maxRotation, t);

        Quaternion targetRot = Quaternion.Euler(0f, 0f, rotationZ);
        speedArrow.localRotation = Quaternion.Lerp(
            speedArrow.localRotation,
            targetRot,
            Time.deltaTime * 8f
        );
    }

    private void UpdateSpeedText()
    {
        if (speedText == null) return;

        float speed = Mathf.Abs(scooter.currentSpeed);

        speedText.text = Mathf.RoundToInt(speed).ToString();
    }

    private void UpdateBattery()
    {
        if (batterySlider == null) return;

        batterySlider.value = scooter.currentBattery / scooter.maxBattery;
    }
}