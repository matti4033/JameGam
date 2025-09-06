using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StringWaveDirectorMatti : MonoBehaviour
{
    [Header("Lane Visuals")]
    public GameObject stringPrefab;
    private List<GameObject> laneStrings = new List<GameObject>();

    [Header("Lanes / Area")]
    public int lanes = 6;
    public float topY = 3.5f;
    public float bottomY = -3.5f;
    public float spawnX = 10f;
    public float killX = -10f;

    [Header("Prefabs")]
    public GameObject wavePrefab;
    private List<GameObject> waves = new List<GameObject>();


    [Header("Pattern")]
    [Tooltip("Optional .txt with lines of '.' (safe) and 'x' (danger) with height = lanes")]
    public TextAsset patternText;

    public bool playOnStart = true;
    public float bpm = 100f;
    public int stepsPerBeat = 2;
    public float telegraphLead = 0.35f;

    [Header("Wave Params")]
    public float waveSpeed = 8.0f;
    public float waveLength = 2.8f;
    public float waveLifePadding = 0.4f;

    [Header("Corridor Mode")]
    public bool safeCorridorMode = true;
    public int maxLaneChangePerStep = 1;
    [Range(0f, 1f)] public float corridorWander = 0.7f;

    [Header("Randomization")]
    public float spawnJitterY = 0.06f;

    [Header("Runtime")]
    public int stepsToRun = 64;

    [Header("Tokens")]
    public GameObject tokenPrefab;
    public bool tokensUntilGoal = true;
    [Range(0f, 1f)] public float tokenSpawnChance = 0.55f;
    public int minStepsBetweenToken = 2;
    public Vector2 tokenOffset = Vector2.zero;
    public float tokenSlideSpeedMul = 1f;

    private TokenGoal _goal;
    private int _steps, _lastTokenStep = -999;
    private int currentSafeLane;
    private List<string> patternLines;

    private float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);

    void Start()
    {
        LoadPattern();
        _goal = FindAnyObjectByType<TokenGoal>();
        currentSafeLane = Mathf.Clamp(lanes / 2, 0, Mathf.Max(0, lanes - 1));

        SpawnPermanentStrings();

        if (playOnStart)
            StartCoroutine(Run());
    }

    private void SpawnPermanentStrings()
    {
        if (!stringPrefab || lanes <= 0) return;

        float leftX = Mathf.Min(spawnX, killX);
        float rightX = Mathf.Max(spawnX, killX);
        float targetWidth = rightX - leftX;

        for (int lane = 0; lane < lanes; lane++)
        {
            float y = LaneY(lane);

            GameObject laneObj = Instantiate(stringPrefab, new Vector3(leftX, y, 0f), Quaternion.identity);

            SpriteRenderer sr = laneObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float spriteWidth = sr.bounds.size.x / laneObj.transform.localScale.x;

                if (spriteWidth > 0f)
                {
                    // Scale X so the sprite exactly matches the target width
                    Vector3 scale = laneObj.transform.localScale;
                    scale.x = targetWidth / spriteWidth;
                    laneObj.transform.localScale = scale;
                }
            }
            BoxCollider2D col = laneObj.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(targetWidth, col.size.y);
                col.offset = new Vector2(targetWidth / 2f, col.offset.y);
            }

            laneObj.transform.SetParent(transform, true);

            laneStrings.Add(laneObj);
        }
    }




    private void LoadPattern()
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

    private IEnumerator Run()
    {
        int steps = 0;

        while (true)
        {
            int nextSafe = currentSafeLane;

            if (patternLines == null || patternLines.Count == 0)
            {
                if (!safeCorridorMode)
                {
                    yield return null;
                    continue;
                }

                nextSafe = PickNextSafeLane();
            }
            else
            {
                for (int lane = 0; lane < lanes && lane < patternLines.Count; lane++)
                {
                    var row = patternLines[lane];
                    int idx = steps % Mathf.Max(1, row.Length);
                    char ch = row[idx];
                    if (ch == '.') nextSafe = lane;
                }
            }

            SpawnStep(nextSafe);
            SpawnTokenIfNeeded(nextSafe);

            _steps++;
            yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
            currentSafeLane = nextSafe;
            steps++;
        }
    }

    private void SpawnTokenIfNeeded(int safeLane)
    {
        if (tokenPrefab && TokensNeeded() && (_steps - _lastTokenStep) >= minStepsBetweenToken &&
            Random.value < tokenSpawnChance)
        {
            Vector3 p = new Vector3(spawnX, LaneY(safeLane), 0f) + (Vector3)tokenOffset;
            var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
            var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
            ct.moveMode = CollectibleToken.MoveMode.Slide;
            ct.moveSpeed = waveSpeed * tokenSlideSpeedMul;
            ct.killX = killX;

            _lastTokenStep = _steps;
        }
    }

    private bool TokensNeeded() => !_goal || _goal.NeedsTokens();

    private int PickNextSafeLane()
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

    private void SpawnStep(int safeLane)
    {
        if (!wavePrefab) return;

        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == safeLane) continue;
            var pos = new Vector3(spawnX, LaneY(lane), 0f);
            var go = Instantiate(wavePrefab, pos, Quaternion.identity);
            waves.Add(go);
            var seg = go.GetComponent<WaveSegment>();
            if (seg != null)
            {
                seg.speed = waveSpeed;
                seg.length = waveLength;
                seg.killX = killX - waveLifePadding;
            }
        }
    }

    private float LaneY(int lane)
    {
        if (lanes <= 0) return 0f;
        float t = (lanes == 1) ? 0.5f : (lane + 0.5f) / lanes;
        return Mathf.Lerp(topY, bottomY, t);
    }
    public void ClearAllWaves()
    {
        foreach (var note in waves)
        {
            if (note != null) Destroy(note);
        }
        waves.Clear();
    }
}
