using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int bossesdead = 0;

    public bool bossOneDead = false;
    public bool bossTwoDead = false;
    public bool bossThreeDead = false;

    private void Awake()
    {
        if(Instance == null)
            Instance = this; DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {

    }

}
