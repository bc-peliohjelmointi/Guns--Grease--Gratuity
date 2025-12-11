using UnityEngine;

public class SmokePuff : MonoBehaviour
{
    public float lifetime = 0.4f;
    public float startScale = 0.1f;
    public float endScale = 2f;

    private float timer = 0f;
    private Material mat;
    private Color startColor;

    void Start()
    {
        transform.localScale = Vector3.one * startScale;

        mat = GetComponent<MeshRenderer>().material;
        startColor = mat.color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / lifetime;

        // Grow sphere
        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = Vector3.one * scale;

        // Fade out
        Color c = startColor;
        c.a = Mathf.Lerp(startColor.a, 0f, t);
        mat.color = c;

        // Destroy when done
        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
