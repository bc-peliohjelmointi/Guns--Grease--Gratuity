using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyBonusText : MonoBehaviour
{
    public float lifetime = 1.5f;
    public float typingSpeed = 0.03f;

    [Header("Wave Settings")]
    public float waveAmp = 5f; // how far wave moves
    public float waveSpeed = 5f; // how fast the wave is

    private TextMeshProUGUI text;
    private string fullText;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string message)
    {
        text.text = "";
        fullText = message;
        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        bool isInsideTag = false;

        foreach (char c in fullText)
        {
            if (c == '<') isInsideTag = true;

            text.text += c;

            if (c == '>')
            {
                isInsideTag = false;
                continue;
            }

            if (!isInsideTag)
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // wave motion
        float xOffset = Mathf.Sin(timer * waveSpeed) * waveAmp;
        transform.localPosition = new Vector3(xOffset, transform.localPosition.y, 0f);

        float scale = 1f + Mathf.Sin(timer * 8f) * 0.05f;
        transform.localScale = Vector3.one * scale;

        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
