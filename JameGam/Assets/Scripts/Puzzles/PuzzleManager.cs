using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public UnityEvent OnPuzzleSolved;
    private bool solved = false;

    public void SolvePuzzle()
    {
        if (solved) return;
        Debug.Log("Puzzle fixad dayum!");
        solved = true;

        OnPuzzleSolved?.Invoke();
        solved = false;
    }
}
