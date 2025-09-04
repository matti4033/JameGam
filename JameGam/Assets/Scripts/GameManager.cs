using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject endMenu;

    public int bossesdead = 0;

    public bool bossOneDead = false;
    public bool bossTwoDead = false;
    public bool bossThreeDead = false;

    public bool GameDoneZooooooo = false;

    private void Awake()
    {
        if(Instance == null)
            Instance = this; DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(bossesdead >= 3)
        {
            Time.timeScale = 0f;
            endMenu.SetActive(true);
        }
        else
        {
            endMenu.SetActive(false);
        }
    }

}
