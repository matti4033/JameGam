using UnityEngine;

public class puzzletest : MonoBehaviour
{
    public GameObject puzzleManager;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            puzzleManager.GetComponent<PuzzleManager>().SolvePuzzle();
            
        }
    }
}
