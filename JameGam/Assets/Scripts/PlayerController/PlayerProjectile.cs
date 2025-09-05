using UnityEngine;

public class BossWakeProjectile : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        BaseBoss boss = collision.gameObject.GetComponent<BaseBoss>();
        if (boss != null)
        {
            boss.WakeUp();
            Destroy(gameObject);
        }
    }
}