using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StringProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 15f;
    public float stayDuration = 15f;
    public GameObject vibrationPrefab;
    public float vibrationTravelTime = 1f;

    private Transform boss;
    private Vector2 startPos;
    private Vector2 tipPos;
    private bool stuck = false;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private PlatformEffector2D effector;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        effector = GetComponent<PlatformEffector2D>();

        if (boxCollider != null)
            boxCollider.enabled = false;
        if (effector != null)
        {
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
        }
    }

    public void Init(Transform anchorPoint)
    {
        boss = anchorPoint;
        startPos = boss.position;
        transform.position = startPos;
        tipPos = startPos;
    }

    void Update()
    {
        if (!stuck)
        {
            tipPos += Vector2.left * speed * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Raycast(tipPos - Vector2.left * speed * Time.deltaTime, Vector2.left, speed * Time.deltaTime);
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                tipPos = hit.point;
                Stick();
            }

            if (Vector2.Distance(startPos, tipPos) >= maxDistance)
            {
                tipPos = startPos + Vector2.left * maxDistance;
                Stick();
            }

            UpdateSpriteAndCollider();
        }
        effector.rotationalOffset = -transform.eulerAngles.z;
    }

    public bool IsStuck()
    {
        return stuck;
    }

    void UpdateSpriteAndCollider()
    {
        Vector2 dir = tipPos - (Vector2)startPos;
        float length = dir.magnitude;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (spriteRenderer != null)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = length / spriteRenderer.sprite.bounds.size.x;
            transform.localScale = localScale;
        }

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(length, 0.2f);
            boxCollider.offset = new Vector2(length / 2f, 0);
        }

        transform.position = startPos;
    }

    void Stick()
    {
        if (stuck) return;
        stuck = true;

        if (boxCollider != null)
            boxCollider.enabled = true;

        StartCoroutine(DestroyAfterDelay(stayDuration));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void PlayString()
    {
        if (!stuck || vibrationPrefab == null || boss == null) return;

        StartCoroutine(VibrationRoutine());
    }

    private IEnumerator VibrationRoutine()
    {
        GameObject v = Instantiate(vibrationPrefab, boss.position, Quaternion.identity);

        float t = 0f;
        while (t < 1f)
        {
            if (this == null)
            {
                if (v != null) Destroy(v);
                yield break;
            }

            t += Time.deltaTime / vibrationTravelTime;
            if (v != null)
                v.transform.position = Vector3.Lerp(boss.position, tipPos, t);

            yield return null;
        }

        if (v != null) Destroy(v);
    }
}
