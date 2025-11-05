using UnityEngine;
using UnityEngine.InputSystem;

public class scooterCtrl : MonoBehaviour
{
    public Transform scooterRoot; // koko skuutin runko
    public float turnSpeed = 60f;
    public bool canControl = false; // asetetaan ScooterMountista

    void Update()
    {
        if (!canControl) return; // ei tee mitään jos pelaaja ei ole mounted

        float turn = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            turn = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            turn = 1f;

        // Käännetään visuaalisesti ohjaustankoa
        transform.localRotation = Quaternion.Euler(0, turn * 20f, 0);

        // Käännetään koko scootteri hitaammin
        scooterRoot.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
    }
}
