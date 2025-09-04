using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    [Tooltip("How many timestep per beat (2 = eights, 4 = sixteenths)")]
    public int stepsPerBeat = 2;

    [Tooltip("Seconds the warning shows before the note falls")]
    public float telegrahLead = 0.45f;

    [Header("Patterns")]
    [Tooltip(
        "Grid: one line per time-step, one char per column. Use 'x' to drop, '.' for empty.\\nExample for 4 columns:\\n.x..\\n..x.\\n...x\\nx...\\n")]
    public TextAsset patternText;

    [Range(0f, 1f)] public float randomDensity = 0.35f;

    [Header("Falling")] public float noteSpeed = 9;

    [Tooltip("Destroy notes this far below the spawn line")]
    public float killY = -6f;

    [Header("Debug")] public bool playOnStart = true;
    public bool loopPattern = true;

    [Header("Always-a-Path Random (recommended)")]
    [Tooltip(
        "If ON, each step picks ONE safe column; all others spawn notes.\nThe safe column wanders slowly so there is always a reachable path.")]
    public bool safeCorridorMode = true;

    [Tooltip("Max number of columns the safe column can change per step (usually 1).")]
    public int maxColumnDeltaPerStep = 1;

    [Range(0f, 1f)] public float corridorWander = 0.7f; // chance to move vs. stay

    [Header("Style")] [Tooltip("Stretches the telegraph vertically (set to 0 to skip).")]
    public float telegraphScaleY = 1.2f;

    [Tooltip("Adds up to this many seconds of random extra delay to each spawn (nice cascade).")]
    public float spawnJitter = 0.06f;

    private List<string> patternLines;
    private int currentSafeCol;
    private float StepDuration => 60f / Mathf.Max(1f, bpm) / Mathf.Max(1, stepsPerBeat);

    void Start()
    {
        LoadPattern();
        currentSafeCol = Mathf.Clamp(columns / 2, 0, Mathf.Max(0, columns - 1));
        if (playOnStart)
        {
            StartCoroutine(Run());
        }
    }

    private void LoadPattern()
    {
        if (patternText != null)
        {
            patternLines = patternText.text.Split('\n').Select(s => s.TrimEnd("\r"))
                .Where(s => !string.IsNullOrEmpty(s)).ToList();

            // Normalize line widths to columns
            for (int i = 0; i < patternLines.Count; i++)
            {
                string ln = patternLines[i];
                if (ln.Length < columns)
                    ln = ln.PadRight(columns, '.');
                else if (ln.Length > columns)
                    ln = ln.Substring(0, columns);
                patternLines[i] = ln;
            }
        }
        else
        {
            // Random mode
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
                // Random modes
                if (safeCorridorMode)
                {
                    // Pick next safe column close to current for a reachable path
                    int nextSafe = PickNextSafeColumn();

                    // Telegraph and schedule drops on all but the safe column
                    for (int c = 0; c < columns; c++)
                    {
                        if (c == nextSafe) continue;
                        QueueDrop(c, extraDelay: UnityEngine.Random.Range(0f, spawnJitter));
                    }

                    // Advance safe column
                    currentSafeCol = nextSafe;
                }
                else
                {
                    // Random per column density
                    int safeChosen = -1;
                    for (int c = 0; c < columns; c++)
                    {
                        bool drop = UnityEngine.Random.value < randomDensity;
                        // if last column and none safe yet, force this one safe
                        if (c == columns - 1 && safeChosen < 0) drop = true;

                        if (drop) QueueDrop(c, extraDelay: UnityEngine.Random.Range(0f, spawnJitter));
                        else if (safeChosen < 0) safeChosen = c;
                    }

                    if (safeChosen < 0) safeChosen = UnityEngine.Random.Range(0, columns); // fallback guard
                }
            }
            else
            {
                // Pattern mode
                string line = patternLines[row];

                for (int c = 0; c < Mathf.Min(columns, line.Length); c++)
                {
                    if (line[c] == 'x' || line[c] == 'X' || line[c] == '1')
                        QueueDrop(c, extraDelay: 0f);
                }

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
        // allowed candidates within delta
        List<int> opts = new List<int>();
        for (int d = -maxColumnDeltaPerStep; d <= maxColumnDeltaPerStep; d++)
        {
            int v = Mathf.Clamp(currentSafeCol + d, 0, columns - 1);
            if (!opts.Contains(v)) opts.Add(v);
        }

        // bias to stay; corridorWander increases chance to move
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

        // Telegraph
        if (telegramPrefab != null)
        {
            var t = Instantiate(telegramPrefab, spawn, Quaternion.identity);
            if (telegraphScaleY > 0f) t.transform.localScale = new Vector3(t.transform.localScale.x, telegraphScaleY, 1f);
            
            var tm = t.GetComponent<TelegraphMarker>();
            if (tm) tm.duration = telegrahLead;
        }

        // Spawn the falling note after lead time
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

    // Gizmos for layout
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