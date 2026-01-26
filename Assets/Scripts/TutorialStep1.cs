using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep1", menuName = "Scriptable Objects/TutorialStep1")]
public class TutorialStep1 : ScriptableObject
{
    [TextArea(3, 6)]
    public string tutorialText;

    public Sprite portrait;
}
