using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController firstPersonController;

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button mainMenuButton;
    public Button settingsButton;
    

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";

    // ── State ──────────────────────────────────────────────────────────────
    public static bool IsPaused { get; private set; } = false;

    // Allow external systems to disable pausing temporarily
    public bool pausingAllowed = true;

    private void Start()
    {
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void Update()
    {
        if (!pausingAllowed) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement & look
        if (firstPersonController != null)
        {
            firstPersonController.canMove = false;
            
            var input = firstPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null) input.cursorInputForLook = false;
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Debug.Log("[PauseMenu] Game paused.");
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player
        if (firstPersonController != null)
        {
            firstPersonController.canMove = true;
            var input = firstPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null) input.cursorInputForLook = true;
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Debug.Log("[PauseMenu] Game resumed.");
    }

    public void GoToMainMenu()
    {
        // Always reset timescale before loading a new scene
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}