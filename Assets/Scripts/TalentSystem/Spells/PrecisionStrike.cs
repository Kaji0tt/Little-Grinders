using UnityEngine;

/// <summary>
/// AD pace spell that provides critical hit chance bonus and attack damage scaling
/// </summary>
public class PrecisionStrike : Ability
{
    private StatModifier critChanceMod;
    private StatModifier critDamageMod;
    private float baseCritChanceBonus = 0.2f; // 20% crit chance bonus
    private float baseCritDamageBonus = 0.5f; // 50% crit damage bonus
    private float comboMultiplier = 1.0f;
    private int consecutiveCrits = 0;
    private int maxComboStacks = 3;
    private float comboResetTime = 3.0f;
    private float comboTimer = 0f;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        baseCritChanceBonus = 0.2f * rarityScaling;
        baseCritDamageBonus = 0.5f * rarityScaling;
        maxComboStacks = Mathf.RoundToInt(3 * rarityScaling);
        Debug.Log($"[PrecisionStrike] rarityScaling applied: {rarityScaling}, critChance: {baseCritChanceBonus}, critDamage: {baseCritDamageBonus}");
    }

    private void Start()
    {
        // Apply base precision bonuses
        ApplyPrecisionBonuses();
        
        // Subscribe to attack events to track critical hits
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
        
        RemovePrecisionBonuses();
    }

    private void ApplyPrecisionBonuses()
    {
        var player = PlayerManager.instance.playerStats;
        
        // Apply crit chance bonus
        critChanceMod = new StatModifier(baseCritChanceBonus, StatModType.PercentAdd, this);
        player.CriticalChance.AddModifier(critChanceMod);
        
        // Apply crit damage bonus (scales with combo)
        float totalCritDamageBonus = baseCritDamageBonus * (1 + comboMultiplier);
        critDamageMod = new StatModifier(totalCritDamageBonus, StatModType.PercentAdd, this);
        player.CriticalDamage.AddModifier(critDamageMod);
        
        Debug.Log($"[PrecisionStrike] Precision bonuses applied: {baseCritChanceBonus * 100}% crit chance, {totalCritDamageBonus * 100}% crit damage");
    }

    private void RemovePrecisionBonuses()
    {
        var player = PlayerManager.instance.playerStats;
        
        if (critChanceMod != null)
        {
            player.CriticalChance.RemoveModifier(critChanceMod);
            critChanceMod = null;
        }
        
        if (critDamageMod != null)
        {
            player.CriticalDamage.RemoveModifier(critDamageMod);
            critDamageMod = null;
        }
    }

    private void OnPlayerAttacked(float damage)
    {
        // Check if this was a critical hit
        // Note: This is a simplified check - in a full implementation, 
        // you'd want the attack system to provide this information
        bool wasCritical = CheckIfCriticalHit();
        
        if (wasCritical)
        {
            OnCriticalHit();
        }
        else
        {
            ResetCombo();
        }
    }

    private bool CheckIfCriticalHit()
    {
        // Simplified critical hit detection
        // In a full implementation, this would be provided by the combat system
        var player = PlayerManager.instance.playerStats;
        float critChance = player.CriticalChance.Value;
        return Random.value < critChance;
    }

    private void OnCriticalHit()
    {
        if (consecutiveCrits < maxComboStacks)
        {
            consecutiveCrits++;
            comboTimer = comboResetTime;
            
            // Update combo multiplier
            comboMultiplier = consecutiveCrits * 0.2f; // 20% bonus per consecutive crit
            
            // Reapply bonuses with new multiplier
            RemovePrecisionBonuses();
            ApplyPrecisionBonuses();
            
            Debug.Log($"[PrecisionStrike] Critical hit combo! Consecutive crits: {consecutiveCrits}/{maxComboStacks}");
            
            // Visual feedback for combo
            if (VFX_Manager.instance != null)
            {
                if (consecutiveCrits >= maxComboStacks)
                {
                    VFX_Manager.instance.PlayEffect("CFX_LightGlobe", PlayerManager.instance.playerStats);
                }
                else
                {
                    VFX_Manager.instance.PlayEffect("CFX_Strike", PlayerManager.instance.playerStats);
                }
            }
        }
        else
        {
            // Refresh timer at max combo
            comboTimer = comboResetTime;
        }
    }

    private void ResetCombo()
    {
        if (consecutiveCrits > 0)
        {
            consecutiveCrits = 0;
            comboMultiplier = 0f;
            
            // Reapply base bonuses without combo
            RemovePrecisionBonuses();
            ApplyPrecisionBonuses();
            
            Debug.Log("[PrecisionStrike] Combo reset");
        }
    }

    protected override void OnUpdateAbility()
    {
        if (consecutiveCrits > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public int GetConsecutiveCrits()
    {
        return consecutiveCrits;
    }

    public float GetComboMultiplier()
    {
        return comboMultiplier;
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