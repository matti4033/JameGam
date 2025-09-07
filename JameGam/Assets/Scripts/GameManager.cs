using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public bool dead = false;

    public TMP_Text playerHeartText;
    public int playerHealth = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerHealth = 3;
    }

    private void Update()
    {

        playerHeartText.text = playerHealth.ToString();

        if(bossesdead >= 3 && SceneManager.GetActiveScene().buildIndex == 1)
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
