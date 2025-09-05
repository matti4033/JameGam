using UnityEngine;

public class TokenGoal : MonoBehaviour
{
    public int target = 4;
    public int current = 0;

    public void Add(int v = 1)
    {
        current = Mathf.Min(target, current + v);
        Debug.Log($"[TokenGoal] Tokens: {current}/{target}");

        if (current >= target)
        {
            // Solve & fade
            var pm = FindAnyObjectByType<PuzzleManager>();
            if (pm != null) pm.SolvePuzzle();
            else Debug.LogWarning("[TokenGoal] No PuzzleManager found in scene.");
        }
    }
}