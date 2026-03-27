using StarterAssets;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
public class Respawn : MonoBehaviour
{
    [Header("References")]
    public FirstPersonController firstPersonController;
    public DaySystem daySystem;

    [Header("UI Panels")]
    public GameObject gameOverScreen;

    [Header("Game Over Screen Elements")]
    public TMP_Text gameOverText;
    public Button tryAgainButton;
    public Button mainMenuButton;


    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    public void TryAgain()
    {
        gameOverScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        daySystem.RestartDay();

        if (firstPersonController != null)
        {
            firstPersonController.canMove = true;
            firstPersonController.isDead = false;
            firstPersonController.currentHealth = firstPersonController.maxHealth;

        }

        Debug.Log("Try again button pressed");
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1f);

        gameOverScreen.SetActive(true);


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
