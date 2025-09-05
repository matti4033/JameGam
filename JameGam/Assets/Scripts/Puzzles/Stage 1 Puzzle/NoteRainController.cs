using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoteRainController : MonoBehaviour
{
    [Header("Area and Columns")] public int columns = 6;
    public float leftX = -6f;
    public float rightX = 6f;
    public float spawnY = 6f;

    [Header("Prefabs")] public GameObject noteRainPrefab;
    public GameObject telegramPrefab;

    [Header("Pattern")] [Tooltip("Optional .txt with lines of '.' (safe) and 'x' (danger) with width = columns")]
    public TextAsset patternText;

    public bool playOnStart = true;
    public float bpm = 100f;
    public int stepsPerBeat = 2;

    [Header("Telegraph")] public float telegraphLead = 0.4f;
    public float telegraphFade = 0.15f;

    [Header("Note Speed")] public float noteSpeed = 12f;
    public float killY = -6.5f;

    [Header("Corridor Mode")] [Tooltip("If true, ignores pattern and runs a moving safe corridor")]
    public bool safeCorridorMode = true;

    public int maxColumnDeltaPerStep = 1;
    [Range(0f, 1f)] public float corridorWander = 0.7f;

    [Header("Style")] public float telegraphScaleY = 1.2f;
    public float spawnJitter = 0.06f;

    [Header("Tokens")] public GameObject tokenPrefab;
    public bool tokensUntilGoal = true;
    [Range(0f, 1f)] public float tokenSpawnChance = 0.55f;
    public int minStepsBetweenToken = 2;
    public Vector2 tokenOffset = Vector2.zero;

    [Header("Token Speed (multiplier)")] public float tokenFallSpeedMul = 1f;

    HashSet<int> _tokenSteps;
    int _tokensSpawned;
    int _stepIndex, _lastTokenStep = -999;

    private List<string> patternLines;
    private int currentSafeCol;
    private float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);

    TokenGoal _goal;

    void Start()
    {
        LoadPattern();
        _goal = FindAnyObjectByType<TokenGoal>();

        currentSafeCol = Mathf.Clamp(columns / 2, 0, Mathf.Max(0, columns - 1));
        if (playOnStart) StartCoroutine(Run());
    }

    private void LoadPattern()
    {
        if (patternText != null)
        {
            patternLines = patternText.text
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.TrimEnd('\r'))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            for (int i = 0; i < patternLines.Count; i++)
            {
                string ln = patternLines[i];
                if (ln.Length < columns) ln = ln.PadRight(columns, '.');
                else if (ln.Length > columns) ln = ln.Substring(0, columns);
                patternLines[i] = ln;
            }
        }
        else
        {
            patternLines = null;
        }
    }

    IEnumerator Run()
    {
        int row = 0;

        while (true)
        {
            if (patternLines == null || patternLines.Count == 0)
            {
                if (safeCorridorMode)
                {
                    int nextSafe = PickNextSafeColumn();
                    Telegraph(nextSafe);
                    yield return new WaitForSeconds(telegraphLead);

                    SpawnStep(nextSafe);

                    // Maybe spawn a token (in the safe column)
                    if (tokenPrefab && TokensNeeded() && (_stepIndex - _lastTokenStep) >= minStepsBetweenToken &&
                        UnityEngine.Random.value < tokenSpawnChance)
                    {
                        Vector3 p = new Vector3(ColumnX(nextSafe), spawnY, 0f) + (Vector3)tokenOffset;
                        var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                        var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
                        ct.moveMode = CollectibleToken.MoveMode.Fall;
                        ct.moveSpeed = noteSpeed * tokenFallSpeedMul;
                        ct.killY = killY;

                        _lastTokenStep = _stepIndex;
                    }

                    _stepIndex++;
                    yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
                    currentSafeCol = nextSafe;
                }
                else
                {
                    // No pattern and not in corridor mode; just idle this frame
                    yield return null;
                }
            }
            else
            {
                string ln = patternLines[row % patternLines.Count];
                // safe = '.', danger = 'x' (or anything not '.')
                int safeCol = -1;
                for (int c = 0; c < ln.Length; c++)
                {
                    if (ln[c] == '.') safeCol = c;
                }

                int nextSafe = (safeCol >= 0) ? safeCol : currentSafeCol;

                Telegraph(nextSafe);
                yield return new WaitForSeconds(telegraphLead);

                SpawnStep(nextSafe);

                // token (optional)
                if (tokenPrefab && TokensNeeded() && (_stepIndex - _lastTokenStep) >= minStepsBetweenToken &&
                    UnityEngine.Random.value < tokenSpawnChance)
                {
                    Vector3 p = new Vector3(ColumnX(nextSafe), spawnY, 0f) + (Vector3)tokenOffset;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>() ?? tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Fall;
                    ct.moveSpeed = noteSpeed * tokenFallSpeedMul;
                    ct.killY = killY;

                    _lastTokenStep = _stepIndex;
                }

                _stepIndex++;
                row++;
                yield return new WaitForSeconds(Mathf.Max(0f, StepDuration - telegraphLead));
                currentSafeCol = nextSafe;
            }
        }
    }

    bool TokensNeeded() => !tokensUntilGoal || _goal == null || _goal.NeedsTokens();

    int PickNextSafeColumn()
    {
        List<int> candidates = new List<int>();
        for (int d = -maxColumnDeltaPerStep; d <= maxColumnDeltaPerStep; d++)
        {
            int c = currentSafeCol + d;
            if (c < 0 || c >= columns) continue;
            candidates.Add(c);
        }

        if (candidates.Count == 0) return currentSafeCol;

        // Weighted wander towards neighbors
        if (UnityEngine.Random.value < corridorWander)
        {
            candidates.RemoveAll(c => Mathf.Abs(c - currentSafeCol) != 1);
            if (candidates.Count == 0) return currentSafeCol;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    void Telegraph(int safeCol)
    {
        if (!telegramPrefab) return;

        for (int c = 0; c < columns; c++)
        {
            if (c == safeCol) continue;

            var go = Instantiate(telegramPrefab, new Vector3(ColumnX(c), spawnY, 0f), Quaternion.identity);
            go.transform.localScale =
                new Vector3(go.transform.localScale.x, telegraphScaleY, go.transform.localScale.z);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr)
            {
                StartCoroutine(FadeAfter(sr, telegraphLead, telegraphFade));
            }

            Destroy(go, telegraphLead + telegraphFade + 0.05f);
        }
    }

    IEnumerator FadeAfter(SpriteRenderer sr, float delay, float fadeTime)
    {
        yield return new WaitForSeconds(delay);
        if (!sr) yield break;

        float t = 0f;
        var c0 = sr.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / fadeTime));
            sr.color = new Color(c0.r, c0.g, c0.b, a);
            yield return null;
        }

        if (sr) sr.enabled = false;
    }

    void SpawnStep(int safeCol)
    {
        if (!noteRainPrefab) return;

        // Spawn in all but the safe column
        for (int c = 0; c < columns; c++)
        {
            if (c == safeCol) continue;

            float jitter = UnityEngine.Random.Range(-spawnJitter, spawnJitter);
            var p = new Vector3(ColumnX(c) + jitter, spawnY, 0f);
            var go = Instantiate(noteRainPrefab, p, Quaternion.identity);

            var fn = go.GetComponent<FallingNote>();
            if (fn)
            {
                fn.speed = noteSpeed;
                fn.killY = killY;
            }
        }
    }

    float ColumnX(int col)
    {
        float t = (col + 0.5f) / Mathf.Max(1, columns);
        return Mathf.Lerp(leftX, rightX, t);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int c = 0; c < Mathf.Max(1, columns); c++)
        {
            float x = ColumnX(c);
            Gizmos.DrawLine(new Vector3(x, spawnY, 0), new Vector3(x, spawnY - 0.7f, 0));
        }

        Gizmos.color = new Color(1, 1, 1, 0.15f);
        Gizmos.DrawLine(new Vector3(leftX, spawnY, 0), new Vector3(rightX, spawnY, 0));
    }
}