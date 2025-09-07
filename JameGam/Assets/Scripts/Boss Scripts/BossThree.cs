using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class BossThree : BaseBoss
{
    [Header("BossOne Style")]
    public GameObject[] projectilePrefabs;
    public Transform projectileShootPoint;
    public float projectileattackCD = 0.5f;
    private float projectileAttackTimer = 0f;

    [SerializeField] private Sprite normal;
    [SerializeField] private Sprite tired;

    private SpriteRenderer sr;

    [Header("BossTwo Style")]
    public GameObject stringPrefab;
    public Transform[] stringShootPoints;
    public int maxStrings = 5;
    public float stringShootCooldown = 0.3f;
    public float vibrationInterval = 1.5f;
    public float stringPhaseDuration = 10f;
    public float stringExtendDelay = 0.5f;

    public override float PhaseDuration => stringPhaseDuration;


    private List<StringProjectile> activeStrings = new List<StringProjectile>();
    private int stringShootIndex = 0;

    private Coroutine phaseRoutine;

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
        if (phaseRoutine != null)
            StopCoroutine(phaseRoutine);
        phaseRoutine = StartCoroutine(PhaseLoop());
        StartCoroutine(ProjectileLoop());
    }

    private IEnumerator PhaseLoop()
    {
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

            int stringCount = 0;
            while (stringCount < maxStrings && !IsDead)
            {
                ShootString();
                stringCount++;
                yield return new WaitForSeconds(stringShootCooldown);
            }

            yield return new WaitForSeconds(stringExtendDelay);

            float vibrationTimer = 0f;
            while (vibrationTimer < stringPhaseDuration && !IsDead)
            {
                PlayRandomString();
                yield return new WaitForSeconds(vibrationInterval);
                vibrationTimer += vibrationInterval;
            }

            foreach (var s in activeStrings) if (s != null) Destroy(s.gameObject);
            activeStrings.Clear();
            stringShootIndex = 0;

            FinishPhase();
        }
    }
    IEnumerator TiredRoutine()
    {
        while (isTired)
        {
            if (sr != null)
            {
                sr.sprite = (sr.sprite == normal) ? tired : normal;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator ProjectileLoop()
    {
        while (!IsDead)
        {
            if (IsTired)
            {
                yield return null;
                continue;
            }

            FireProjectile();
            yield return new WaitForSeconds(projectileattackCD);
        }
    }
  
    void FireProjectile()
    {
        if (projectilePrefabs.Length == 0) return;

        int index = Random.Range(0, projectilePrefabs.Length);
        GameObject selectedProjectile = projectilePrefabs[index];

        GameObject proj = Instantiate(selectedProjectile, projectileShootPoint.position, Quaternion.identity);

        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 projScale = proj.transform.localScale;
        projScale.x = Mathf.Abs(projScale.x) * direction;
        proj.transform.localScale = projScale;
    }

    void ShootString()
    {
        if (stringShootPoints.Length == 0) return;

        Transform firePoint = stringShootPoints[stringShootIndex];
        GameObject s = Instantiate(stringPrefab, firePoint.position, Quaternion.identity);
        StringProjectile sp = s.GetComponent<StringProjectile>();
        sp.Init(firePoint);
        activeStrings.Add(sp);

        stringShootIndex = (stringShootIndex + 1) % stringShootPoints.Length;
    }
    void PlayRandomString()
    {
        var stuckStrings = activeStrings.FindAll(s => s != null && s.IsStuck());
        if (stuckStrings.Count == 0) return;

        int index = Random.Range(0, stuckStrings.Count);
        stuckStrings[index].PlayString();
    }
    protected override void Defeated()
    {
        foreach (var s in activeStrings)
            if (s != null) Destroy(s.gameObject);

        activeStrings.Clear();
        base.Defeated();
    }
}
