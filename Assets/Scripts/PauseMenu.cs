using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles the pause menu. Assign this to a persistent GameObject (e.g. "GameManager").
/// Hook up the UI panel and buttons in the Inspector.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController firstPersonController;

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject controlsPanel;

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button controlsButton;
    public Button mainMenuButton;

    [Header("Controls Panel Buttons")]
    public Button controlsBackButton;

    [Header("Canvas (for sort order fix)")]
    public Canvas pauseCanvas;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public int pauseCanvasSortOrder = 10;

    // ── State ──────────────────────────────────────────────────────────────
    public static bool IsPaused { get; private set; } = false;
    public bool pausingAllowed = true;

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Start()
    {
        // ── Debug: validate all references up front ──
        Debug.Log($"[PauseMenu] Start(). pauseMenuPanel={(pauseMenuPanel == null ? "NULL ❌" : pauseMenuPanel.name + " ✓")}");
        Debug.Log($"[PauseMenu] pauseCanvas={(pauseCanvas == null ? "NULL (optional)" : pauseCanvas.name + " ✓")}");
        Debug.Log($"[PauseMenu] firstPersonController={(firstPersonController == null ? "NULL ❌" : "assigned ✓")}");

        // Sort order fix
        if (pauseCanvas != null)
        {
            pauseCanvas.overrideSorting = true;
            pauseCanvas.sortingOrder = pauseCanvasSortOrder;
            Debug.Log($"[PauseMenu] Canvas sort order set to {pauseCanvasSortOrder}.");
        }
        else
        {
            Debug.LogWarning("[PauseMenu] pauseCanvas not assigned — panel may render behind HUD. Drag your Canvas into the Pause Canvas field.");
        }

        // Hide panels at start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Wire buttons
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (controlsButton != null) controlsButton.onClick.AddListener(ShowControls);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (controlsBackButton != null) controlsBackButton.onClick.AddListener(HideControls);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"[PauseMenu] ESC pressed — pausingAllowed={pausingAllowed}, IsPaused={IsPaused}");

            if (!pausingAllowed)
            {
                Debug.LogWarning("[PauseMenu] Pause blocked — pausingAllowed is false (Game Over screen may be active).");
                return;
            }

            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void Pause()
    {
        if (!pausingAllowed) return;

        IsPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (firstPersonController != null)
        {
            firstPersonController.canMove = false;
            var input = firstPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null) input.cursorInputForLook = false;
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);

            // activeInHierarchy will be false if a PARENT GameObject is also inactive
            Debug.Log($"[PauseMenu] SetActive(true) called on '{pauseMenuPanel.name}'. " +
                      $"activeInHierarchy={pauseMenuPanel.activeInHierarchy} " +
                      $"(false = a parent object is disabled — check the hierarchy)");
        }
        else
        {
            Debug.LogError("[PauseMenu] pauseMenuPanel is NULL — drag your panel GameObject into the 'Pause Menu Panel' field in the Inspector!");
        }
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (firstPersonController != null)
        {
            firstPersonController.canMove = true;
            var input = firstPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null) input.cursorInputForLook = true;
        }

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        Debug.Log("[PauseMenu] Resumed.");
    }

    public void ShowControls()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}