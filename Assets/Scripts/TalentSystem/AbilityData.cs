﻿using UnityEngine;

// Der Enum bleibt so, wie du ihn erweitert hast.
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