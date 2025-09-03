using UnityEngine;

public class BossTwo : BaseBoss
{

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void OnEnterState(BossState newState)
    {
        switch (newState)
        {
            case BossState.Attack:
                Debug.Log($"{bossName} prepares attack!");
                break;
            case BossState.Evade:
                break;
            case BossState.Move:
                break;
        }
    }

    protected override void HandleState() 
    {
        switch (currentState)
        {
            case BossState.Attack:
                break;
            case BossState.Evade:
                break;
            case BossState.Move:
                break;
        }
    }
}
