using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Respawn : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController firstPersonController;
    public PlayerHealth playerHealth; // NEW: Reference to PlayerHealth component
    public DaySystem daySystem;

    [Header("UI Panels")]
    public GameObject gameOverScreen;

    [Header("Game Over Screen Elements")]
    public TMP_Text gameOverText;
    public Button tryAgainButton;
    public Button mainMenuButton;

    private void Start()
    {
        // Auto-find PlayerHealth if not assigned
        if (playerHealth == null && firstPersonController != null)
        {
            playerHealth = firstPersonController.GetComponent<PlayerHealth>();
        }

        // Make sure game over screen is hidden at start
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    public void TryAgain()
    {
        // Hide game over screen
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset player health FIRST (before day restart)
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }

        // Restart the day
        if (daySystem != null)
            daySystem.RestartDay();

        // Re-enable player movement
        if (firstPersonController != null)
        {
            firstPersonController.canMove = true;
        }

        Debug.Log("Try again - Player respawned with full health");
    }

    public void MainMenu()
    {
        // Load main menu scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        Debug.Log("Main menu button pressed");
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1f);

        // Show game over screen
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Over screen shown");
    }
}