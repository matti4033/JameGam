using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MixedPuzzlesMattiEdition : MonoBehaviour
{
    [Header("Grid & Area")] public int lanes = 6;
    public int columns = 6;
    public float topY = 3.6f;
    public float bottomY = -3.6f;
    public float spawnX_Waves = 11f;
    public float killX_Waves = -11f;
    public float spawnY_Notes = 6.5f;
    public float killY_Notes = -6.5f;
    public float leftX_Notes = -7.5f;
    public float rightX_Notes = 7.5f;

    [Header("Prefabs")] public GameObject wavePrefab;
    public GameObject noteRainPrefab;
    //public GameObject waveTelegraphPrefab;
    //public GameObject noteTelegraphPrefab;

    [Header("Strings")]
    public GameObject stringPrefab;
    private List<GameObject> laneStrings = new List<GameObject>();

    [Header("Pattern")] public TextAsset patternText;
    public bool playOnStart = true;
    public float bpm = 100f;
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

    [Header("Style")] public float waveSpawnJitterY = 0.06f;
    public float noteSpawnJitterX = 0.06f;
    public float noteTelegraphScaleY = 1.2f;
    public float noteTelegraphFade = 0.15f;

    [Header("Tokens")] public GameObject tokenPrefab;
    public bool tokensUntilGoal = true;
    [Range(0f, 1f)] public float tokenSpawnChance = 0.5f;
    public int minStepsBetweenToken = 2;
    public Vector2 tokenOffsetForNotes = Vector2.zero;
    public Vector2 tokenOffsetForWaves = Vector2.zero;

    [Header("Token Speed (multiplier)")] public float tokenFallSpeedMul = 1f;
    public float tokenSlideSpeedMul = 1f;

    TokenGoal _goal;

    //int _steps;
    //int _lastTokenStep = -999;
    int _stepIndex;
    int _lastTokenWaveStep = -999;
    int _lastTokenNoteStep = -999;

    int safeLane, safeColumn;
    bool _useWavesNext = true;

    float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);

    void Start()
    {
        _goal = FindAnyObjectByType<TokenGoal>();
        safeLane = Mathf.Clamp(lanes / 2, 0, Mathf.Max(0, lanes - 1));
        safeColumn = Mathf.Clamp(columns / 2, 0, Mathf.Max(0, columns - 1));
        if (stringPrefab)
            SpawnPermanentStrings();
        if (playOnStart) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        int steps = 0;

        while (true)
        {
            if (_useWavesNext)
            {
                int nextSafe = PickNextSafeLane();


                SpawnWaveStep(nextSafe);

                if (tokenPrefab && TokensNeeded() &&
                    (_stepIndex - _lastTokenWaveStep) >= minStepsBetweenToken &&
                    Random.value < tokenSpawnChance)
                {
                    Vector3 p = new Vector3(spawnX_Waves, LaneY(nextSafe), 0f) + (Vector3)tokenOffsetForWaves;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Slide;
                    ct.moveSpeed = waveSpeed * tokenSlideSpeedMul;
                    ct.killX = killX_Waves;

                    _lastTokenWaveStep = _stepIndex;
                }

                _stepIndex++;
                yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
                safeLane = nextSafe;
            }
            else
            {
                int nextSafe = PickNextSafeColumn();

                SpawnNoteStep(nextSafe);

                if (tokenPrefab && TokensNeeded() &&
                    (_stepIndex - _lastTokenNoteStep) >= minStepsBetweenToken &&
                    Random.value < tokenSpawnChance)
                {
                    Vector3 p = new Vector3(ColumnX(nextSafe), spawnY_Notes, 0f) + (Vector3)tokenOffsetForNotes;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Fall;
                    ct.moveSpeed = noteSpeed * tokenFallSpeedMul;
                    ct.killY = killY_Notes;

                    _lastTokenNoteStep = _stepIndex;
                }

                _stepIndex++;
                yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
                safeColumn = nextSafe;
            }

            _useWavesNext = !_useWavesNext;
            steps++;
        }
    }

    private void SpawnPermanentStrings()
    {
        if (!stringPrefab || lanes <= 0) return;

        float leftX = Mathf.Min(spawnX_Waves, killX_Waves);
        float rightX = Mathf.Max(spawnX_Waves, killX_Waves);
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


    bool TokensNeeded() => !tokensUntilGoal || _goal == null || _goal.NeedsTokens();

    int PickNextSafeLane()
    {
        List<int> candidates = new List<int>();
        for (int d = -maxLaneDeltaPerStep; d <= maxLaneDeltaPerStep; d++)
        {
            int lane = safeLane + d;
            if (lane < 0 || lane >= lanes) continue;
            candidates.Add(lane);
        }

        if (candidates.Count == 0) return safeLane;

        if (Random.value < laneWander)
        {
            candidates.RemoveAll(c => Mathf.Abs(c - safeLane) != 1);
            if (candidates.Count == 0) return safeLane;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    int PickNextSafeColumn()
    {
        List<int> candidates = new List<int>();
        for (int d = -maxColumnDeltaPerStep; d <= maxColumnDeltaPerStep; d++)
        {
            int col = safeColumn + d;
            if (col < 0 || col >= columns) continue;
            candidates.Add(col);
        }

        if (candidates.Count == 0) return safeColumn;

        if (Random.value < columnWander)
        {
            candidates.RemoveAll(c => Mathf.Abs(c - safeColumn) != 1);
            if (candidates.Count == 0) return safeColumn;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    void SpawnWaveStep(int laneSafe)
    {
        if (!wavePrefab) return;

        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == laneSafe) continue;
            var pos = new Vector3(spawnX_Waves, LaneY(lane), 0f);
            var go = Instantiate(wavePrefab, pos, Quaternion.identity);
            var seg = go.GetComponent<WaveSegment>();
            if (seg)
            {
                seg.speed = waveSpeed;
                seg.length = waveLength;
                seg.killX = killX_Waves - waveLifePadding;
            }
        }
    }

    void SpawnNoteStep(int safeCol)
    {
        if (!noteRainPrefab) return;

        for (int col = 0; col < columns; col++)
        {
            if (col == safeCol) continue;

            float jitter = Random.Range(-noteSpawnJitterX, noteSpawnJitterX);
            var p = new Vector3(ColumnX(col) + jitter, spawnY_Notes, 0f);
            var go = Instantiate(noteRainPrefab, p, Quaternion.identity);
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
        float t = (col + 0.5f) / columns;
        return Mathf.Lerp(leftX_Notes, rightX_Notes, t);
    }
}