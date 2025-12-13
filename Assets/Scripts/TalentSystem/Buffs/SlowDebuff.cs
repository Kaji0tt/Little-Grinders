using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slow-Debuff der linear von initialSlowPercentage zu 0% über die Dauer abklingt.
/// Wird von AimedShotAbility und anderen Abilities verwendet.
/// </summary>
[CreateAssetMenu(menuName = "Buff/SlowDebuff")]
public class SlowDebuff : Buff
{
    [Header("Slow Settings")]
    [Tooltip("Initiale Verlangsamung in Prozent (0.0 - 1.0, z.B. 0.95 = 95% Slow)")]
    [Range(0f, 1f)]
    public float initialSlowPercentage = 0.95f;

    /// <summary>
    /// Kopiert SlowDebuff-spezifische Daten in die BuffInstance
    /// </summary>
    public override void CopyFrom(Buff source)
    {
        if (source is SlowDebuff slowSource)
        {
            initialSlowPercentage = slowSource.initialSlowPercentage;
            Debug.Log($"[SlowDebuff] CopyFrom - Kopiere initialSlowPercentage: {initialSlowPercentage}");
        }
    }

    public override void Activated(IEntitie instanceTarget, Transform targetTransform)
    {
        // ✅ Hole BuffInstance über GetBuffs() - sie ist jetzt schon in der Liste!
        BuffInstance instance = instanceTarget.GetBuffs().Find(b => b.originalBuff == this && b.MyTargetEntitie == instanceTarget);
        
        if (instance == null)
        {
            Debug.LogWarning("[SlowDebuff] BuffInstance nicht gefunden in Activated()!");
            return;
        }

        // ✅ Speichere Daten IN der BuffInstance
        float originalSpeed = instanceTarget.GetStat(EntitieStats.MovementSpeed).BaseValue;
        float slowedSpeed = originalSpeed * (1f - initialSlowPercentage);
        
        StatModifier slowModifier = new StatModifier(
            slowedSpeed - originalSpeed, 
            StatModType.Flat, 
            this
        );
        
        // Speichere in BuffInstance
        instance.SetInstanceData("slowModifier", slowModifier);
        
        // Wende Modifier an
        instanceTarget.GetStat(EntitieStats.MovementSpeed).AddModifier(slowModifier);
        
        Debug.Log($"[SlowDebuff] Aktiviert auf {targetTransform.name}! Speed: {originalSpeed:F2} → {slowedSpeed:F2} ({initialSlowPercentage * 100:F0}% Slow), Duration: {instance.MyDuration:F2}s");
    }

    public override void Update()
    {
        // ✅ Nichts zu tun - Slow bleibt konstant über die gesamte Duration
    }

    public override void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        // ✅ Nichts zu tun - Slow ändert sich nicht über Zeit
    }

    public override void Expired(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        Debug.Log($"[SlowDebuff] Expired() wird aufgerufen für {instanceTarget.GetTransform().name}");
        
        BuffInstance instance = instanceTarget.GetBuffs().Find(b => b.originalBuff == this && b.MyTargetEntitie == instanceTarget);
        
        if (instance != null && instance.HasInstanceData("slowModifier"))
        {
            StatModifier slowModifier = instance.GetInstanceData<StatModifier>("slowModifier");
            
            Debug.Log($"[SlowDebuff] Entferne Slow-Modifier. Speed vor Entfernung: {instanceTarget.GetStat(EntitieStats.MovementSpeed).Value:F2}");
            instanceTarget.GetStat(EntitieStats.MovementSpeed).RemoveModifier(slowModifier);
            Debug.Log($"[SlowDebuff] Expired - Speed zurück auf: {instanceTarget.GetStat(EntitieStats.MovementSpeed).Value:F2}");
        }
        else
        {
            Debug.LogWarning($"[SlowDebuff] Expired - Keine Instanz-Daten gefunden!");
        }
        
        active = false;
    }
}
