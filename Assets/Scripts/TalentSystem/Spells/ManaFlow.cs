using UnityEngine;

/// <summary>
/// AP pace spell that reduces ability costs and provides mana-like resource management
/// </summary>
public class ManaFlow : Ability
{
    private float currentManaPool = 100f;
    private float maxManaPool = 100f;
    private float manaRegenRate = 10f; // Mana per second
    private float abilityCostReduction = 0.3f; // 30% cost reduction
    private bool manaFlowActive = true;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        maxManaPool = 100f * rarityScaling;
        currentManaPool = maxManaPool;
        manaRegenRate = 10f * rarityScaling;
        abilityCostReduction = 0.3f * rarityScaling;
        Debug.Log($"[ManaFlow] rarityScaling applied: {rarityScaling}, maxMana: {maxManaPool}, regenRate: {manaRegenRate}");
    }

    private void Start()
    {
        currentManaPool = maxManaPool;
        
        // Subscribe to ability usage events
        if (GameEvents.Instance != null)
        {
            // Assuming there are events for ability usage
            // GameEvents.Instance.OnAbilityUsed += OnAbilityUsed;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            // GameEvents.Instance.OnAbilityUsed -= OnAbilityUsed;
        }
    }

    protected override void OnUpdateAbility()
    {
        if (manaFlowActive)
        {
            // Regenerate mana over time
            RegenerateMana();
            
            // Provide AP boost based on current mana percentage
            UpdateAPBoostFromMana();
        }
    }

    private void RegenerateMana()
    {
        if (currentManaPool < maxManaPool)
        {
            float regenAmount = manaRegenRate * Time.deltaTime;
            currentManaPool = Mathf.Min(currentManaPool + regenAmount, maxManaPool);
        }
    }

    private void UpdateAPBoostFromMana()
    {
        // Higher mana = more AP (up to 25% bonus at full mana)
        float manaPercentage = currentManaPool / maxManaPool;
        float apBonus = manaPercentage * 0.25f; // 25% max bonus
        
        // Note: In a full implementation, you'd apply this as a dynamic modifier
        // For now, we'll just log the bonus
        if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Debug.Log($"[ManaFlow] Mana: {currentManaPool:F0}/{maxManaPool:F0} ({manaPercentage:P0}) - AP Bonus: {apBonus:P0}");
        }
    }

    public bool TryConsumeManaCost(float baseCost)
    {
        float actualCost = baseCost * (1f - abilityCostReduction);
        
        if (currentManaPool >= actualCost)
        {
            currentManaPool -= actualCost;
            Debug.Log($"[ManaFlow] Consumed {actualCost:F1} mana (reduced from {baseCost:F1}). Remaining: {currentManaPool:F1}");
            return true;
        }
        
        Debug.Log($"[ManaFlow] Insufficient mana! Need {actualCost:F1}, have {currentManaPool:F1}");
        return false;
    }

    public void RestoreMana(float amount)
    {
        currentManaPool = Mathf.Min(currentManaPool + amount, maxManaPool);
        Debug.Log($"[ManaFlow] Restored {amount:F1} mana. Current: {currentManaPool:F1}");
    }

    public float GetManaPercentage()
    {
        return currentManaPool / maxManaPool;
    }

    public float GetCurrentMana()
    {
        return currentManaPool;
    }

    public float GetMaxMana()
    {
        return maxManaPool;
    }

    // Trigger mana restoration on enemy kills
    public void OnEnemyKilled()
    {
        float manaRestore = maxManaPool * 0.2f; // 20% mana on kill
        RestoreMana(manaRestore);
        
        // Visual feedback
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_LightHit", PlayerManager.instance.playerStats);
        }
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