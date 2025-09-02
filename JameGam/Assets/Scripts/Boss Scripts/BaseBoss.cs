using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class BaseBoss : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Attack,
        Evade,
        Move
    }

    [Header("Settings")]
    public string bossName;
    public float maxHealth;
    public float currentHealth;

    [Header("AI Settings")]
    public float stateDuration;
    private float stateTimer;
    protected BossState currentState;

    protected bool isDead = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        ChangeState(BossState.Idle);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            PickNextState();
        }

        HandleState();
    }

    protected void ChangeState(BossState newState)
    {
        currentState = newState;
        stateTimer = stateDuration;
        OnEnterState(newState);
    }

    protected void PickNextState()
    {
        BossState next = (BossState)Random.Range(1, 4);
        ChangeState(next);
    }

    protected abstract void OnEnterState(BossState newState);
    protected abstract void HandleState();

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
