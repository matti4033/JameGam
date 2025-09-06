using Unity.VisualScripting;
using UnityEngine;

public class MusicChanger : MonoBehaviour
{
    public GameObject LevelOne;
    public GameObject LevelTwo;
    public GameObject LevelTree;


    public float duration = 1f;
    public float counter;


    void Update()
    {
        OverWorld();

    }

    public void OverWorld()
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
}
