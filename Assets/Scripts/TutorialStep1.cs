using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep1", menuName = "Scriptable Objects/TutorialStep1")]
public class TutorialStep1 : ScriptableObject
{
    [TextArea(3, 6)]
    public string tutorialText;

    public Sprite portrait;

    [Header("Panel Layout")]
    public Vector2 panelPosition;
    public Vector2 panelSize = new Vector2(600, 200);

    [Header("Text Box Layout")]
    public Vector2 textPosition;
    public Vector2 textSize = new Vector2(400, 150);

    [Header("Portrait Layout")]
    public Vector2 portraitPosition;
    public Vector2 portraitSize = new Vector2(128, 128);

    [Header("Anchors")]
    public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
    public Vector2 anchorMax = new Vector2(0.5f, 0.5f);

    [Header("Skip text")]
    public Vector2 skipPosition;
}
