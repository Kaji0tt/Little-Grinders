using UnityEngine;

public class VitalDischarge : Ability
{
    private float healthDrainPercent = 0.03f; // 3% des Lebens pro Sekunde (konstant)
    private float damageMultiplier = 1f; // Multiplikator für ausgeteilten Schaden
    
    public override void UseBase(IEntitie entitie)
    {
        // Diese Fähigkeit ist persistent/aktiv - die eigentliche Logik passiert in OnTick
        Debug.Log("[VitalDischarge] UseBase: Vital Discharge aktiviert");
    }

    public override void OnTick(IEntitie entitie)
    {
        // Berechne den Lebensentzug: 3% der maximalen Lebenspunkte pro Sekunde
        float maxHealth = entitie.Get_maxHp();
        float healthDrain = maxHealth * healthDrainPercent;
        
        // Stelle sicher, dass der Spieler nicht stirbt durch diese Fähigkeit
        float currentHealth = entitie.Get_currentHp();
        if (currentHealth <= healthDrain)
        {
            // Fähigkeit sofort abbrechen und in Cooldown schicken
            Debug.Log("[VitalDischarge] Abgebrochen - zu wenig Leben! Wechsel zu Cooldown.");
            
            // Setze den State direkt auf Cooldown und beende die aktive Phase
            state = AbilityState.Cooldown;
            
            // Spiele Abbruch-Effekt ab
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_VitalDischarge_Abort", entitie);
            }
            
            return;
        }
        
        // Entziehe dem Spieler Leben
        entitie.TakeDirectDamage(healthDrain, 999f); // Range 999 = immer treffen
        
        // Finde alle Feinde im Umkreis
        Collider[] hits = Physics.OverlapSphere(entitie.GetTransform().position, areaOfEffectRadius);
        
        // Zähle lebende Feinde
        var livingEnemies = new System.Collections.Generic.List<EnemyController>();
        foreach (var col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyController enemy = col.GetComponentInParent<EnemyController>();
                if (enemy != null && !enemy.mobStats.isDead)
                {
                    livingEnemies.Add(enemy);
                }
            }
        }
        
        // Verteile den Schaden gleichmäßig auf alle lebenden Feinde
        if (livingEnemies.Count > 0)
        {
            // Schaden wird durch damageMultiplier verstärkt, aber Lebensverbrauch bleibt konstant
            float totalDamageToDistribute = healthDrain * damageMultiplier;
            float damagePerEnemy = totalDamageToDistribute / livingEnemies.Count;
            
            foreach (var enemy in livingEnemies)
            {
                enemy.TakeDirectDamage(damagePerEnemy, areaOfEffectRadius);
                Debug.Log($"[VitalDischarge] {enemy.name} erleidet {damagePerEnemy} Schaden");
            }
            
            Debug.Log($"[VitalDischarge] {healthDrain} Leben entzogen, {totalDamageToDistribute} Schaden verteilt auf {livingEnemies.Count} Feinde als je {damagePerEnemy} Schaden (Multiplikator: {damageMultiplier})");
        }
        else
        {
            Debug.Log($"[VitalDischarge] {healthDrain} Leben entzogen, aber keine Feinde im Umkreis!");
        }
        
        // Spieleffekte
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_EssenceDrain", entitie);
        }
    }

    public override void OnCooldown(IEntitie entitie)
    {
        Debug.Log("[VitalDischarge] OnCooldown: Vital Discharge beendet");
    }

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Rarity Scaling: Nur der ausgeteilte Schaden wird multipliziert, nicht der Lebensverbrauch
        damageMultiplier = rarityScaling;
        
        Debug.Log($"[VitalDischarge] Rarity Scaling angewendet: {rarityScaling}, Schadens-Multiplikator: {damageMultiplier}");
    }
}
