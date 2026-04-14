using TMPro;
using UnityEngine;

public class EnemyBonusText : MonoBehaviour
{
    public float lifetime = 1.5f;
    public float moveUpAmount = 30f;

    private TextMeshProUGUI text;
    private Vector3 startPos;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        startPos = transform.localPosition;
    }

    public void SetText(string message)
    {
        text.text = message;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        float yOffset = Mathf.Lerp(0, moveUpAmount, t);
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);

        float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
        transform.localScale = Vector3.one * scale;

        float alpha = Mathf.Lerp(1f, 0f, t);
        
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
