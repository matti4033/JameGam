using UnityEngine;
using System;

public abstract class BaseBoss : MonoBehaviour
{
    [Header("Settings")]
    public string bossName;
    public float moveSpeed;
    public Rigidbody2D rb;
    public Transform player;

    [Header("Cycle Settings")]
    public int maxPhaseCycles = 2;
    public int maxAttackCycles = 3;

    protected bool isDead = false;
    protected bool isTired = false;
    protected int phaseCount = 0;
    protected int attackCycleCount = 0;

    public bool IsTired => isTired;
    public bool IsDead => isDead;

    public Action<BaseBoss> OnBossDefeated;

    protected virtual void Start()
    {
    }

    protected void FinishPhase()
    {
        phaseCount++;
        if (phaseCount >= maxPhaseCycles)
        {
            EnterTiredState();
        }
    }

    protected void EnterTiredState()
    {
        isTired = true;
        attackCycleCount++;
        Debug.Log($"{bossName} is tired! Cycle {attackCycleCount}/{maxAttackCycles}");

    }

    public virtual void WakeUp()
    {
        if (isDead) return;

        if (attackCycleCount >= maxAttackCycles)
        {
            Debug.Log($"{bossName} collapses after final cycle!");
            Defeated();
        }
        else
        {
            Debug.Log($"{bossName} wakes up!");
            isTired = false;
            phaseCount = 0;

            StartBossPhases();
        }
    }

    protected virtual void Defeated()
    {
        isDead = true;
        Debug.Log($"{bossName} dead!");
        OnBossDefeated?.Invoke(this);
    }

    protected abstract void StartBossPhases();
}
