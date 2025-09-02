using TMPro;
using UnityEngine;

public class TestForLevelProgression : MonoBehaviour
{

    [SerializeField] GameObject LevelMatti;
    [SerializeField] GameObject LevelBirkan;


    void Start()
    {
        //Checking how many bosses are killed and applying cleansed or corrupted color accordingly
        if (GameManager.Instance.bossesdead >= 1)
        {
            LevelMatti.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 1f);
        }
        else
        {
            LevelMatti.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 0f);
        }

        if (GameManager.Instance.bossesdead >= 2)
        {
            LevelBirkan.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 1f);
        }
        else
        {
            LevelBirkan.GetComponentInChildren<Renderer>().material.SetFloat("_Boss", 0f);
        }

    }

}
