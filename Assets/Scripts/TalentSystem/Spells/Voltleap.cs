using UnityEngine;
using System.Collections.Generic;

public class Voltleap : Ability
{
    private int channelTicks = 0;
    private float baseAbilityPowerMultiplier = 1f;
    private float radius = 2.5f; // AoE-Radius

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        baseAbilityPowerMultiplier = rarityScaling;
        // Debug.Log($"[Voltleap] rarityScaling angewendet: {rarityScaling}, baseAbilityPowerMultiplier: {baseAbilityPowerMultiplier}");
    }

    public override void OnTick(IEntitie entitie)
    {
        channelTicks++;
        if(VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_LightHit", PlayerManager.instance.player.playerStats);
        }
        // Debug.Log($"[Voltleap] Channeling... Ticks: {channelTicks}");
    }

    public override void UseBase(IEntitie entitie)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPos;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Floor")))
        {
            targetPos = hit.point;
        }
        else
        {
            // Debug.LogWarning("[Voltleap] Kein gültiger Zielpunkt unter dem Mauszeiger gefunden!");
            channelTicks = 0;
            return;
        }

        var player = PlayerManager.instance.player;
        Vector3 teleportPos = targetPos;
        teleportPos.y = player.transform.position.y;
        player.transform.position = teleportPos;

        var playerStats = PlayerManager.instance.playerStats;
        float ap = playerStats.AbilityPower.Value;
        float totalDamage = (ap / 4f) * (1 + channelTicks) * baseAbilityPowerMultiplier;

        Collider[] hits = Physics.OverlapSphere(targetPos, radius);
        List<GameObject> enemies = new List<GameObject>();
        foreach (var hitCol in hits)
        {
            if (hitCol.CompareTag("Enemy"))
            {
                var enemy = hitCol.GetComponentInParent<EnemyController>();
                if (enemy != null && !enemy.mobStats.isDead)
                    enemies.Add(hitCol.gameObject);
            }
        }
        // Debug.Log(enemies.Count + " Gegner im Umkreis gefunden.");
        int enemyCount = enemies.Count;

        if (enemyCount == 0)
        {
            // Debug.Log("[Voltleap] Keine Gegner im Radius gefunden.");
            channelTicks = 0;
            return;
        }

        float splitDamage = totalDamage / enemyCount;

        foreach (var enemyObj in enemies)
        {
            var enemy = enemyObj.GetComponentInParent<EnemyController>();
            if (enemy != null && !enemy.mobStats.isDead)
            {
                enemy.TakeDirectDamage(splitDamage, radius);
                // Optional: VFX
                // if (VFX_Manager.instance != null)
                //     VFX_Manager.instance.PlayEffect("CFX_VoltHit", enemy);
            }
        }

        // Debug.Log($"[Voltleap] Spieler teleportiert zu {targetPos} und verteilt {totalDamage:F1} Schaden auf {enemyCount} Gegner (je {splitDamage:F1}).");

        channelTicks = 0;
    }

    public override void OnCooldown(IEntitie entitie)
    {
        channelTicks = 0;
    }

    protected override void OnChannelEnter()
    {
        if(VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("CFX_LightGlobe", PlayerManager.instance.player.playerStats);
        }
        // z.B. VFX, Sound, etc.
    }

    protected override void OnChannelUpdate()
    {
        // z.B. Animation, Effekte, etc.
    }

    protected override void OnChannelExit()
    {
        // Debug.Log("[Voltleap] Channeling beendet!");
        // z.B. VFX stoppen, etc.
    }
}
