using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class BossOne : BaseBoss
{

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnterState(BossState newState)
    {
        switch (newState)
        {
            case BossState.Attack:
                Debug.Log($"{bossName} prepares attack!");
                break;
            case BossState.Evade:
                Debug.Log($"{bossName} evades!");
                break;
            case BossState.Move:
                Debug.Log($"{bossName} repositions!");
                break;
        }
    }

    protected override void HandleState()
    {
        switch (currentState)
        {
            case BossState.Attack:
                Attack();
                break;

            case BossState.Evade:
                Evade();
                break;

            case BossState.Move:
                Move();
                break;
        }
    }

    void Attack()
    {

    }

    void Evade()
    {

    }

    void Move()
    {
        
    }
}
