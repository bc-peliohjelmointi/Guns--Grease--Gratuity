using UnityEngine;
using System.Collections;

public class SmokePuff : MonoBehaviour
{
    public float lifetime = 0.4f;
    public float startScale = 0.1f;
    public float endScale = 2f;

    public AudioSource audioSource;
    public AudioClip deathSound;

    private float timer = 0f;
    private Material mat;
    private Color startColor;

    void Start()
    {
        transform.localScale = Vector3.one * startScale;

        mat = GetComponent<MeshRenderer>().material;
        startColor = mat.color;
    }

    private void Awake()
    {
        audioSource.PlayOneShot(deathSound);
    }

    private IEnumerator soundAndDestroy()   // Waits for the sound to finish before destroying the gameobject
    {
        GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
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
            StartCoroutine(soundAndDestroy());
    }
}
