using UnityEngine;

/// <summary>
/// Utility pace spell that provides temporary resource regeneration boost during combat
/// </summary>
public class BattleTrance : Ability
{
    private StatModifier regenMod;
    private float regenBoostMultiplier = 3.0f;
    private float tranceDuration = 8.0f;
    private float combatCheckInterval = 1.0f;
    private bool inTrance = false;
    private float lastCombatTime = 0f;
    private float combatTimeout = 5.0f; // Seconds after last combat action to end trance

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        regenBoostMultiplier = 3.0f * rarityScaling;
        tranceDuration = 8.0f * rarityScaling;
        Debug.Log($"[BattleTrance] rarityScaling applied: {rarityScaling}, regenMultiplier: {regenBoostMultiplier}");
    }

    private void Start()
    {
        // Subscribe to combat events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerHasAttacked += OnCombatAction;
            GameEvents.Instance.OnPlayerWasAttacked += OnPlayerDamaged;
        }
        
        // Check combat status periodically
        InvokeRepeating(nameof(CheckCombatStatus), combatCheckInterval, combatCheckInterval);
    }

    private void OnDestroy()
    {
        CancelInvoke();
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerHasAttacked -= OnCombatAction;
            GameEvents.Instance.OnPlayerWasAttacked -= OnPlayerDamaged;
        }
        
        EndTrance();
    }

    private void OnCombatAction(float damage)
    {
        lastCombatTime = Time.time;
        
        if (!inTrance)
        {
            StartTrance();
        }
    }

    private void OnPlayerDamaged(float damage)
    {
        lastCombatTime = Time.time;
        
        if (!inTrance)
        {
            StartTrance();
        }
    }

    private void CheckCombatStatus()
    {
        if (inTrance)
        {
            // End trance if no combat for timeout period
            if (Time.time - lastCombatTime > combatTimeout)
            {
                EndTrance();
            }
        }
    }

    private void StartTrance()
    {
        if (inTrance) return;
        
        inTrance = true;
        
        var player = PlayerManager.instance.playerStats;
        regenMod = new StatModifier(regenBoostMultiplier, StatModType.PercentMult, this);
        player.Regeneration.AddModifier(regenMod);
        
        Debug.Log($"[BattleTrance] Battle trance started! Regeneration: {regenBoostMultiplier}x");
        
        // Visual feedback
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_LightGlobe", player);
        }
    }

    private void EndTrance()
    {
        if (!inTrance) return;
        
        var player = PlayerManager.instance.playerStats;
        if (regenMod != null)
        {
            player.Regeneration.RemoveModifier(regenMod);
            regenMod = null;
        }
        
        inTrance = false;
        Debug.Log("[BattleTrance] Battle trance ended");
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