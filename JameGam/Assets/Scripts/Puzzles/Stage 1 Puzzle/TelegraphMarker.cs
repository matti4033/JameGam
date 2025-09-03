using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TelegraphMarker : MonoBehaviour
{
    public float duration = 0.45f;
    public float blinkHz = 10f;

    SpriteRenderer sr;
    float t;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        t += Time.deltaTime;
        if (sr) {
            float a = 0.25f + 0.75f * Mathf.Abs(Mathf.Sin(2f * Mathf.PI * blinkHz * t));
            var c = sr.color; c.a = a; sr.color = c;
        }
        if (t >= duration) Destroy(gameObject);
    }
}