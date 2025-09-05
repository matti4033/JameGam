using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StringWaveDirector : MonoBehaviour
{
    [Header("Lanes / Area")] public int lanes = 6;
    public float topY = 3.5f;
    public float bottomY = -3.5f;
    public float spawnX = 10f;
    public float killX = -10f;

    [Header("Prefabs")] public GameObject wavePrefab;
    public GameObject telegraphPrefab;

    [Header("Timing (music-like)")] public float bpm = 110f;
    public int stepsPerBeat = 2;
    public float telegraphLead = 0.40f;

    [Header("Waves")] public float waveSpeed = 8.5f;
    public float waveLength = 3.0f;
    public float waveLifePadding = 1.0f;

    [Header("Path Constraints")] public int maxLaneChangePerStep = 1;
    [Range(0f, 1f)] public float wander = 0.6f;

    [Header("Start/Loop")] public bool playOnStart = true;
    public bool endless = true;
    public int stepsToRun = 64;

    [Header("Tokens")] public GameObject tokenPrefab;
    public int totalTokens = 4;
    public int firstTokenStep = 4;
    public int lastTokenStep = 28;
    public Vector2 tokenOffset = Vector2.zero;

    HashSet<int> _tokenSteps;
    int _tokensSpawned;
    int _steps;

    float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);
    int currentSafeLane;
    System.Random rng;

    void Start()
    {
        rng = new System.Random();
        currentSafeLane = Mathf.Clamp(lanes / 2, 0, Mathf.Max(0, lanes - 1));

        _tokenSteps = new HashSet<int>();
        int lo = Mathf.Max(0, firstTokenStep);
        int hi = Mathf.Max(lo + 1, lastTokenStep);
        while (_tokenSteps.Count < totalTokens)
            _tokenSteps.Add(UnityEngine.Random.Range(lo, hi));

        if (playOnStart) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        int steps = 0;
        while (endless || steps < stepsToRun)
        {
            int nextSafe = PickNextSafeLane();

            TelegraphRow(nextSafe);
            yield return new WaitForSeconds(telegraphLead);

            SpawnRow(nextSafe);

            if (tokenPrefab && _tokenSteps.Contains(_steps) && _tokensSpawned < totalTokens)
            {
                Vector3 p = new Vector3(spawnX, LaneY(nextSafe), 0f) + (Vector3)tokenOffset;
                var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                var ct = tok.GetComponent<CollectibleToken>();
                if (ct == null) ct = tok.AddComponent<CollectibleToken>();
                ct.moveMode = CollectibleToken.MoveMode.Slide;
                ct.moveSpeed = waveSpeed;
                ct.killX = killX - waveLifePadding;
                _tokensSpawned++;
            }

            _steps++;

            yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
            currentSafeLane = nextSafe;
            steps++;
        }
    }

    int PickNextSafeLane()
    {
        List<int> candidates = new List<int>();
        for (int d = -maxLaneChangePerStep; d <= maxLaneChangePerStep; d++)
        {
            int lane = Mathf.Clamp(currentSafeLane + d, 0, lanes - 1);
            if (!candidates.Contains(lane)) candidates.Add(lane);
        }

        int stay = currentSafeLane;
        if (rng.NextDouble() < wander)
        {
            candidates.Remove(stay);
            if (candidates.Count > 0) return candidates[rng.Next(candidates.Count)];
        }

        return stay;
    }

    void TelegraphRow(int safeLane)
    {
        if (!telegraphPrefab) return;
        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == safeLane) continue;
            var pos = new Vector3(spawnX, LaneY(lane), 0f);
            var t = Instantiate(telegraphPrefab, pos, Quaternion.identity);
            var tm = t.GetComponent<TelegraphMarker>();
            if (tm)
            {
                tm.duration = telegraphLead;
                tm.width = waveLength;
            }
        }
    }

    void SpawnRow(int safeLane)
    {
        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == safeLane) continue;
            var pos = new Vector3(spawnX, LaneY(lane), 0f);
            var go = Instantiate(wavePrefab, pos, Quaternion.identity);
            var seg = go.GetComponent<WaveSegment>();
            if (seg)
            {
                seg.speed = waveSpeed;
                seg.length = waveLength;
                seg.killX = killX - waveLifePadding;
            }
        }
    }

    float LaneY(int lane)
    {
        if (lanes <= 0) return 0f;
        float t = (lanes == 1) ? 0.5f : (lane + 0.5f) / lanes;
        return Mathf.Lerp(topY, bottomY, t);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, 0.25f);
        for (int i = 0; i < Mathf.Max(1, lanes); i++)
        {
            float y = (lanes == 1) ? (topY + bottomY) / 2f : Mathf.Lerp(topY, bottomY, (i + 0.5f) / lanes);
            Gizmos.DrawLine(new Vector3(-20f, y, 0), new Vector3(20f, y, 0));
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(spawnX, bottomY - 0.5f, 0), new Vector3(spawnX, topY + 0.5f, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(killX, bottomY - 0.5f, 0), new Vector3(killX, topY + 0.5f, 0));
    }
}