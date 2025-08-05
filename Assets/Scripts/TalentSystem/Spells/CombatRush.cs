using UnityEngine;

/// <summary>
/// Movement pace spell that provides a brief speed boost when combat starts
/// </summary>
public class CombatRush : Ability
{
    private bool combatRushActive = false;
    private StatModifier rushSpeedMod;
    private float boostDuration = 3.0f;
    private float boostTimer = 0f;
    private float speedMultiplier = 1.5f;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Higher rarity = longer duration and more speed
        speedMultiplier = 1.5f * rarityScaling;
        boostDuration = 3.0f * rarityScaling;
        Debug.Log($"[CombatRush] rarityScaling applied: {rarityScaling}, speedMultiplier: {speedMultiplier}, duration: {boostDuration}");
    }

    private void Start()
    {
        // Subscribe to combat events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerHasAttacked += OnCombatStarted;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerHasAttacked -= OnCombatStarted;
        }
        
        // Remove any active modifiers
        EndCombatRush();
    }

    private void OnCombatStarted(float damage)
    {
        // Only trigger if not already active
        if (!combatRushActive)
        {
            TriggerCombatRush();
        }
    }

    private void TriggerCombatRush()
    {
        combatRushActive = true;
        boostTimer = boostDuration;
        
        var player = PlayerManager.instance.playerStats;
        rushSpeedMod = new StatModifier(speedMultiplier, StatModType.PercentMult, this);
        player.MovementSpeed.AddModifier(rushSpeedMod);
        
        Debug.Log($"[CombatRush] Combat rush activated! Speed boost: {speedMultiplier}x for {boostDuration}s");
        
        // Optional: Add visual/audio feedback
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_LightGlobe", player);
        }
    }

    private void EndCombatRush()
    {
        if (combatRushActive)
        {
            var player = PlayerManager.instance.playerStats;
            if (rushSpeedMod != null)
            {
                player.MovementSpeed.RemoveModifier(rushSpeedMod);
                rushSpeedMod = null;
            }
            
            combatRushActive = false;
            Debug.Log("[CombatRush] Combat rush ended");
        }
    }

    protected override void OnUpdateAbility()
    {
        if (combatRushActive)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                EndCombatRush();
            }
        }
    }

    // Required abstract implementations (not used for this passive spell)
    public override void UseBase(IEntitie entitie)
    {
        // This is a passive spell, no direct use
    }

    public override void OnTick(IEntitie entitie)
    {
        // Not used for this spell
    }

    public override void OnCooldown(IEntitie entitie)
    {
        // Not used for this spell
    }
}