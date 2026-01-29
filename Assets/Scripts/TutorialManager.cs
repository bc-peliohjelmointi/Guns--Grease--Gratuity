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

    public RectTransform tutorialPanelRect;
    public RectTransform portraitRect;
    public RectTransform textRect;
    public RectTransform skipRect;


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

        TutorialStep1 step = steps[index];

        // Layouts

        // Panel
        tutorialPanelRect.anchorMin = step.anchorMin;
        tutorialPanelRect.anchorMax = step.anchorMax;
        tutorialPanelRect.anchoredPosition = step.panelPosition;
        tutorialPanelRect.sizeDelta = step.panelSize;

        // Portrait
        portraitRect.anchoredPosition = step.portraitPosition;
        portraitRect.sizeDelta = step.portraitSize;

        // Text box
        textRect.anchoredPosition = step.textPosition;
        textRect.sizeDelta = step.textSize;

        // Skip text
        skipRect.anchoredPosition = step.skipPosition;

        StopAllCoroutines();
        StartCoroutine(TypeText(step.tutorialText));
        portraitImage.sprite = step.portrait;
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

