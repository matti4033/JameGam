using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossTwo : BaseBoss
{
    [Header("Strings")]
    public GameObject stringPrefab;
    public Transform[] shootPoints;
    private int shootIndex = 0;
    public int maxStrings = 5;
    public float shootCooldown = 0.3f;

    [Header("Vibrations")]
    public GameObject vibrationPrefab;
    public float vibrationTravelTime = 1f;
    public float vibrationInterval = 1.5f;

    [Header("Phase Settings")]
    public float phaseDuration = 15f;
    public float stringExtendDelay = 0.5f;

    private List<StringProjectile> activeStrings = new List<StringProjectile>();

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(PhaseLoop());
    }

    private IEnumerator PhaseLoop()
    {
        while (!isDead)
        {
            int stringCount = 0;
            while (stringCount < maxStrings && !isDead)
            {
                ShootString();
                stringCount++;
                yield return new WaitForSeconds(shootCooldown);
            }

            yield return new WaitForSeconds(stringExtendDelay);

            float waitTimer = 0f;
            float maxWait = 3f;
            while (activeStrings.Exists(s => s != null && !s.IsStuck()) && waitTimer < maxWait)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            float phaseTimer = 0f;
            while (phaseTimer < phaseDuration && !isDead)
            {
                PlayRandomString();
                yield return new WaitForSeconds(vibrationInterval);
                phaseTimer += vibrationInterval;
            }

            foreach (var s in activeStrings)
            {
                if (s != null) Destroy(s.gameObject);
            }
            activeStrings.Clear();
            shootIndex = 0;
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

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
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
