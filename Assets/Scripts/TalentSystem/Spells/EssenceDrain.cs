using UnityEngine;

public class EssenceDrain : Ability
{
    private float totalDamage = 0f;

    public override void UseBase(IEntitie entitie)
    {
        totalDamage = 0f; // Reset bei jedem Start
        Debug.Log("[EssenceDrain] UseBase: totalDamage reset.");
    }

    public override void OnTick(IEntitie entitie)
    {
        float radius = areaOfEffectRadius;
        //float apScaling = projectileSpeed;

        float playerAP = PlayerManager.instance.playerStats.AbilityPower.Value;
        Debug.Log($"[EssenceDrain] OnTick: radius={radius}, playerAP={playerAP}");

        Collider[] hits = Physics.OverlapSphere(entitie.GetTransform().position, radius);
        Debug.Log($"[EssenceDrain] OnTick: {hits.Length} Collider im Umkreis gefunden.");

        int affectedEnemies = 0;
        float tickDamage = 1f;

        foreach (var col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyController enemy = col.GetComponentInParent<EnemyController>();
                if (enemy != null && !enemy.mobStats.isDead)
                {
                    float dmg = playerAP * 0.2f; // Beispielhafte Schadensberechnung, ggf. anpassen
                    enemy.TakeDirectDamage(dmg, radius); // 5 = DamageType, ggf. anpassen
                    tickDamage += dmg;
                    totalDamage += dmg;
                    affectedEnemies++;
                    Debug.Log($"[EssenceDrain] OnTick: {enemy.name} getroffen, Schaden: {dmg}");
                }
            }
        }

        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_EssenceDrain", PlayerManager.instance.playerStats);
            VFX_Manager.instance.PlayEffect("CFX_SwordTrail Void", PlayerManager.instance.playerStats);
        }
        Debug.Log($"[EssenceDrain] OnTick: {affectedEnemies} Gegner getroffen, Tick-Schaden: {tickDamage}, totalDamage: {totalDamage}");
    }

    public override void OnCooldown(IEntitie entitie)
    {
        float healAmount = totalDamage * 0.1f;
        entitie.Heal((int)healAmount);
        Debug.Log($"[EssenceDrain] OnCooldown: Spieler heilt um {healAmount} (10% von {totalDamage})");
        totalDamage = 0f;
    }

        protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Hier MUSS die Kindklasse den Wert verwenden!
        Debug.Log($"[EssenceDrain] rarityScaling angewendet: {rarityScaling}");
        // Beispiel: areaOfEffectRadius *= rarityScaling;
        //           projectileSpeed *= rarityScaling;
    }
}
