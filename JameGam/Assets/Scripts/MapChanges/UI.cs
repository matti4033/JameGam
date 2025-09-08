using System.Diagnostics.Tracing;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject UiText;
    public GameObject goLevel1;
    public GameObject goLevel2;
    public GameObject pauseMenu;

    public GameObject tutorialText;
    public float tutorialTimer = 20f;

    void Start()
    {
        UiText.SetActive(false);
        goLevel1.SetActive(false);
        goLevel2.SetActive(false);

        if (GameManager.Instance.bossesdead == 0)
            tutorialText.SetActive(true);
        else
            tutorialText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        tutorialTimer -= Time.deltaTime;

        if (tutorialTimer < 0)
        {
            tutorialText.SetActive(false);
        }
    }
}
