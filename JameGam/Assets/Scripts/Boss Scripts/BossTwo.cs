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
