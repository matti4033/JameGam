using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossTwo : BaseBoss
{
    [Header("Strings")]
    public GameObject stringPrefab;
    public Transform[] shootPoints;
    public int maxStrings = 5;
    public float shootCooldown = 0.3f;

    [Header("Vibrations")]
    public float vibrationInterval = 1.5f;
    public float phaseDuration = 15f;
    public float stringExtendDelay = 0.5f;

    private List<StringProjectile> activeStrings = new List<StringProjectile>();
    private Coroutine phaseRoutine;
    private int shootIndex = 0;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartBossPhases();
    }

    protected override void StartBossPhases()
    {
        if (phaseRoutine != null) StopCoroutine(phaseRoutine);
        phaseRoutine = StartCoroutine(PhaseLoop());
    }

    private IEnumerator PhaseLoop()
    {
        while (!IsDead && !IsTired)
        {
            int stringCount = 0;
            while (stringCount < maxStrings && !IsDead)
            {
                ShootString();
                stringCount++;
                yield return new WaitForSeconds(shootCooldown);
            }

            yield return new WaitForSeconds(stringExtendDelay);

            float timer = 0f;
            while (timer < phaseDuration && !IsDead)
            {
                PlayRandomString();
                yield return new WaitForSeconds(vibrationInterval);
                timer += vibrationInterval;
            }

            foreach (var s in activeStrings) if (s != null) Destroy(s.gameObject);
            activeStrings.Clear();
            shootIndex = 0;

            FinishPhase();
        }
    }

    private void ShootString()
    {
        if (shootPoints.Length == 0) return;

        Transform firePoint = shootPoints[shootIndex];
        GameObject s = Instantiate(stringPrefab, firePoint.position, Quaternion.identity);
        StringProjectile sp = s.GetComponent<StringProjectile>();
        sp.Init(firePoint);
        activeStrings.Add(sp);

        shootIndex = (shootIndex + 1) % shootPoints.Length;
    }

    private void PlayRandomString()
    {
        var stuckStrings = activeStrings.FindAll(s => s != null && s.IsStuck());
        if (stuckStrings.Count == 0) return;

        int index = Random.Range(0, stuckStrings.Count);
        stuckStrings[index].PlayString();
    }

    protected override void Defeated()
    {
        foreach (var s in activeStrings)
        {
            if (s != null) Destroy(s.gameObject);
        }
        activeStrings.Clear();

        base.Defeated();
    }
}

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class BossTwo : BaseBoss
//{
//    [Header("Strings")]
//    public GameObject stringPrefab;
//    public Transform[] shootPoints;
//    private int shootIndex = 0;
//    public int maxStrings = 5;
//    public float shootCooldown = 0.3f;

//    [Header("Vibrations")]
//    public GameObject vibrationPrefab;
//    public float vibrationTravelTime = 1f;
//    public float vibrationInterval = 1.5f;

//    [Header("Phase Settings")]
//    public float phaseDuration = 15f;
//    public float stringExtendDelay = 0.5f;
//    public int maxPhaseCycles = 2;
//    public int maxAttackCycles = 3;

//    private List<StringProjectile> activeStrings = new List<StringProjectile>();

//    private int phaseCount = 0;
//    private int attackCycleCount = 0;
//    private bool isTired = false;
//    private Coroutine phaseRoutine;
//    public bool IsTired => isTired;

//    protected override void Start()
//    {
//        base.Start();
//        player = GameObject.FindGameObjectWithTag("Player").transform;
//        StartPhases();
//    }

//    public void StartPhases()
//    {
//        if (phaseRoutine != null) StopCoroutine(phaseRoutine);
//        phaseRoutine = StartCoroutine(PhaseLoop());
//    }

//    private IEnumerator PhaseLoop()
//    {
//        phaseCount = 0;
//        isTired = false;

//        while (!isDead && !isTired)
//        {
//            int stringCount = 0;
//            while (stringCount < maxStrings && !isDead)
//            {
//                ShootString();
//                stringCount++;
//                yield return new WaitForSeconds(shootCooldown);
//            }

//            yield return new WaitForSeconds(stringExtendDelay);

//            float waitTimer = 0f;
//            float maxWait = 3f;
//            while (activeStrings.Exists(s => s != null && !s.IsStuck()) && waitTimer < maxWait)
//            {
//                waitTimer += Time.deltaTime;
//                yield return null;
//            }

//            float phaseTimer = 0f;
//            while (phaseTimer < phaseDuration && !isDead)
//            {
//                PlayRandomString();
//                yield return new WaitForSeconds(vibrationInterval);
//                phaseTimer += vibrationInterval;
//            }

//            foreach (var s in activeStrings)
//            {
//                if (s != null) Destroy(s.gameObject);
//            }
//            activeStrings.Clear();
//            shootIndex = 0;

//            phaseCount++;
//            if (phaseCount >= maxPhaseCycles)
//            {
//                isTired = true;
//                attackCycleCount++;
//                Debug.Log("Boss is tired! Waiting for player action...");

//                //behöver nån tmp eller ngt som säger till spelaren
//            }
//        }
//    }

//    private void ShootString()
//    {
//        if (shootPoints.Length == 0) return;

//        Transform firePoint = shootPoints[shootIndex];
//        GameObject s = Instantiate(stringPrefab, firePoint.position, Quaternion.identity);
//        StringProjectile sp = s.GetComponent<StringProjectile>();
//        sp.Init(firePoint);
//        activeStrings.Add(sp);

//        shootIndex = (shootIndex + 1) % shootPoints.Length;
//    }

//    private void PlayRandomString()
//    {
//        var stuckStrings = activeStrings.FindAll(s => s != null && s.IsStuck());
//        if (stuckStrings.Count == 0) return;

//        int index = Random.Range(0, stuckStrings.Count);
//        stuckStrings[index].PlayString();
//    }

//    public void WakeUp()
//    {
//        if (isTired && !isDead)
//        {
//            if (attackCycleCount >= maxAttackCycles)
//            {
//                Defeated();
//            }
//            else
//            {
//                Debug.Log("Boss wakes up!");
//                StartPhases();
//            }
//        }
//    }

//    protected override void Defeated()
//    {
//        foreach (var s in activeStrings)
//        {
//            if (s != null) Destroy(s.gameObject);
//        }
//        activeStrings.Clear();

//        base.Defeated();
//    }
//}