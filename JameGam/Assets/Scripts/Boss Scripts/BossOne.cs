using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System.Collections;

public class BossOne : BaseBoss
{
    [Header("Attack")]
    public GameObject[] projectilePrefabs;
    public Transform shootPoint;
    public float attackCD;
    public float attackPhaseDuration;

    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite tired;

    private SpriteRenderer sr;

    public override float PhaseDuration => attackPhaseDuration;


    public float attackTimer;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sr = GetComponent<SpriteRenderer>();

        maxPhaseCycles = 1;

        StartBossPhases();
    }
    protected override void StartBossPhases()
    {
        StartCoroutine(PhaseLoop());        
    }

    private IEnumerator PhaseLoop()
    {
        float phaseTimer = 0f;

        while (!IsDead)
        {
            if (isTired)
            {
                StartCoroutine(TiredRoutine());
                yield return null;
                continue;
            }
            else
            {
                sr.sprite = normal;
                StopCoroutine(TiredRoutine());
            }
            attackTimer = 0f;

            while (phaseTimer < attackPhaseDuration && !IsTired && !IsDead)
            {
                if (attackTimer <= 0f)
                {
                    Attack();
                    attackTimer = attackCD;
                }

                attackTimer -= Time.deltaTime;
                phaseTimer += Time.deltaTime;

                yield return null;
            }

            FinishPhase();
            Debug.Log($"{bossName} is now tired: {IsTired}");

            phaseTimer = 0f;
        }
    }

    IEnumerator TiredRoutine()
    {
        while(isTired)
        {
            if(sr != null)
            {
                sr.sprite = (sr.sprite == normal) ? tired : normal;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void Attack()
    {
        if (projectilePrefabs.Length == 0) return;

        int index = Random.Range(0, projectilePrefabs.Length);
        GameObject selectedProjectile = projectilePrefabs[index];

        GameObject proj = Instantiate(selectedProjectile, shootPoint.position, Quaternion.identity);

        proj.transform.localScale = selectedProjectile.transform.localScale;

        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 projScale = proj.transform.localScale;
        projScale.x = Mathf.Abs(projScale.x) * direction;
        proj.transform.localScale = projScale;

    }

    protected override void Defeated()
    {
        base.Defeated();
    }
}