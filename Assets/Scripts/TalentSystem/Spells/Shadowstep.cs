using UnityEngine;

public class Shadowstep : Ability
{
    //private float range = 7f;
    private float weaponDamageMultiplier = 1f;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Beispiel: 1.0 = 100% Waffenschaden, 1.5 = 150% Waffenschaden
        weaponDamageMultiplier = rarityScaling;
        Debug.Log($"[Shadowstep] rarityScaling angewendet: {rarityScaling}, weaponDamageMultiplier: {weaponDamageMultiplier}");
    }

    public override void UseBase(IEntitie entitie)
    {
        // Hole das aktuelle Ziel aus dem CharacterCombat/TargetSystem
        var player = PlayerManager.instance.player;
        var combat = player.GetComponent<CharacterCombat>();
        var target = combat?.currentTarget;

        if (target == null || target.mobStats.isDead)
        {
            Debug.LogWarning("[Shadowstep] Kein gültiges Ziel gefunden!");
            return;
        }

        float distance = Vector3.Distance(player.transform.position, target.transform.position);
        if (distance > range)
        {
            Debug.LogWarning("[Shadowstep] Ziel ist zu weit entfernt!");
            return;
        }
        if(VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_Vanish", player.playerStats);
        }
        // Teleportiere den Spieler zum Ziel (leicht versetzt, damit nicht im Collider)
        Vector3 dirToTarget = (target.transform.position - player.transform.position).normalized;
        Vector3 teleportPos = target.transform.position - dirToTarget * .5f; // .5f = Abstand vor dem Gegner
        teleportPos.y = player.transform.position.y; // Behalte die aktuelle Höhe

        player.transform.position = teleportPos;

        // Waffenschaden zufügen (multipliziert mit Rarität)
        var playerStats = PlayerManager.instance.playerStats;
        //float baseWeaponDamage = playerStats.WeaponDamage.Value;
        float baseWeaponDamage = playerStats.AttackPower.Value; // Angenommen, AttackPower ist der Basiswaffenschaden
        float totalDamage = baseWeaponDamage * weaponDamageMultiplier;

        target.TakeDirectDamage(totalDamage, playerStats.Range);
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_Strike", target);
            VFX_Manager.instance.PlayEffect("CFX_SwordTrailP2", player.playerStats);
        }

        // Optional: FX/Sound
        //if (VFX_Manager.instance != null)
        //VFX_Manager.instance.PlayEffect("CFX_ShadowStep", target.gameObject);
        //if (AudioManager.instance != null)
        //AudioManager.instance.PlayEntitySound("ShadowStep", player);

        Debug.Log($"[Shadowstep] Spieler teleportiert zu {target.name} und verursacht {totalDamage} Schaden.");
    }

    public override void OnCooldown(IEntitie entitie)
    {
        // Optional: Logik bei Cooldown-Ende
    }

    public override void OnTick(IEntitie entitie)
    {
        throw new System.NotImplementedException();
    }
}
