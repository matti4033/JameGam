using System.Diagnostics.Tracing;
using UnityEngine;

public class UI : MonoBehaviour
{
    public GameObject UiText;
    public GameObject pauseMenu;

    void Start()
    {
        UiText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
