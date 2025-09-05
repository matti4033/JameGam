using UnityEngine;

public class TokenGoal : MonoBehaviour
{
    public int target = 4;
    public int current = 0;
    
    bool _completed = false;

    public void Add(int v = 1)
    {
        if (_completed) return;

        current = Mathf.Min(target, current + v);
        Debug.Log($"[TokenGoal] Tokens: {current}/{target}");

        if (current >= target)
        {
            _completed = true;

            // Clean up all directors/hazard spawners
            CleanupDirectors();

            // Ask PuzzleManager to solve (support both SolvePuzzle and solvepuzzle)
            var pm = FindAnyObjectByType<PuzzleManager>();
            if (pm != null)
            {
                // Call either casing safely without requiring the method
                pm.SendMessage("SolvePuzzle", SendMessageOptions.DontRequireReceiver);
                pm.SendMessage("solvepuzzle", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[TokenGoal] No PuzzleManager found in scene.");
            }
        }
    }

    public bool NeedsTokens() => current < target;

    void CleanupDirectors()
    {
        // Destroy NoteRainController
        foreach (var n in FindObjectsByType<NoteRainController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (n) Destroy(n.gameObject);
        }

        // Destroy StringWaveDirector
        foreach (var s in FindObjectsByType<StringWaveDirector>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (s) Destroy(s.gameObject);
        }

        // Destroy MixedMusicHazardsDirector
        foreach (var m in FindObjectsByType<MixedMusicHazardsDirector>(FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            if (m) Destroy(m.gameObject);
        }
    }
}