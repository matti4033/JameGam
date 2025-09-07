using System.Diagnostics.Tracing;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject UiText;
    public GameObject goLevel1;
    public GameObject goLevel2;
    public GameObject pauseMenu;

    void Start()
    {
        UiText.SetActive(false);
        goLevel1.SetActive(false);
        goLevel2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
