using System.Collections;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public BaseBoss bossPrefab;
    private BaseBoss activeBoss;
    public Transform spawnPos;

    public void StartBossFight()
    {
        Vector3 startPos = spawnPos.position;
        activeBoss = Instantiate(bossPrefab, startPos, Quaternion.identity);
        activeBoss.OnBossDefeated += HandleBossDefeated;

        Debug.Log("Boss fight started!");
    }

    private void HandleBossDefeated(BaseBoss boss)
    {
        Debug.Log($"Boss {boss.bossName} defeated!");

        if (GameManager.Instance != null)
        {
            switch (boss.bossName)
            {
                case "BossOne":
                    GameManager.Instance.bossOneDead = true;
                    GameManager.Instance.bossesdead++;
                    break;
                case "BossTwo":
                    GameManager.Instance.bossTwoDead = true;
                    GameManager.Instance.bossesdead++;
                    break;
                case "BossThree":
                    GameManager.Instance.bossThreeDead = true;
                    GameManager.Instance.bossesdead++;
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

        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}