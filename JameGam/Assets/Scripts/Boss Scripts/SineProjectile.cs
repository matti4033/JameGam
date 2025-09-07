using UnityEngine;
using UnityEngine.EventSystems;

public class SineProjectile : MonoBehaviour
{
    public float speed;
    public float amplitude;
    public float frequency;

    private int bounces;
    public int Maxbounces;

    private Vector2 startPos;
    private Vector3 moveDirection;

    private void Start()
    {
        startPos = transform.position;

        float randomX = Random.Range(-1f, 1.0f);
        float randomY = Random.Range(-1f, 1.0f);

        moveDirection = new Vector2(randomX, randomY).normalized;

        float direction = transform.localScale.x > 0 ? -1f : 1f;
        moveDirection.x *= direction;
    }

    private void Update()
    {
       transform.position += moveDirection * speed * Time.deltaTime;


        if (bounces >= Maxbounces)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Floor"))
        {
            Vector2 normal = collision.contacts[0].normal;

            moveDirection = Vector2.Reflect(moveDirection, normal).normalized;

            bounces++;
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("1");
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("2");
                var hp = collision.gameObject.GetComponent<PlayerHealth>();
                if (hp != null) hp.Damage(1);
            }
        }
    }

}
