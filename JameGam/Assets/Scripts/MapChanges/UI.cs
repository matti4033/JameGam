using System.Diagnostics.Tracing;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject UiText;
    public GameObject pauseMenu;
    public bool paused;

    void Start()
    {
        UiText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                paused = true;
                Time.timeScale = 0f;
                pauseMenu.SetActive(true);
            }
            else
            {
                paused = false;
                Time.timeScale = 1f;
                pauseMenu.SetActive(false);
            }
        }
    }
}
