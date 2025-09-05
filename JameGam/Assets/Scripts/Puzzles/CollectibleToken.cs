using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleToken : MonoBehaviour
{
    public enum MoveMode
    {
        None,
        Fall,
        Slide
    }

    [Header("Collect")] public int value = 1;
    public float lifeTime = 10f;
    public string playerTag = "Player";

    [Header("Movement (auto-set by directors)")]
    public MoveMode moveMode = MoveMode.None;

    public float moveSpeed = 5f;
    public float killY = -9999f; // for Fall
    public float killX = -9999f; // for Slide

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnEnable()
    {
        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        switch (moveMode)
        {
            case MoveMode.Fall:
                transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                if (transform.position.y <= killY) Destroy(gameObject);
                break;
            case MoveMode.Slide:
                transform.position += Vector3.left * moveSpeed * Time.deltaTime;
                if (transform.position.x <= killX) Destroy(gameObject);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        FindAnyObjectByType<TokenGoal>()?.Add(value);
        Destroy(gameObject);
    }
}