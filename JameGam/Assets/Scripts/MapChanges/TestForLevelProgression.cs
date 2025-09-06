using TMPro;
using UnityEngine;

public class TestForLevelProgression : MonoBehaviour
{

    [SerializeField] GameObject Level1;
    [SerializeField] GameObject Level2;
    [SerializeField] GameObject Level3;


    void Start()
    {
        //Checking how many bosses are killed and applying cleansed or corrupted color accordingly
        if (GameManager.Instance.bossesdead >= 1)
        {
            Level1.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 1f);
        }
        else
        {
            Level1.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 0f);
        }

        if (GameManager.Instance.bossesdead >= 2)
        {
            Level2.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 1f);
        }
        else
        {
            Level2.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 0f);
        }
        if (GameManager.Instance.bossesdead >= 3)
        {
            Level2.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 1f);
        }
        else
        {
            Level2.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 0f);
        }

    }

}
