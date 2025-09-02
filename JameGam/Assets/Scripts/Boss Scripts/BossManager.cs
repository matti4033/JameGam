using UnityEngine;

public class BossManager : MonoBehaviour
{
    public BaseBoss bossPrefab;
    private BaseBoss activeBoss;

    public void StartBossFight(Vector2 spawnPos)
    {
        activeBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        activeBoss.OnBossDefeated += HandleBossDefeated;

        Debug.Log("Boss fight started!");
    }

    private void HandleBossDefeated(BaseBoss boss)
    {
        Debug.Log($"Boss {boss.bossName} defeated!");

        //death effect, coroutine etc? Öppna nästa bana i overworld
    }
}
