using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class BaseBoss : MonoBehaviour
{

    [Header("Settings")]
    public string bossName;
    public float moveSpeed;
    public float lifeTime;
    public float timeAlive;

    public Rigidbody2D rb;
    public Transform player;

    protected bool isDead = false;

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        if (isDead) return;

        timeAlive += Time.deltaTime;
        if(timeAlive > lifeTime)
        {
            //prompt att man kan "cleansa" bossen!
            Debug.Log("CLEASE TIME!!!");
        }

    }

    protected virtual void Defeated()
    {
        // Do sum death effect?
        isDead = true;
        Debug.Log($"{bossName} dead!");
        OnBossDefeated?.Invoke(this);
    }

    public System.Action<BaseBoss> OnBossDefeated;

}
