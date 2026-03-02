using UnityEngine;
using TMPro; // for TextMeshPro

public class GunUI : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public GameObject crosshair;

    public void UpdateAmmo(int current, int total)
    {
        if (ammoText)
            ammoText.text = $"{current} / {total}";
    }
}
