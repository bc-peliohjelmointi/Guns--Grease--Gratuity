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

    public static TutorialManager Instance;

    private int currentStepIndex = 0;

    public float typingSpeed = 0.03f;

    bool skipTyping = false;



    void Start()
    {
        tutorialPanel.SetActive(false);
        StartTutorial();
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {


            // Lock some steps to force an action
            if (steps[currentStepIndex].isSkippable)
            {

                NextStep();
            }
           
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


        if (step.isSkippable)
        {
            skipText.text = "Press Q to skip";
        }
        else
        {
            skipText.text = "Complete the action to continue";
        }

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

    // Hieno kirjain kerrallaan kirjoitus
    IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        bool isInsideTag = false;

        foreach (char c in text)
        {
            if (c == '<')
            {
                isInsideTag = true;
            }

            dialogueText.text += c;

            if (c == '>')
            {
                isInsideTag = false;
                continue; // don't delay after finishing a tag
            }

            if (!isInsideTag)
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void NotifyTrigger(TutorialTriggerType trigger)
    {
        if (currentStepIndex >= steps.Length) return;

        if (steps[currentStepIndex].triggerType == trigger)
        {
            NextStep();
        }
    }

    public void StartTutorial()
    {
        currentStepIndex = 0;
        ShowStep(0);
    }
}

