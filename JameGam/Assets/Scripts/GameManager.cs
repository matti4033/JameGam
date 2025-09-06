using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameFinishedMenu;
    public GameObject gameOverMenu;

    public int bossesdead = 0;

    public bool bossOneDead = false;
    public bool bossTwoDead = false;
    public bool bossThreeDead = false;

    public bool GameDoneZooooooo = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(bossesdead >= 3)
        {
            Time.timeScale = 0f;
            gameFinishedMenu.SetActive(true);
        }
        else
        {
            gameFinishedMenu.SetActive(false);
        }
    }

    public static void ResetSingleton()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}
