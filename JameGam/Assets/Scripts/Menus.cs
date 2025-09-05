using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject gameMananger;

    public bool paused;

    private void Update()
    {
        if (!paused)
        {
            paused = false;
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);

        }

        if(!paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!paused || !GameManager.Instance.GameDoneZooooooo)
                {
                    paused = true;
                    Time.timeScale = 0f;
                    pauseMenu.SetActive(true);
                }
            }
        }

    }

    public void StartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(1);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
        pauseMenu.SetActive(false);
        paused = false;
    }

    public void MainMenu()
    {
        Destroy(gameMananger);
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
