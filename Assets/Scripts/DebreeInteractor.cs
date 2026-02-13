using UnityEngine;
using UnityEngine.InputSystem;

public class DebreeInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera playerCamera;
    public float interactDistance = 3f;

    [Header("Hold Settings")]
    public float holdTime = 1.5f;

    [Header("UI")]
    public GameObject interactPrompt;
    public UnityEngine.UI.Slider progressBar;

    private GameObject currentDebree;
    private float holdTimer;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance) &&
            hit.collider.CompareTag("Debree"))
        {
            currentDebree = hit.collider.gameObject;

            if (interactPrompt != null)
                interactPrompt.SetActive(true);

            HandleHoldInput();
        }
        else
        {
            ClearTarget();
        }
    }

    void HandleHoldInput()
    {
        if (Keyboard.current.eKey.isPressed)
        {
            holdTimer += Time.deltaTime;

            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.value = holdTimer / holdTime;
            }

            if (holdTimer >= holdTime)
            {
                Destroy(currentDebree);
                ClearTarget();
            }
        }
        else
        {
            holdTimer = 0f;

            if (progressBar != null)
            {
                progressBar.value = 0f;
                progressBar.gameObject.SetActive(false);
            }
        }
    }

    void ClearTarget()
    {
        currentDebree = null;
        holdTimer = 0f;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(false);
        }
    }
}
