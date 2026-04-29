using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Respawn : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController firstPersonController;
    public PlayerHealth playerHealth;
    public DaySystem daySystem;

    [Header("Pause Menu (to disable ESC during Game Over)")]
    public PauseMenu pauseMenu;

    [Header("UI Panels")]
    public GameObject gameOverPanel;

    [Header("Game Over Screen Elements")]
    public TMP_Text gameOverTitleText;
    public TMP_Text gameOverSubtitleText;  
    public Button tryAgainButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public float gameOverDelay = 1.2f;
    public string mainMenuSceneName = "MainMenu";

    // ── Unity Lifecycle ────────────────────────────────────────────────────
    private void Start()
    {
        // Auto-find PlayerHealth if not assigned
        if (playerHealth == null && firstPersonController != null)
            playerHealth = firstPersonController.GetComponent<PlayerHealth>();

        // Auto-find PauseMenu if not assigned
        if (pauseMenu == null)
            pauseMenu = Object.FindAnyObjectByType<PauseMenu>();

        // Hide game over panel at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Wire buttons
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(TryAgain);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void GameOver(string reason = "Better luck next time.")
    {
        StartCoroutine(GameOverRoutine(reason));
    }

    public void TryAgain()
    {
        // Re-enable pausing
        if (pauseMenu != null)
            pauseMenu.pausingAllowed = true;

        // Hide panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Restore time (in case it was frozen)
        Time.timeScale = 1f;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset player health
        if (playerHealth != null)
            playerHealth.ResetHealth();

        // Restart the day
        if (daySystem != null)
            daySystem.RestartDay();

        // Re-enable movement
        if (firstPersonController != null)
        {
            firstPersonController.canMove = true;
            var input = firstPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null) input.cursorInputForLook = true;
        }

        Debug.Log("[Respawn] Player respawned with full health.");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ── Coroutine ──────────────────────────────────────────────────────────

    private IEnumerator GameOverRoutine(string reason)
    {
        // Disable pausing so ESC doesn't open the pause menu over the Game Over screen
        if (pauseMenu != null)
            pauseMenu.pausingAllowed = false;

        // If paused somehow, force-resume first 
        if (PauseMenu.IsPaused && pauseMenu != null)
            pauseMenu.Resume();

        // Disable player control immediately
        if (firstPersonController != null)
        {
            firstPersonController.canMove = false;
            var input = firstPersonController.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null) input.cursorInputForLook = false;
        }

        // Short dramatic pause before showing the screen
        yield return new WaitForSeconds(gameOverDelay);

        // Set text
        if (gameOverTitleText != null)
            gameOverTitleText.text = "GAME OVER";

        if (gameOverSubtitleText != null)
            gameOverSubtitleText.text = reason;

        // Show panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[Respawn] Game Over — {reason}");
    }
}