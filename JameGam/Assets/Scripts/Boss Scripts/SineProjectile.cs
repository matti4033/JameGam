using UnityEngine;

public class SineProjectile : MonoBehaviour
{
    public float speed;
    public float amplitude;
    public float frequency;


    private Vector2 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {

        float direction = transform.localScale.x > 0 ? -1f : 1f;
        transform.position += Vector3.right * speed * direction * Time.deltaTime;

        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector2(transform.position.x, startPos.y + yOffset);

        if (transform.position.x <= -10)
        {
            Destroy(gameObject);
        }
    }

}
