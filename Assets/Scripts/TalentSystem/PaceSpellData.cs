using UnityEngine;

[CreateAssetMenu(menuName = "PaceSpells/CombatRush")]
public class CombatRushData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Combat Rush";
        description = "Provides a brief speed boost when combat starts, enhancing combat pace and positioning.";
        properties = SpellProperty.PaceMovement | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 3f;
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/EvasionBoost")]
public class EvasionBoostData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Evasion Boost";
        description = "Grants speed boost after taking damage, helping maintain combat flow and positioning.";
        properties = SpellProperty.PaceMovement | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 2f;
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/VictoryMomentum")]
public class VictoryMomentumData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Victory Momentum";
        description = "Reduces all cooldowns when enemies are defeated, maintaining combat pace.";
        properties = SpellProperty.PaceUtility | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 5f;
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/BattleTrance")]
public class BattleTranceData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Battle Trance";
        description = "Provides enhanced regeneration during combat, sustaining fight endurance.";
        properties = SpellProperty.PaceUtility | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 8f;
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/ArcaneEcho")]
public class ArcaneEchoData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Arcane Echo";
        description = "Increases ability power based on number of abilities used recently, rewarding active spellcasting.";
        properties = SpellProperty.PaceAP | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 6f;
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/ManaFlow")]
public class ManaFlowData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Mana Flow";
        description = "Provides mana management and ability cost reduction, enabling more frequent spell usage.";
        properties = SpellProperty.PaceAP | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 0f; // Always active
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/BloodFrenzy")]
public class BloodFrenzyData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Blood Frenzy";
        description = "Increases attack speed with consecutive hits, building momentum in combat.";
        properties = SpellProperty.PaceAD | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 4f;
        maxCharges = 1;
    }
}

[CreateAssetMenu(menuName = "PaceSpells/PrecisionStrike")]
public class PrecisionStrikeData : AbilityData
{
    private void OnEnable()
    {
        abilityName = "Precision Strike";
        description = "Provides critical hit bonuses and combo multipliers, rewarding skillful combat.";
        properties = SpellProperty.PaceAD | SpellProperty.Persistent;
        cooldownTime = 0f; // Passive ability
        activeTime = 3f;
        maxCharges = 1;
    }
}