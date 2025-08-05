using UnityEngine;

/// <summary>
/// Utility pace spell that reduces all cooldowns when enemies are defeated
/// </summary>
public class VictoryMomentum : Ability
{
    private float cooldownReduction = 0.2f; // 20% cooldown reduction per kill
    private float momentumDuration = 5.0f;
    private float momentumTimer = 0f;
    private bool momentumActive = false;
    private int killStack = 0;
    private int maxStacks = 5;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        cooldownReduction = 0.2f * rarityScaling;
        maxStacks = Mathf.RoundToInt(5 * rarityScaling);
        Debug.Log($"[VictoryMomentum] rarityScaling applied: {rarityScaling}, reduction: {cooldownReduction}, maxStacks: {maxStacks}");
    }

    private void Start()
    {
        // Subscribe to enemy death events
        // Note: Assuming there's an event for enemy deaths - may need to implement
        if (GameEvents.Instance != null)
        {
            // GameEvents.Instance.OnEnemyKilled += OnEnemyKilled;
        }
        
        // Alternative: Check for dead enemies in radius periodically
        InvokeRepeating(nameof(CheckForKills), 1f, 0.5f);
    }

    private void OnDestroy()
    {
        CancelInvoke();
        if (GameEvents.Instance != null)
        {
            // GameEvents.Instance.OnEnemyKilled -= OnEnemyKilled;
        }
    }

    private void CheckForKills()
    {
        // Check for recently killed enemies in area
        Collider[] enemies = Physics.OverlapSphere(transform.position, 10f);
        foreach (var collider in enemies)
        {
            if (collider.CompareTag("Enemy"))
            {
                var enemy = collider.GetComponentInParent<EnemyController>();
                if (enemy != null && enemy.mobStats.isDead)
                {
                    // Trigger momentum if enemy died recently
                    OnEnemyKilled();
                    break; // Only one kill trigger per check
                }
            }
        }
    }

    private void OnEnemyKilled()
    {
        if (killStack < maxStacks)
        {
            killStack++;
            RefreshMomentum();
            
            Debug.Log($"[VictoryMomentum] Kill registered! Stack: {killStack}/{maxStacks}");
            
            // Visual feedback
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("CFX_LightHit", PlayerManager.instance.playerStats);
            }
        }
    }

    private void RefreshMomentum()
    {
        momentumActive = true;
        momentumTimer = momentumDuration;
        
        // Apply cooldown reduction to all player abilities
        ApplyCooldownReduction();
    }

    private void ApplyCooldownReduction()
    {
        // Reduce cooldowns for all abilities
        // This would need to access the player's ability manager
        float totalReduction = cooldownReduction * killStack;
        
        Debug.Log($"[VictoryMomentum] Applying {totalReduction * 100}% cooldown reduction");
        
        // Note: This implementation assumes there's a way to access and modify ability cooldowns
        // You might need to implement this based on how abilities are managed in the game
    }

    protected override void OnUpdateAbility()
    {
        if (momentumActive)
        {
            momentumTimer -= Time.deltaTime;
            if (momentumTimer <= 0f)
            {
                killStack = 0;
                momentumActive = false;
                Debug.Log("[VictoryMomentum] Momentum expired, stacks reset");
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