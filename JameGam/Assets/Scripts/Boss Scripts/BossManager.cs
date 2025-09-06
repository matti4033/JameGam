using System.Collections;
using UnityEngine;
using TMPro;

public class BossManager : MonoBehaviour
{
    public BaseBoss bossPrefab;
    public BaseBoss activeBoss;
    public Transform spawnPos;

    public float bossSpawnTimer;
    public float timer;
    private bool doOnce = false;
    public TMP_Text cleanseprompt;

    public void StartBossFight()
    {
        StartCoroutine(SpawnBoss());

    }

    public void Update()
    {
        if(activeBoss.IsTired) cleanseprompt.gameObject.SetActive(true);
        
        if(!activeBoss.IsTired) cleanseprompt.gameObject.SetActive(false);
    }
    IEnumerator SpawnBoss()
    {
        yield return new WaitForSeconds(bossSpawnTimer);
        if (!doOnce)
        {
            Vector3 startPos = spawnPos.position;
            activeBoss = Instantiate(bossPrefab, startPos, Quaternion.identity);
            activeBoss.OnBossDefeated += HandleBossDefeated;
            doOnce = true;
        }
        Debug.Log("Boss fight started!");
        timer = 0;
    }

    private void HandleBossDefeated(BaseBoss boss)
    {
        Debug.Log($"Boss {boss.bossName} defeated!");

        if (GameManager.Instance != null)
        {
            switch (boss.bossName)
            {
                case "BossOne":
                    if (!GameManager.Instance.bossOneDead)
                    {
                        GameManager.Instance.bossOneDead = true;
                        GameManager.Instance.bossesdead++;
                    }
                    break;
                case "BossTwo":
                    if (!GameManager.Instance.bossTwoDead)
                    {
                        GameManager.Instance.bossTwoDead = true;
                        GameManager.Instance.bossesdead++;
                    }
                    break;
                case "BossThree":
                    if (!GameManager.Instance.bossThreeDead)
                    {
                        GameManager.Instance.bossThreeDead = true;
                        GameManager.Instance.bossesdead++;
                    }
                    break;
            }
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        player?.GetComponent<PlayerHealth>()?.ApplyBossBonus(2);

        StartCoroutine(EndFight());
    }

    private IEnumerator EndFight()
    {
        yield return new WaitForSeconds(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("OverWorld3D");
    }
}