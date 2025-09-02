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

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();
        Hover();
    }

    protected override void OnEnterState(BossState newState)
    {
        switch (newState)
        {
            case BossState.Attack:
                Debug.Log($"{bossName} prepares attack!");
                break;
            case BossState.Evade:
                DoEvade();
                Debug.Log($"{bossName} evades!");
                break;
            case BossState.Move:
                Debug.Log($"{bossName} repositions!");
                break;
        }
    }

    protected override void HandleState()
    {
        switch (currentState)
        {
            case BossState.Attack:
                Attack();
                break;

            case BossState.Evade:
                break;

            case BossState.Move:
                Move();
                break;
        }
    }

    void Attack()
    {

    }

    void DoEvade()
    {
        if (!isEvading)
            StartCoroutine(EvadeRoutine());
    }

    void Move()
    {
        
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
