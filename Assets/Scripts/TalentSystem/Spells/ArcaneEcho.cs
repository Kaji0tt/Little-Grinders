using UnityEngine;

/// <summary>
/// AP pace spell that increases ability power based on number of abilities used recently
/// </summary>
public class ArcaneEcho : Ability
{
    private StatModifier apBoostMod;
    private float apBoostPerCast = 0.15f; // 15% AP boost per recent cast
    private int maxStacks = 5;
    private int currentStacks = 0;
    private float stackDuration = 6.0f;
    private float stackTimer = 0f;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        apBoostPerCast = 0.15f * rarityScaling;
        maxStacks = Mathf.RoundToInt(5 * rarityScaling);
        stackDuration = 6.0f * rarityScaling;
        Debug.Log($"[ArcaneEcho] rarityScaling applied: {rarityScaling}, boostPerCast: {apBoostPerCast}, maxStacks: {maxStacks}");
    }

    private void Start()
    {
        // Subscribe to spell cast events
        // Note: This assumes there's a way to detect when abilities are used
        // You might need to modify the Ability base class to fire events when abilities are used
        
        // For now, we'll check periodically for ability usage
        InvokeRepeating(nameof(CheckForAbilityUsage), 0.5f, 0.5f);
    }

    private void OnDestroy()
    {
        CancelInvoke();
        RemoveAPBoost();
    }

    private void CheckForAbilityUsage()
    {
        // This is a simplified implementation
        // In a full implementation, you'd want to track actual ability usage
        
        // For demonstration, we'll trigger on spell key presses
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || 
            Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.T))
        {
            OnAbilityUsed();
        }
    }

    public void OnAbilityUsed()
    {
        if (currentStacks < maxStacks)
        {
            currentStacks++;
            stackTimer = stackDuration;
            
            UpdateAPBoost();
            
            Debug.Log($"[ArcaneEcho] Ability used! Stacks: {currentStacks}/{maxStacks}");
            
            // Visual feedback
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("CFX_LightHit", PlayerManager.instance.playerStats);
            }
        }
        else
        {
            // Refresh duration at max stacks
            stackTimer = stackDuration;
        }
    }

    private void UpdateAPBoost()
    {
        RemoveAPBoost();
        
        if (currentStacks > 0)
        {
            float totalBoost = apBoostPerCast * currentStacks;
            var player = PlayerManager.instance.playerStats;
            apBoostMod = new StatModifier(totalBoost, StatModType.PercentAdd, this);
            player.AbilityPower.AddModifier(apBoostMod);
            
            Debug.Log($"[ArcaneEcho] AP boost updated: +{totalBoost * 100}% ({currentStacks} stacks)");
        }
    }

    private void RemoveAPBoost()
    {
        if (apBoostMod != null)
        {
            var player = PlayerManager.instance.playerStats;
            player.AbilityPower.RemoveModifier(apBoostMod);
            apBoostMod = null;
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
                RemoveAPBoost();
                Debug.Log("[ArcaneEcho] Stacks expired");
            }
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