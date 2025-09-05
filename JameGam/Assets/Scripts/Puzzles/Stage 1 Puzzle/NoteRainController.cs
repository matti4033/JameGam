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

    [Header("Timing")] public float bpm = 120;
    public int stepsPerBeat = 2;
    public float telegrahLead = 0.45f;

    [Header("Patterns")] public TextAsset patternText;
    [Range(0f, 1f)] public float randomDensity = 0.35f;

    [Header("Falling")] public float noteSpeed = 9;
    public float killY = -6f;

    [Header("Debug")] public bool playOnStart = true;
    public bool loopPattern = true;

    [Header("Always-a-Path Random (recommended)")]
    public bool safeCorridorMode = true;

    public int maxColumnDeltaPerStep = 1;
    [Range(0f, 1f)] public float corridorWander = 0.7f;

    [Header("Style")] public float telegraphScaleY = 1.2f;
    public float spawnJitter = 0.06f;

    [Header("Tokens")] public GameObject tokenPrefab;
    public int totalTokens = 4;
    public int firstTokenStep = 4;
    public int lastTokenStep = 28;
    public Vector2 tokenOffset = Vector2.zero;

    HashSet<int> _tokenSteps;
    int _tokensSpawned;
    int _stepIndex;

    private List<string> patternLines;
    private int currentSafeCol;
    private float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);

    void Start()
    {
        LoadPattern();

        _tokenSteps = new HashSet<int>();
        int lo = Mathf.Max(0, firstTokenStep);
        int hi = Mathf.Max(lo + 1, lastTokenStep);
        while (_tokenSteps.Count < totalTokens)
            _tokenSteps.Add(UnityEngine.Random.Range(lo, hi));

        currentSafeCol = Mathf.Clamp(columns / 2, 0, Mathf.Max(0, columns - 1));
        if (playOnStart) StartCoroutine(Run());
    }

    private void LoadPattern()
    {
        if (patternText != null)
        {
            patternLines = patternText.text
                .Split('\n')
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
        else patternLines = null;
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

                    if (tokenPrefab && _tokenSteps.Contains(_stepIndex) && _tokensSpawned < totalTokens)
                    {
                        Vector3 p = new Vector3(ColumnX(nextSafe), spawnY, 0f) + (Vector3)tokenOffset;
                        var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                        var ct = tok.GetComponent<CollectibleToken>();
                        if (ct == null) ct = tok.AddComponent<CollectibleToken>();
                        ct.moveMode = CollectibleToken.MoveMode.Fall;
                        ct.moveSpeed = noteSpeed;
                        ct.killY = killY;
                        _tokensSpawned++;
                    }

                    _stepIndex++;

                    for (int c = 0; c < columns; c++)
                    {
                        if (c == nextSafe) continue;
                        QueueDrop(c, extraDelay: UnityEngine.Random.Range(0f, spawnJitter));
                    }

                    currentSafeCol = nextSafe;
                }
                else
                {
                    int safeChosen = -1;
                    for (int c = 0; c < columns; c++)
                    {
                        bool drop = UnityEngine.Random.value < randomDensity;
                        if (c == columns - 1 && safeChosen < 0) drop = true;

                        if (drop) QueueDrop(c, extraDelay: UnityEngine.Random.Range(0f, spawnJitter));
                        else if (safeChosen < 0) safeChosen = c;
                    }

                    if (safeChosen < 0) safeChosen = UnityEngine.Random.Range(0, columns);
                }
            }
            else
            {
                string line = patternLines[row];

                List<int> safeCols = new List<int>();
                for (int c = 0; c < Mathf.Min(columns, line.Length); c++)
                    if (!(line[c] == 'x' || line[c] == 'X' || line[c] == '1'))
                        safeCols.Add(c);

                if (tokenPrefab && safeCols.Count > 0 && _tokenSteps.Contains(_stepIndex) &&
                    _tokensSpawned < totalTokens)
                {
                    int col = safeCols[UnityEngine.Random.Range(0, safeCols.Count)];
                    Vector3 p = new Vector3(ColumnX(col), spawnY, 0f) + (Vector3)tokenOffset;
                    var tok = Instantiate(tokenPrefab, p, Quaternion.identity);
                    var ct = tok.GetComponent<CollectibleToken>();
                    if (ct == null) ct = tok.AddComponent<CollectibleToken>();
                    ct.moveMode = CollectibleToken.MoveMode.Fall;
                    ct.moveSpeed = noteSpeed;
                    ct.killY = killY;
                    _tokensSpawned++;
                }

                _stepIndex++;

                for (int c = 0; c < Mathf.Min(columns, line.Length); c++)
                    if (line[c] == 'x' || line[c] == 'X' || line[c] == '1')
                        QueueDrop(c, extraDelay: 0f);

                row++;
                if (row >= patternLines.Count)
                {
                    if (loopPattern) row = 0;
                    else yield break;
                }
            }

            yield return new WaitForSeconds(StepDuration);
        }
    }

    int PickNextSafeColumn()
    {
        List<int> opts = new List<int>();
        for (int d = -maxColumnDeltaPerStep; d <= maxColumnDeltaPerStep; d++)
        {
            int v = Mathf.Clamp(currentSafeCol + d, 0, columns - 1);
            if (!opts.Contains(v)) opts.Add(v);
        }

        if (UnityEngine.Random.value < corridorWander && opts.Count > 1)
        {
            opts.Remove(currentSafeCol);
            return opts[UnityEngine.Random.Range(0, opts.Count)];
        }

        return currentSafeCol;
    }

    void QueueDrop(int col, float extraDelay)
    {
        Vector3 spawn = new Vector3(ColumnX(col), spawnY, 0f);

        if (telegramPrefab != null)
        {
            var t = Instantiate(telegramPrefab, spawn, Quaternion.identity);
            if (telegraphScaleY > 0f)
                t.transform.localScale = new Vector3(t.transform.localScale.x, telegraphScaleY, 1f);
            var tm = t.GetComponent<TelegraphMarker>();
            if (tm) tm.duration = telegrahLead;
        }

        StartCoroutine(SpawnAfter(col, telegrahLead + extraDelay));
    }

    IEnumerator SpawnAfter(int col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!noteRainPrefab) yield break;

        var note = Instantiate(noteRainPrefab, new Vector3(ColumnX(col), spawnY, 0f), Quaternion.identity);
        var fn = note.GetComponent<FallingNote>();
        if (fn)
        {
            fn.speed = noteSpeed;
            fn.killY = killY;
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