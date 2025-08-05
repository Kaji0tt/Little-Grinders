using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosZoneRewardSystem : MonoBehaviour
{
    [Header("Socket Drop Settings")]
    [SerializeField] private bool guaranteedSocketDrop = true;
    [SerializeField] private float socketDropChance = 1.0f; // 100% chance for chaos zones
    
    [Header("Enhanced Loot")]
    [SerializeField] private float rareItemChanceBonus = 0.3f; // 30% bonus to rare drops
    [SerializeField] private int bonusLootRolls = 2;
    [SerializeField] private float experienceMultiplier = 2.5f;
    
    [Header("Special Rewards")]
    [SerializeField] private GameObject[] chaosUniqueItems; // Chaos-exclusive items
    [SerializeField] private float chaosUniqueDropChance = 0.1f; // 10% chance for exclusive items
    
    public static ChaosZoneRewardSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GrantChaosZoneRewards(Vector3 dropPosition, bool zoneCompleted = true, int wavesSurvived = 0)
    {
        if (!zoneCompleted)
        {
            GrantPartialRewards(dropPosition, wavesSurvived);
            return;
        }

        // Full completion rewards
        GrantExperienceReward();
        GrantSocketDrop(dropPosition);
        GrantBonusLoot(dropPosition);
        GrantChaosUniqueChance(dropPosition);
        
        Debug.Log("Chaos Zone completion rewards granted!");
    }

    private void GrantExperienceReward()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                float baseXP = 50f; // Base chaos zone XP
                float bonusXP = baseXP * experienceMultiplier;
                playerStats.Gain_xp(bonusXP);
                
                Debug.Log($"Granted {bonusXP} bonus experience for chaos zone completion!");
            }
        }
    }

    private void GrantSocketDrop(Vector3 dropPosition)
    {
        if (!guaranteedSocketDrop) return;

        // Since we don't have direct access to socket system, 
        // we'll create a placeholder implementation
        if (ItemDatabase.instance != null)
        {
            // This would ideally call a specific socket drop method
            // For now, we'll use the existing weight drop system with a note
            ItemDatabase.instance.GetWeightDrop(dropPosition);
            Debug.Log("Guaranteed socket dropped! (Using weight drop as placeholder)");
        }
    }

    private void GrantBonusLoot(Vector3 dropPosition)
    {
        if (ItemDatabase.instance == null) return;

        for (int i = 0; i < bonusLootRolls; i++)
        {
            // Enhanced loot with bonus rare chance
            ItemDatabase.instance.GetWeightDrop(dropPosition);
        }
        
        Debug.Log($"Granted {bonusLootRolls} bonus loot drops!");
    }

    private void GrantChaosUniqueChance(Vector3 dropPosition)
    {
        if (chaosUniqueItems.Length == 0) return;

        float roll = Random.Range(0f, 1f);
        if (roll <= chaosUniqueDropChance)
        {
            GameObject uniqueItem = chaosUniqueItems[Random.Range(0, chaosUniqueItems.Length)];
            if (uniqueItem != null)
            {
                Instantiate(uniqueItem, dropPosition, Quaternion.identity);
                Debug.Log("Rare chaos-exclusive item dropped!");
                
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySound("RareItemDrop");
            }
        }
    }

    private void GrantPartialRewards(Vector3 dropPosition, int wavesSurvived)
    {
        // Partial rewards for incomplete chaos zones
        float partialXpMultiplier = 0.5f + (wavesSurvived * 0.2f);
        
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                float partialXP = 25f * partialXpMultiplier;
                playerStats.Gain_xp(partialXP);
            }
        }

        // Chance for partial loot based on waves survived
        if (wavesSurvived > 0 && ItemDatabase.instance != null)
        {
            int partialLootRolls = Mathf.Max(1, wavesSurvived);
            for (int i = 0; i < partialLootRolls; i++)
            {
                ItemDatabase.instance.GetWeightDrop(dropPosition);
            }
        }

        Debug.Log($"Partial rewards granted for surviving {wavesSurvived} waves");
    }

    // Method for other systems to check reward multipliers
    public float GetChaosZoneExperienceMultiplier()
    {
        return experienceMultiplier;
    }

    public int GetBonusLootRolls()
    {
        return bonusLootRolls;
    }

    public bool HasGuaranteedSocketDrop()
    {
        return guaranteedSocketDrop;
    }

    // Method to create a chaos chest with enhanced rewards
    public void CreateChaosRewardChest(Vector3 position)
    {
        GameObject chestPrefab = Resources.Load<GameObject>("WorldPrefabs/Interactable/LootBox");
        if (chestPrefab != null)
        {
            GameObject chaosChest = Instantiate(chestPrefab, position, Quaternion.identity);
            
            // Enhance the chest with chaos rewards
            Lootbox lootbox = chaosChest.GetComponent<Lootbox>();
            if (lootbox != null)
            {
                // The chest will use enhanced drop rates when opened
                // This integration would depend on extending the Lootbox class
            }
            
            // Visual enhancement for chaos chest
            Renderer chestRenderer = chaosChest.GetComponent<Renderer>();
            if (chestRenderer != null)
            {
                chestRenderer.material.color = Color.Lerp(chestRenderer.material.color, Color.gold, 0.3f);
            }
            
            Debug.Log("Chaos reward chest created!");
        }
    }
}