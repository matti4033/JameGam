using UnityEngine;
using UnityEngine.SocialPlatforms;

public class UI : MonoBehaviour
{
    public GameObject uiText;
    public GameObject pauseMenu;

    public bool paused;
    void Start()
    {
        uiText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                Time.timeScale = 0f;
                pauseMenu.SetActive(true);
            }
            else
            {
                paused = false;
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}
