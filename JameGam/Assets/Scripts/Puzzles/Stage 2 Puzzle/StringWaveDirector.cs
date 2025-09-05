using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StringWaveDirector : MonoBehaviour
{
    [Header("Lane Visuals")]
    public GameObject stringPrefab;
    private List<GameObject> laneStrings = new List<GameObject>();

    [Header("Lanes / Area")] public int lanes = 6;
    public float topY = 3.5f;
    public float bottomY = -3.5f;
    public float spawnX = 10f;
    public float killX = -10f;

    [Header("Prefabs")] public GameObject wavePrefab;
    public GameObject telegraphPrefab;

    [Header("Pattern")] [Tooltip("Optional .txt with lines of '.' (safe) and 'x' (danger) with height = lanes")]
    public TextAsset patternText;

    public bool playOnStart = true;
    public float bpm = 100f;
    public int stepsPerBeat = 2;
    public float telegraphLead = 0.35f;

    [Header("Wave Params")] public float waveSpeed = 8.0f;
    public float waveLength = 2.8f;
    public float waveLifePadding = 0.4f;

    [Header("Corridor Mode")] public bool safeCorridorMode = true;
    public int maxLaneChangePerStep = 1;
    [Range(0f, 1f)] public float corridorWander = 0.7f;

    [Header("Randomization")] public float spawnJitterY = 0.06f;

    [Header("Runtime")] public int stepsToRun = 64;

    [Header("Tokens")] public GameObject tokenPrefab;
    public bool tokensUntilGoal = true;
    [Range(0f, 1f)] public float tokenSpawnChance = 0.55f;
    public int minStepsBetweenToken = 2;
    public Vector2 tokenOffset = Vector2.zero;

    [Header("Token Speed (multiplier)")] public float tokenSlideSpeedMul = 1f;

    TokenGoal _goal;
    int _steps, _lastTokenStep = -999;

    float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);
    int currentSafeLane;

    List<string> patternLines;

    void Start()
    {
        LoadPattern();
        _goal = FindAnyObjectByType<TokenGoal>();
        currentSafeLane = Mathf.Clamp(lanes / 2, 0, Mathf.Max(0, lanes - 1));
        SpawnPermanentStrings();

        if (playOnStart) StartCoroutine(Run());
    }

    void SpawnPermanentStrings()
    {
        if (stringPrefab == null) return;

        float leftX = Mathf.Min(spawnX, killX);
        float rightX = Mathf.Max(spawnX, killX);
        float width = rightX - leftX;
        float centerX = leftX + width * 0.5f;

        for (int lane = 0; lane < lanes; lane++)
        {
            float y = LaneY(lane);
            var pos = new Vector3(centerX, y, 0f);

            var laneObj = Instantiate(stringPrefab, pos, Quaternion.identity, transform);

            var scale = laneObj.transform.localScale;
            laneObj.transform.localScale = new Vector3(width, scale.y, scale.z);

            laneStrings.Add(laneObj);
        }
    }

    void LoadPattern()
    {
        if (patternText == null)
        {
            patternLines = null;
            return;
        }

        var raw = patternText.text.Replace("\r", "");
        var lines = new List<string>(raw.Split('\n'));
        lines.RemoveAll(string.IsNullOrWhiteSpace);

        patternLines = lines;
    }

    IEnumerator Run()
    {
        int steps = 0;

        while (true)
        {
            if (patternLines == null || patternLines.Count == 0)
            {
                if (!safeCorridorMode)
                {
                    yield return null;
                    continue;
                }

                int nextSafe = PickNextSafeLane();

                Telegraph(nextSafe);
                yield return new WaitForSeconds(telegraphLead);

                SpawnStep(nextSafe);

                // Maybe spawn a token in the safe lane
                if (tokenPrefab && TokensNeeded() && (_steps - _lastTokenStep) >= minStepsBetweenToken &&
                    Random.value < tokenSpawnChance)
                {
                    Vector3 p = new Vector3(spawnX, LaneY(nextSafe), 0f) + (Vector3)tokenOffset;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Slide;
                    ct.moveSpeed = waveSpeed * tokenSlideSpeedMul;
                    ct.killX = killX;

                    _lastTokenStep = _steps;
                }

                _steps++;
                yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
                currentSafeLane = nextSafe;
                steps++;
            }
            else
            {
                // Choose the only '.' lane as safe (or fallback to current).
                int nextSafe = currentSafeLane;
                for (int lane = 0; lane < lanes && lane < patternLines.Count; lane++)
                {
                    var row = patternLines[lane];
                    int idx = steps % Mathf.Max(1, row.Length);
                    char ch = row[idx];
                    if (ch == '.') nextSafe = lane;
                }

                Telegraph(nextSafe);
                yield return new WaitForSeconds(telegraphLead);

                SpawnStep(nextSafe);

                // token?
                if (tokenPrefab && TokensNeeded() && (_steps - _lastTokenStep) >= minStepsBetweenToken &&
                    Random.value < tokenSpawnChance)
                {
                    Vector3 p = new Vector3(spawnX, LaneY(nextSafe), 0f) + (Vector3)tokenOffset;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Slide;
                    ct.moveSpeed = waveSpeed * tokenSlideSpeedMul;
                    ct.killX = killX;

                    _lastTokenStep = _steps;
                }

                _steps++;

                yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
                currentSafeLane = nextSafe;
                steps++;
            }
        }
    }

    bool TokensNeeded() => !_goal || _goal.NeedsTokens();

    int PickNextSafeLane()
    {
        List<int> candidates = new List<int>();
        for (int d = -maxLaneChangePerStep; d <= maxLaneChangePerStep; d++)
        {
            int lane = currentSafeLane + d;
            if (lane < 0 || lane >= lanes) continue;
            candidates.Add(lane);
        }

        if (candidates.Count == 0) return currentSafeLane;

        if (Random.value < corridorWander)
        {
            candidates.RemoveAll(c => Mathf.Abs(c - currentSafeLane) != 1);
            if (candidates.Count == 0) return currentSafeLane;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    void Telegraph(int safeLane)
    {
        if (!stringPrefab) return;

        float leftX = Mathf.Min(spawnX, killX);
        float rightX = Mathf.Max(spawnX, killX);
        float width = rightX - leftX;

        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == safeLane) continue;

            float y = LaneY(lane);

            var pos = new Vector3(leftX + width * 0.5f, y, 0f);

            var laneObj = Instantiate(stringPrefab, pos, Quaternion.identity, transform);

            var scale = laneObj.transform.localScale;
            laneObj.transform.localScale = new Vector3(width, scale.y, scale.z);
        }
    }



    //void Telegraph(int safeLane)
    //{
    //    if (!telegraphPrefab) return;
    //    for (int lane = 0; lane < lanes; lane++)
    //    {
    //        if (lane == safeLane) continue;
    //        var p = new Vector3(spawnX, LaneY(lane), 0f);
    //        var go = Instantiate(telegraphPrefab, p, Quaternion.identity);
    //        Destroy(go, telegraphLead + 0.1f);
    //    }
    //}

    void SpawnStep(int safeLane)
    {
        if (!wavePrefab) return;

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