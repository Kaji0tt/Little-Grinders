using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChaosZoneConfig", menuName = "Little Grinders/Chaos Zone Configuration")]
public class ChaosZoneConfig : ScriptableObject
{
    [Header("Zone Identity")]
    public string zoneName = "Unnamed Chaos Zone";
    public string zoneDescription = "A dangerous area filled with chaotic energy";
    
    [Header("Difficulty Settings")]
    [Range(1f, 5f)]
    public float difficultyMultiplier = 2f;
    [Range(0.5f, 3f)]
    public float pressureIntensity = 1.5f;
    [Range(60f, 600f)]
    public float zoneDuration = 300f; // seconds
    
    [Header("Wave Configuration")]
    [Range(1, 10)]
    public int waveCount = 3;
    [Range(10f, 60f)]
    public float waveCooldown = 30f;
    [Range(1, 20)]
    public int baseEnemiesPerWave = 4;
    
    [Header("Rewards")]
    public bool guaranteedSocketDrop = true;
    [Range(1f, 5f)]
    public float experienceMultiplier = 2.5f;
    [Range(0, 10)]
    public int bonusLootRolls = 2;
    [Range(0, 5)]
    public int guaranteedRareDrops = 1;
    
    [Header("Environmental Effects")]
    public Color ambientColor = Color.red;
    public Color pressureOverlayColor = new Color(0.8f, 0.2f, 0.2f, 0.1f);
    public AudioClip zoneAmbientSound;
    public ParticleSystem chaosParticleEffect;
    
    [Header("Spawn Locations")]
    public Vector3[] enemySpawnPoints = new Vector3[8];
    public Vector3 playerSpawnPoint = Vector3.zero;
    public Vector3 exitPortalPosition = Vector3.zero;
    
    [Header("Zone Type Specific")]
    public ChaosZoneType zoneType = ChaosZoneType.Standard;
    public GameObject[] specialObjects; // Zone-specific objects like environmental hazards
}

public enum ChaosZoneType
{
    Standard,
    Forest,
    Cave,
    Ruins,
    Volcanic,
    Frozen,
    Corrupted
}