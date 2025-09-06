using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicChanger : MonoBehaviour
{
    public GameObject LevelOne;
    public GameObject LevelTwo;
    public GameObject LevelTree;

    public GameObject music;


    public float duration = 1f;
    public float counter;

    void Update()
    {
        //OverWorld();

        GamePlay();
    }

    public void OverWorld()
    {
        if (SceneManager.sceneCount == 1)
        {
            //Overworld LvlOne corrupted
            if (GameManager.Instance.bossesdead == 0 && LevelOne)
            {
                counter += Time.deltaTime;

                float t = Mathf.PingPong(counter / duration, 1f);

                LevelOne.GetComponent<AudioSource>().pitch = Mathf.Lerp(1.1f, 0.9f, t);
            }
            //Overworld LvlTwo corrupted
            if (GameManager.Instance.bossesdead == 0 && LevelTwo)
            {
                counter += Time.deltaTime;

                float t = Mathf.PingPong(counter / duration, 1f);

                LevelTwo.GetComponent<AudioSource>().pitch = Mathf.Lerp(1.1f, 0.9f, t);
            }
            //Overworld LvlThree corrupted
            if (GameManager.Instance.bossesdead == 0 && LevelTree)
            {
                counter += Time.deltaTime;

                float t = Mathf.PingPong(counter / duration, 1f);

                LevelTree.GetComponent<AudioSource>().pitch = Mathf.Lerp(1.1f, 0.9f, t);
            }
        }
        else
            return;
    }

    public void GamePlay()
    {
        //-------------------------------------------------------------LEVEL 1
        Debug.Log("1");
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            Debug.Log("2");
            if (GameManager.Instance.bossOneDead == false)
            {
                Debug.Log("3");
                counter += Time.deltaTime;

                float t = Mathf.PingPong(counter / duration, 1f);

                music.GetComponent<AudioSource>().pitch = Mathf.Lerp(1.1f, 0.9f, t);
            }
            else
                music.GetComponent<AudioSource>().pitch = 1f;
        }
        //-------------------------------------------------------------LEVEL 2
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            if (!GameManager.Instance.bossTwoDead == false)
            {
                counter += Time.deltaTime;

                float t = Mathf.PingPong(counter / duration, 1f);

                music.GetComponent<AudioSource>().pitch = Mathf.Lerp(1.1f, 0.9f, t);
            }
            else
                music.GetComponent<AudioSource>().pitch = 1f;
        }
        //-------------------------------------------------------------LEVEL 3
        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            if (!GameManager.Instance.bossThreeDead == false)
            {
                counter += Time.deltaTime;

                float t = Mathf.PingPong(counter / duration, 1f);

                music.GetComponent<AudioSource>().pitch = Mathf.Lerp(1.1f, 0.9f, t);
            }
            else
                music.GetComponent<AudioSource>().pitch = 1f;
        }
    }
}
