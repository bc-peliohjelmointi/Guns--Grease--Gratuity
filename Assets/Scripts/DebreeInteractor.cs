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
    private DebreeProgress currentProgress;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance) &&
            hit.collider.CompareTag("Debree"))
        {
            SetTarget(hit.collider.gameObject);
            HandleHoldInput();
        }
        else
        {
            ClearTarget();
        }
    }

    void SetTarget(GameObject debree)
    {
        if (currentDebree == debree)
            return;

        currentDebree = debree;
        currentProgress = debree.GetComponent<DebreeProgress>();

        if (interactPrompt != null)
            interactPrompt.SetActive(true);

        if (progressBar != null && currentProgress != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = currentProgress.progress;
        }
    }

    void HandleHoldInput()
    {
        if (currentProgress == null)
            return;

        if (Keyboard.current.eKey.isPressed)
        {
            currentProgress.progress += Time.deltaTime / holdTime;
            currentProgress.progress = Mathf.Clamp01(currentProgress.progress);

            if (progressBar != null)
                progressBar.value = currentProgress.progress;

            if (currentProgress.progress >= 1f)
            {
                Destroy(currentDebree);
                ClearTarget();
            }
        }
    }

    void ClearTarget()
    {
        currentDebree = null;
        currentProgress = null;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(false);
        }
    }
}