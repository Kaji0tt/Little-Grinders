using UnityEngine;

/// <summary>
/// AD pace spell that increases attack speed with consecutive hits
/// </summary>
public class BloodFrenzy : Ability
{
    private StatModifier attackSpeedMod;
    private float attackSpeedBoostPerHit = 0.1f; // 10% attack speed per hit
    private int maxStacks = 6;
    private int currentStacks = 0;
    private float stackDuration = 4.0f;
    private float stackTimer = 0f;
    private bool frenzyActive = false;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        attackSpeedBoostPerHit = 0.1f * rarityScaling;
        maxStacks = Mathf.RoundToInt(6 * rarityScaling);
        stackDuration = 4.0f * rarityScaling;
        Debug.Log($"[BloodFrenzy] rarityScaling applied: {rarityScaling}, boostPerHit: {attackSpeedBoostPerHit}, maxStacks: {maxStacks}");
    }

    private void Start()
    {
        // Subscribe to attack events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerHasAttacked += OnPlayerAttacked;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerHasAttacked -= OnPlayerAttacked;
        }
        
        RemoveFrenzyBoost();
    }

    private void OnPlayerAttacked(float damage)
    {
        if (currentStacks < maxStacks)
        {
            currentStacks++;
            stackTimer = stackDuration;
            
            UpdateFrenzyBoost();
            
            Debug.Log($"[BloodFrenzy] Hit registered! Stacks: {currentStacks}/{maxStacks}");
            
            // Visual feedback with increasing intensity
            if (VFX_Manager.instance != null)
            {
                if (currentStacks >= maxStacks)
                {
                    VFX_Manager.instance.PlayEffect("CFX_BloodSplash", PlayerManager.instance.playerStats);
                }
                else
                {
                    VFX_Manager.instance.PlayEffect("CFX_Strike", PlayerManager.instance.playerStats);
                }
            }
        }
        else
        {
            // Refresh duration at max stacks
            stackTimer = stackDuration;
        }
    }

    private void UpdateFrenzyBoost()
    {
        RemoveFrenzyBoost();
        
        if (currentStacks > 0)
        {
            float totalBoost = attackSpeedBoostPerHit * currentStacks;
            var player = PlayerManager.instance.playerStats;
            attackSpeedMod = new StatModifier(totalBoost, StatModType.PercentAdd, this);
            player.AttackSpeed.AddModifier(attackSpeedMod);
            
            frenzyActive = true;
            
            Debug.Log($"[BloodFrenzy] Attack speed boost updated: +{totalBoost * 100}% ({currentStacks} stacks)");
        }
    }

    private void RemoveFrenzyBoost()
    {
        if (attackSpeedMod != null)
        {
            var player = PlayerManager.instance.playerStats;
            player.AttackSpeed.RemoveModifier(attackSpeedMod);
            attackSpeedMod = null;
            frenzyActive = false;
        }
    }

    protected override void OnUpdateAbility()
    {
        if (currentStacks > 0)
        {
            stackTimer -= Time.deltaTime;
            if (stackTimer <= 0f)
            {
                currentStacks = 0;
                RemoveFrenzyBoost();
                Debug.Log("[BloodFrenzy] Frenzy stacks expired");
            }
        }
    }

    public int GetCurrentStacks()
    {
        return currentStacks;
    }

    public int GetMaxStacks()
    {
        return maxStacks;
    }

    public bool IsAtMaxFrenzy()
    {
        return currentStacks >= maxStacks;
    }

    // Required abstract implementations
    public override void UseBase(IEntitie entitie)
    {
        // Passive spell
    }

    public override void OnTick(IEntitie entitie)
    {
        // Not used
    }

    public override void OnCooldown(IEntitie entitie)
    {
        // Not used
    }
}