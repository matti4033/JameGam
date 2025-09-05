using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class WaveSegment : MonoBehaviour
{
    public float speed = 8f;
    public float length = 3f; // collider width along X
    public float killX = -12f;
    public string playerTag = "Player";

    Rigidbody2D rb;
    BoxCollider2D box;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.bodyType = RigidbodyType2D.Kinematic; // Move it manually for consistency

        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void OnEnable()
    {
        // Size collider to desired length
        if (box != null)
        {
            var s = box.size;
            s.x = Mathf.Max(0.1f, length);
            box.size = s;
            var off = box.offset;
            off.x = -s.x * 0.5f; // so the front edge is at the object's origin
            box.offset = off;
        }
    }

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if (transform.position.x < killX) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Hook your damage/respawn here:
            // other.GetComponent<PlayerHealth>()?.Kill();
            Debug.Log("Player hit by wave!");
        }
    }
}