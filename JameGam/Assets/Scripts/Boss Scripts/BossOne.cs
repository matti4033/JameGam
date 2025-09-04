using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Collections;

public class BossOne : BaseBoss
{
    [Header("Hover Settings")]
    public float hoverAmplitude = 1f;
    public float hoverSpeed = 2f;
    public float hoverDistance = 3f;
    public float hoverBaseY = 2f;

    [Header("Evade")]
    public float evadeDistance = 3f;
    public float evadeDuration = 0.3f;
    private bool isEvading = false;

    [Header("Attack")]
    public GameObject[] projectilePrefabs;
    public Transform shootPoint;
    public float attackCD;
    private float attackTimer;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();
        Hover();

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    void Attack()
    {
        if (attackTimer > 0) return;

        if (projectilePrefabs.Length == 0) return;

        int index = Random.Range(0, projectilePrefabs.Length);
        GameObject selectedProjectile = projectilePrefabs[index];

        GameObject proj = Instantiate(selectedProjectile, shootPoint.position, Quaternion.identity);

        proj.transform.localScale = selectedProjectile.transform.localScale;

        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 projScale = proj.transform.localScale;
        projScale.x = Mathf.Abs(projScale.x) * direction;
        proj.transform.localScale = projScale;

        attackTimer = attackCD;
    }

    void DoEvade()
    {
        if (!isEvading)
            StartCoroutine(EvadeRoutine());
    }

    void Hover()
    {
        Vector2 target = transform.position;

        Vector2 dir = (transform.position - player.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        target.x = player.position.x + dir.x * hoverDistance;

        target.y = hoverBaseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;

        if (!isEvading)
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        else
        {
            Vector2 pos = transform.position;
            pos.y = target.y;
            transform.position = pos;
        }
    }

    IEnumerator EvadeRoutine()
    {
        isEvading = true;

        Vector2 dir = (transform.position - player.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        Vector2 start = transform.position;
        Vector2 target = start + dir * evadeDistance;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / evadeDuration;
            rb.MovePosition(Vector2.Lerp(start, target, t));
            yield return null;
        }

        isEvading = false;
    }
}
