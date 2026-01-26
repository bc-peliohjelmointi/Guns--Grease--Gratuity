using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;


public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Image portraitImage;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI skipText;


    public TutorialStep1[] steps;

    private int currentStepIndex = 0;

    public float typingSpeed = 0.03f;


    void Start()
    {
        ShowStep(0);
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextStep();
        }
    }

    void ShowStep(int index)
    {
        if (index >= steps.Length)
        {
            tutorialPanel.SetActive(false);
            return;
        }

        tutorialPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TypeText(steps[index].tutorialText));
        portraitImage.sprite = steps[index].portrait;
    }

    void NextStep()
    {
        currentStepIndex++;
        ShowStep(currentStepIndex);
    }

    IEnumerator TypeText(string text)
    {
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}

