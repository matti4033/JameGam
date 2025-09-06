using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Starting")] public int startingHP = 3;

    [Header("Limits")] public bool capToMax = false; // if true, current HP will never exceed maxHP
    public int maxHP = 999; // only used when capToMax = true

    [Header("Events")] public UnityEvent<int> onHealthChanged; // passes current HP
    public UnityEvent onDeath;
    
    public bool IsDead => CurrentHP <= 0;
    bool _deathFired = false;

    [Header("Regen")] [Tooltip("Enable timed regeneration.")]
    public bool regenEnabled = true;

    [Tooltip("How much HP to restore per tick.")]
    public int regenAmount = 1;

    [Tooltip("Seconds between regen ticks once regen is active.")]
    public float regenTickInterval = 2f;

    [Tooltip("Wait this many seconds since LAST damage before regen starts.")]
    public float regenDelayAfterDamage = 3f;

    [Tooltip("If true, auto-heal stops at startingHP. If false, it can exceed startingHP (and will be clamped by maxHP when capToMax is true).")]
    public bool regenOnlyUpToStartingHP = true;

    public int CurrentHP { get; private set; }

    float lastDamageTime;
    Coroutine regenRoutine;

    void Awake()
    {
        CurrentHP = Mathf.Max(1, startingHP);
        lastDamageTime = Time.time;
        onHealthChanged?.Invoke(CurrentHP);
    }

    void OnEnable()
    {
        StartRegenLoopIfNeeded();
    }

    void OnDisable()
    {
        if (regenRoutine != null)
        {
            StopCoroutine(regenRoutine);
            regenRoutine = null;
        }
    }

    private void Update()
    {
        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    public void Damage(int amount)
    {
        if (amount <= 0) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        lastDamageTime = Time.time; // reset regen cooldown
        onHealthChanged?.Invoke(CurrentHP);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        int target = CurrentHP + amount;
        if (capToMax) target = Mathf.Min(target, maxHP);
        CurrentHP = target;
        onHealthChanged?.Invoke(CurrentHP);
    }

    // Call this when the boss spawns, adds +2 to current health
    public void ApplyBossBonus(int bonus = 2)
    {
        Heal(bonus);
    }

    void StartRegenLoopIfNeeded()
    {
        if (!regenEnabled) return;
        if (regenRoutine == null) regenRoutine = StartCoroutine(RegenLoop());
    }

    IEnumerator RegenLoop()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.01f, regenTickInterval));

        while (true)
        {
            yield return wait;

            if (!regenEnabled)
                continue;

            // cooldown since last damage
            if (Time.time - lastDamageTime < regenDelayAfterDamage)
                continue;

            // determine ceiling for auto-heal
            int ceiling = int.MaxValue;
            if (regenOnlyUpToStartingHP)
                ceiling = startingHP;
            else if (capToMax)
                ceiling = maxHP;

            if (CurrentHP >= ceiling)
                continue;

            int amount = regenAmount;
            if (ceiling != int.MaxValue)
                amount = Mathf.Min(amount, ceiling - CurrentHP);

            if (amount > 0)
                Heal(amount);
        }
    }

    public void Die()
    {
        //if (_deathFired) return;
        //_deathFired = true;
        Debug.Log("[PlayerHealth] Player died.");
        //onDeath?.Invoke(); // hook respawn in Inspector (or in code)

        GameManager.Instance.gameOverMenu.SetActive(true);
        Time.timeScale = 0;
    }

    // Inspector fix
    void OnValidate()
    {
        startingHP = Mathf.Max(1, startingHP);
        maxHP = Mathf.Max(1, maxHP);
        regenAmount = Mathf.Max(1, regenAmount);
        regenTickInterval = Mathf.Max(0.01f, regenTickInterval);
        regenDelayAfterDamage = Mathf.Max(0f, regenDelayAfterDamage);
    }
}