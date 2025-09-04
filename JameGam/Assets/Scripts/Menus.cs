using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class Menus : MonoBehaviour
{
    public GameObject gameManager;

    public void StartGame()
    {
        SceneManager.LoadScene("Cornelis");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        Destroy(gameManager);
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
