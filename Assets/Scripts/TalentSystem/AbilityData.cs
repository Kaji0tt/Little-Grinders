using UnityEngine;

// Extended for pace-enhancing spell clusters
[System.Flags]
public enum SpellProperty
{
    None = 0,
    Ranged = 1 << 0,
    AoE = 1 << 1,
    Projectile = 1 << 2,
    Targeted = 1 << 3,
    Movement = 1 << 4,
    Persistent = 1 << 5, // NEU: Immer aktiv, passiv lauschend
    Channeling = 1 << 6,
    Instant = 1 << 7,
    Active = 1 << 8,
    
    // Pace-enhancing clusters
    PaceMovement = 1 << 9,  // Movement cluster for pace enhancement
    PaceUtility = 1 << 10,  // Utility cluster for pace enhancement
    PaceAP = 1 << 11,       // Ability Power cluster for pace enhancement
    PaceAD = 1 << 12,       // Attack Damage cluster for pace enhancement
}

[CreateAssetMenu(menuName = "Active/Ability")]
public class AbilityData : ScriptableObject
{
    [Header("Core Information")]
    public string abilityName;
    [TextArea]
    public string description;
    public Sprite icon;
    public SpellProperty properties;

    [Header("Logic Prefab")]
    public GameObject abilityPrefab;

    [Header("Mechanics (Conditional)")]
    public float range;
    public float areaOfEffectRadius;
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public float channelTime;
    public float activeTime; // NEU: Für Active-Fähigkeiten
    public float tickTimer;  // Für Active und Channeling

    [Header("Core Stats")]
    public float cooldownTime;
    [Tooltip("Wie viele Aufladungen hat die Fähigkeit maximal? Default: 1 für Standardfähigkeiten.")]
    public int maxCharges = 1;
}