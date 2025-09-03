using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FallingNote : MonoBehaviour
{
    public float speed = 9f;
    public float killY = -6f;
    public string playerTag = "Player";

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnEnable() { rb.linearVelocity = Vector2.down * speed; }
    void Update()
    {
        // Keep constant downward velocity
        rb.linearVelocity = Vector2.down * speed;
        if (transform.position.y < killY) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Hook your damage/death logic here
            // other.GetComponent<PlayerHealth>()?.Kill();
            Debug.Log("Player hit by note!");
        }
    }
}