using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Kettenschlag : Ability
{
    [Header("Kettenschlag Settings")]
    [SerializeField] private float baseDamageMultiplier = .5f;   // 50% AD für ersten Hit
    [SerializeField] private float chainDistance = 1.5f;         // Maximaler Abstand zwischen Sprüngen
    [SerializeField] private int baseMaxChains = 5;              // Basis-Anzahl der Sprünge
    [SerializeField] private float damageReductionPerChain = 0.5f; // Schaden halbiert sich pro Sprung
    
    // Rarity Scaling Werte
    private float currentDamageMultiplier = 1.0f;                // Aktueller Schadens-Multiplikator
    private int currentMaxChains = 5;                            // Aktuelle maximale Sprunganzahl
    
    public override void UseBase(IEntitie entitie)
    {
        // Hole das aktuelle Target vom TargetSystem
        CharacterCombat combat = PlayerManager.instance.player.GetComponent<CharacterCombat>();
        EnemyController initialTarget = combat.currentTarget;
        
        if (initialTarget == null || initialTarget.mobStats.isDead)
        {
            Debug.Log("[Kettenschlag] Kein gültiges Target gefunden!");
            return;
        }
        
        // Starte die Ketten-Sequenz
        StartCoroutine(ExecuteChainAttack(initialTarget, entitie));
        
        Debug.Log($"[Kettenschlag] Gestartet auf {initialTarget.name}");
    }
    
    public override void OnTick(IEntitie entitie)
    {
        // Kettenschlag ist eine Instant-Fähigkeit, kein Tick nötig
    }
    
    public override void OnCooldown(IEntitie entitie)
    {
        // Kettenschlag ist eine Instant-Fähigkeit, kein Cooldown-Verhalten nötig
    }
    
    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Schadens-Multiplikator: Rarity 2 = 200% AD, Rarity 3 = 300% AD, etc.
        currentDamageMultiplier = baseDamageMultiplier * rarityScaling;
        
        // Zusätzliche Sprünge: Rarity 2 = +1 Sprung, Rarity 3 = +2 Sprünge, etc.
        int bonusChains = Mathf.RoundToInt(rarityScaling - 1); // Rarity 1 = +0, Rarity 2 = +1, etc.
        currentMaxChains = baseMaxChains + bonusChains;
        
        Debug.Log($"[Kettenschlag] Rarity Scaling angewendet: {rarityScaling} - Schaden: {(currentDamageMultiplier * 100):F0}% AD, Max Sprünge: {currentMaxChains}");
    }
    
    /// <summary>
    /// Führt die komplette Ketten-Attacke aus
    /// </summary>
    private IEnumerator ExecuteChainAttack(EnemyController initialTarget, IEntitie caster)
    {
        PlayerStats playerStats = PlayerManager.instance.playerStats;
        float baseAttackPower = playerStats.AttackPower.Value;
        
        EnemyController currentTarget = initialTarget;
        float currentDamageMultiplier = this.currentDamageMultiplier; // Startwert
        int totalHits = 0; // Zähler für Debug-Ausgaben
        
        for (int chainIndex = 0; chainIndex < currentMaxChains && currentTarget != null; chainIndex++)
        {
            // Berechne Schaden für diesen Sprung
            float damage = baseAttackPower * currentDamageMultiplier;
            
            // Berechne kritischen Treffer
            bool isCrit = Random.value < playerStats.CriticalChance.Value;
            if (isCrit)
                damage *= playerStats.CriticalDamage.Value; // Damange wird mit dem kritischen Schadenswert des Spielers multipliziert
            
            // Verursache Schaden
            currentTarget.TakeDamage(damage, 10, isCrit);
            totalHits++;
            
            Debug.Log($"[Kettenschlag] Sprung {chainIndex + 1}: {currentTarget.name} nimmt {damage} Schaden ({(currentDamageMultiplier * 100):F0}% AD) - Crit: {isCrit}");
            
            // Visueller Effekt für jeden Sprung
            if (VFX_Manager.instance != null)
            {
                if (chainIndex == 0)
                    VFX_Manager.instance.PlayEffect("VFX_Kettenschlag_Initial", currentTarget);
                else
                    VFX_Manager.instance.PlayEffect("VFX_Kettenschlag_Chain", currentTarget);
            }
            
            // Finde nächstes Target für den nächsten Sprung (muss ein ANDERER Gegner sein!)
            EnemyController nextTarget = FindNextChainTarget(currentTarget);
            
            // Wenn kein anderes Target gefunden wurde, brich die Kette ab
            if (nextTarget == null)
            {
                Debug.Log($"[Kettenschlag] Kette abgebrochen - kein anderes Target in Reichweite von {currentTarget.name}");
                break;
            }
            
            // Wenn es ein nächstes Target gibt, zeige Verbindungseffekt
            if (chainIndex < currentMaxChains - 1)
            {
                // Visueller Effekt für Ketten-Verbindung
                if (VFX_Manager.instance != null)
                {
                    // Hier könnte ein Line-Renderer oder Beam-Effekt zwischen currentTarget und nextTarget gezeigt werden
                    VFX_Manager.instance.PlayEffect("VFX_Kettenschlag_Link", currentTarget);
                }
                
                // Kurze Pause zwischen Sprüngen für visuellen Effekt
                yield return new WaitForSeconds(0.15f);
            }
            
            // Bereite nächsten Sprung vor
            currentTarget = nextTarget;
            currentDamageMultiplier *= damageReductionPerChain; // Halbiere den Schaden für nächsten Sprung
        }
        
        Debug.Log($"[Kettenschlag] Beendet nach {totalHits} Sprüngen");
        
        // Abschluss-Effekt
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_Kettenschlag_Finish", caster);
        }
    }
    
    /// <summary>
    /// Findet das nächste Target für den Ketten-Sprung (muss ein ANDERER Gegner sein!)
    /// </summary>
    private EnemyController FindNextChainTarget(EnemyController currentTarget)
    {
        Vector3 currentPosition = currentTarget.transform.position;
        
        // Finde alle Gegner im Sprung-Radius
        Collider[] nearbyColliders = Physics.OverlapSphere(currentPosition, chainDistance);
        List<EnemyController> potentialTargets = new List<EnemyController>();
        
        foreach (var collider in nearbyColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyController enemy = collider.GetComponentInParent<EnemyController>();
                if (enemy != null && !enemy.mobStats.isDead && enemy != currentTarget) // WICHTIG: enemy != currentTarget!
                {
                    potentialTargets.Add(enemy);
                }
            }
        }
        
        // Wenn keine anderen Targets gefunden wurden
        if (potentialTargets.Count == 0)
        {
            return null; // Kette wird abgebrochen
        }
        
        // Wähle das nächstgelegene Target (das nicht das aktuelle Target ist)
        EnemyController closestTarget = null;
        float closestDistance = float.MaxValue;
        
        foreach (var target in potentialTargets)
        {
            float distance = Vector3.Distance(currentPosition, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        
        return closestTarget;
    }
    
    /// <summary>
    /// Hilfsmethode für Debug-Visualization im Editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Zeige Chain-Distance als Gizmo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chainDistance);
    }
}
