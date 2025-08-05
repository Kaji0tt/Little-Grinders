using UnityEngine;

/// <summary>
/// Movement pace spell that provides speed boost after dodging attacks
/// </summary>
public class EvasionBoost : Ability
{
    private StatModifier evasionSpeedMod;
    private float dodgeSpeedMultiplier = 2.0f;
    private float dodgeBoostDuration = 2.0f;
    private float dodgeTimer = 0f;
    private bool dodgeBoostActive = false;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        dodgeSpeedMultiplier = 2.0f * rarityScaling;
        dodgeBoostDuration = 2.0f * rarityScaling;
        Debug.Log($"[EvasionBoost] rarityScaling applied: {rarityScaling}, speedMultiplier: {dodgeSpeedMultiplier}");
    }

    private void Start()
    {
        // Subscribe to damage events to detect when player takes damage
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerWasAttacked += OnPlayerDamaged;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerWasAttacked -= OnPlayerDamaged;
        }
        
        EndEvasionBoost();
    }

    private void OnPlayerDamaged(float damage)
    {
        // Small chance to trigger evasion boost when taking damage (represents "dodging")
        if (Random.value < 0.3f) // 30% chance to trigger
        {
            TriggerEvasionBoost();
        }
    }

    private void TriggerEvasionBoost()
    {
        // End previous boost if active
        EndEvasionBoost();
        
        dodgeBoostActive = true;
        dodgeTimer = dodgeBoostDuration;
        
        var player = PlayerManager.instance.playerStats;
        evasionSpeedMod = new StatModifier(dodgeSpeedMultiplier, StatModType.PercentMult, this);
        player.MovementSpeed.AddModifier(evasionSpeedMod);
        
        Debug.Log($"[EvasionBoost] Evasion boost triggered! Speed: {dodgeSpeedMultiplier}x for {dodgeBoostDuration}s");
        
        // Visual feedback
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_Vanish", player);
        }
    }

    private void EndEvasionBoost()
    {
        if (dodgeBoostActive)
        {
            var player = PlayerManager.instance.playerStats;
            if (evasionSpeedMod != null)
            {
                player.MovementSpeed.RemoveModifier(evasionSpeedMod);
                evasionSpeedMod = null;
            }
            
            dodgeBoostActive = false;
            Debug.Log("[EvasionBoost] Evasion boost ended");
        }
    }

    protected override void OnUpdateAbility()
    {
        if (dodgeBoostActive)
        {
            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0f)
            {
                EndEvasionBoost();
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