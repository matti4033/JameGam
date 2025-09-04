using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class BaseBoss : MonoBehaviour
{

    [Header("Settings")]
    public string bossName;
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;

    public Rigidbody2D rb;
    public Transform player;

    protected bool isDead = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (isDead) return;

    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (currentHealth <= 0) Defeated();
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
