using UnityEngine;

public class Voidwither : Ability
{
    [Range(0f, 1f)]
    public float abilityPowerPercent = 0.4f; // 40% der AbilityPower als Bonus-Schaden

    private PlayerStats playerStats;

    public override void UseBase(IEntitie entitie)
    {
        playerStats = entitie as PlayerStats;

        if (playerStats == null)
        {
            // Debug.LogWarning("Voidwither konnte keine PlayerStats referenzieren.");
            return;
        }

        // Event abonnieren
        GameEvents.Instance.OnEnemyWasAttacked += ApplyVoidwitherEffect;

        // Spiele Start-Effekt ab
        if (VFX_Manager.instance == null)
        {
            Debug.LogWarning("VFX_Manager ist nicht initialisiert. Kein Effekt wird abgespielt.");
            return;
        }
        VFX_Manager.instance?.PlayEffect("CFX_VoidwitherStart", playerStats);
        // Debug.Log("Voidwither aktiviert.");
    }

    public override void OnTick(IEntitie entitie)
    {
        // Kein Tick benötigt
    }

    public override void OnCooldown(IEntitie entitie)
    {
        // Event abbestellen
        GameEvents.Instance.OnEnemyWasAttacked -= ApplyVoidwitherEffect;

        // Debug.Log("Voidwither deaktiviert.");
    }

    private void ApplyVoidwitherEffect(float baseDamage, Transform enemyTransform, bool crit)
    {
        if (playerStats == null || enemyTransform == null)
            return;

        IEntitie target = enemyTransform.GetComponent<IEntitie>();
        if (target == null)
            return;

        float bonusDamage = playerStats.AbilityPower.Value * abilityPowerPercent;
        target.TakeDirectDamage(bonusDamage, playerStats.Range);

        // Optional: FX anzeigen
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_SwordHitVoid", enemyTransform.GetComponent<EnemyController>());
        }
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound("Wurf_05");
        }
        // Debug.Log($"Voidwither verursacht {bonusDamage} magischen Zusatzschaden an {enemyTransform.name}");
    }
    
        protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Hier MUSS die Kindklasse den Wert verwenden!
        Debug.Log($"[Voidwiether] rarityScaling angewendet: {rarityScaling}");
        // Beispiel: areaOfEffectRadius *= rarityScaling;
        //           projectileSpeed *= rarityScaling;
    }
}
