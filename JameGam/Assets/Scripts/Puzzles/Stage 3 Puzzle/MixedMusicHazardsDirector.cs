using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MixedMusicHazardsDirector : MonoBehaviour
{
    [Header("Grid & Area")] public int lanes = 6; // rows (strings)
    public int columns = 6; // vertical rain columns
    public float topY = 3.6f;
    public float bottomY = -3.6f;
    public float spawnX_Waves = 11f; // waves spawn at right, move left
    public float killX_Waves = -11f;
    public float spawnY_Notes = 6.5f; // notes spawn at top, fall down
    public float killY_Notes = -6.5f;
    public float leftX_Notes = -7.5f; // leftmost column world X
    public float rightX_Notes = 7.5f;

    [Header("Prefabs")] public GameObject wavePrefab;
    public GameObject notePrefab;
    public GameObject telegraphLane;
    public GameObject telegraphColumn;

    [Header("Timing (music-like)")] public float bpm = 112f;
    public int stepsPerBeat = 2;
    public float telegraphLead = 0.40f;

    [Header("Waves")] public float waveSpeed = 8.5f;
    public float waveLength = 3.0f;
    public float waveLifePadding = 0.5f;

    [Header("Falling Notes")] public float noteSpeed = 10.0f;

    [Header("Path constraints (fairness)")]
    public int maxLaneDeltaPerStep = 1;

    public int maxColumnDeltaPerStep = 1;
    [Range(0f, 1f)] public float laneWander = 0.7f;
    [Range(0f, 1f)] public float columnWander = 0.7f;

    [Header("Run")] public bool playOnStart = true;
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
    bool _nextTokenFalls = true;

    float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);
    int safeLane, safeColumn;
    System.Random rng;

    void Start()
    {
        rng = new System.Random();
        safeLane = Mathf.Clamp(lanes / 2, 0, Mathf.Max(0, lanes - 1));
        safeColumn = Mathf.Clamp(columns / 2, 0, Mathf.Max(0, columns - 1));

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
            int nextLane = PickNextIndex(safeLane, lanes, maxLaneDeltaPerStep, laneWander);
            int nextCol = PickNextIndex(safeColumn, columns, maxColumnDeltaPerStep, columnWander);

            Telegraph(nextLane, nextCol);
            yield return new WaitForSeconds(telegraphLead);

            SpawnStep(nextLane, nextCol);

            // token on safe path: alternate fall/slide
            if (tokenPrefab && _tokenSteps.Contains(_steps) && _tokensSpawned < totalTokens)
            {
                if (_nextTokenFalls)
                {
                    Vector3 p = new Vector3(ColumnX(nextCol), spawnY_Notes, 0f) + (Vector3)tokenOffset;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>();
                    if (ct == null) ct = tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Fall;
                    ct.moveSpeed = noteSpeed;
                    ct.killY = killY_Notes;
                }
                else
                {
                    Vector3 p = new Vector3(spawnX_Waves, LaneY(nextLane), 0f) + (Vector3)tokenOffset;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>();
                    if (ct == null) ct = tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Slide;
                    ct.moveSpeed = waveSpeed;
                    ct.killX = killX_Waves - waveLifePadding;
                }

                _nextTokenFalls = !_nextTokenFalls;
                _tokensSpawned++;
            }

            _steps++;

            yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
            safeLane = nextLane;
            safeColumn = nextCol;
            steps++;
        }
    }

    int PickNextIndex(int current, int count, int maxDelta, float wander)
    {
        List<int> opts = new List<int>();
        for (int d = -maxDelta; d <= maxDelta; d++)
        {
            int v = Mathf.Clamp(current + d, 0, count - 1);
            if (!opts.Contains(v)) opts.Add(v);
        }

        if (rng.NextDouble() < wander && opts.Count > 1)
        {
            opts.Remove(current);
            return opts[rng.Next(opts.Count)];
        }

        return current;
    }

    void Telegraph(int laneSafe, int colSafe)
    {
        if (telegraphLane)
        {
            for (int lane = 0; lane < lanes; lane++)
            {
                if (lane == laneSafe) continue;
                Vector3 pos = new Vector3(spawnX_Waves, LaneY(lane), 0f);
                var t = Instantiate(telegraphLane, pos, Quaternion.identity);
                var tm = t.GetComponent<TelegraphMarker>();
                if (tm)
                {
                    tm.duration = telegraphLead;
                    tm.width = waveLength;
                }
            }
        }

        if (telegraphColumn)
        {
            for (int c = 0; c < columns; c++)
            {
                if (c == colSafe) continue;
                Vector3 pos = new Vector3(ColumnX(c), spawnY_Notes, 0f);
                var t = Instantiate(telegraphColumn, pos, Quaternion.identity);
                var tm = t.GetComponent<TelegraphMarker>();
                if (tm) tm.duration = telegraphLead;
            }
        }
    }

    void SpawnStep(int laneSafe, int colSafe)
    {
        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == laneSafe) continue;
            var go = Instantiate(wavePrefab, new Vector3(spawnX_Waves, LaneY(lane), 0f), Quaternion.identity);
            var seg = go.GetComponent<WaveSegment>();
            if (seg)
            {
                seg.speed = waveSpeed;
                seg.length = waveLength;
                seg.killX = killX_Waves - waveLifePadding;
            }
        }

        for (int c = 0; c < columns; c++)
        {
            if (c == colSafe) continue;
            var go = Instantiate(notePrefab, new Vector3(ColumnX(c), spawnY_Notes, 0f), Quaternion.identity);
            var fn = go.GetComponent<FallingNote>();
            if (fn)
            {
                fn.speed = noteSpeed;
                fn.killY = killY_Notes;
            }
        }
    }

    float LaneY(int lane)
    {
        if (lanes <= 0) return 0f;
        float t = (lanes == 1) ? 0.5f : (lane + 0.5f) / lanes;
        return Mathf.Lerp(topY, bottomY, t);
    }

    float ColumnX(int col)
    {
        if (columns <= 0) return 0f;
        float t = (columns == 1) ? 0.5f : (col + 0.5f) / columns;
        return Mathf.Lerp(leftX_Notes, rightX_Notes, t);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, 0.25f);
        for (int i = 0; i < Mathf.Max(1, lanes); i++)
        {
            float y = (lanes == 1) ? (topY + bottomY) * 0.5f : Mathf.Lerp(topY, bottomY, (i + 0.5f) / lanes);
            Gizmos.DrawLine(new Vector3(-20, y, 0), new Vector3(20, y, 0));
        }

        Gizmos.color = new Color(0, 1, 1, 0.2f);
        for (int c = 0; c < Mathf.Max(1, columns); c++)
        {
            float x = (columns == 1)
                ? (leftX_Notes + rightX_Notes) * 0.5f
                : Mathf.Lerp(leftX_Notes, rightX_Notes, (c + 0.5f) / columns);
            Gizmos.DrawLine(new Vector3(x, 20, 0), new Vector3(x, -20, 0));
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(spawnX_Waves, bottomY - 0.5f, 0), new Vector3(spawnX_Waves, topY + 0.5f, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(killX_Waves, bottomY - 0.5f, 0), new Vector3(killX_Waves, topY + 0.5f, 0));
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(leftX_Notes, spawnY_Notes, 0), new Vector3(rightX_Notes, spawnY_Notes, 0));
    }
}