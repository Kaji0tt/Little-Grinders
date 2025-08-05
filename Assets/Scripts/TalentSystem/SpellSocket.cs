using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a socket in the talent tree that can hold pace-enhancing spells
/// </summary>
public class SpellSocket : MonoBehaviour
{
    [Header("Socket Configuration")]
    public SpellSocketType socketType;
    public bool isUnlocked = false;
    
    [Header("Socket UI")]
    public Image socketIcon;
    public Image lockedOverlay;
    public Button socketButton;
    
    [Header("Currently Socketed Spell")]
    public AbilityData socketedSpell;
    private Ability socketedAbilityInstance;
    
    [Header("Visual Feedback")]
    public Sprite emptySocketSprite;
    public Sprite movementSocketSprite;
    public Sprite utilitySocketSprite;
    public Sprite apSocketSprite;
    public Sprite adSocketSprite;
    
    private void Start()
    {
        if (socketButton != null)
        {
            socketButton.onClick.AddListener(OnSocketClicked);
        }
        
        UpdateSocketVisuals();
    }
    
    public void UnlockSocket()
    {
        isUnlocked = true;
        UpdateSocketVisuals();
    }
    
    public void LockSocket()
    {
        isUnlocked = false;
        RemoveSocketedSpell();
        UpdateSocketVisuals();
    }
    
    public bool CanSocketSpell(AbilityData spell)
    {
        if (!isUnlocked) return false;
        if (spell == null) return false;
        
        // Check if spell matches socket type
        SpellProperty requiredProperty = GetRequiredSpellProperty();
        return spell.properties.HasFlag(requiredProperty);
    }
    
    public bool SocketSpell(AbilityData spell)
    {
        if (!CanSocketSpell(spell)) return false;
        
        // Remove existing spell first
        RemoveSocketedSpell();
        
        // Socket new spell
        socketedSpell = spell;
        
        // Create ability instance and add to player
        if (spell.abilityPrefab != null)
        {
            GameObject abilityObject = Instantiate(spell.abilityPrefab, PlayerManager.instance.player.transform);
            socketedAbilityInstance = abilityObject.GetComponent<Ability>();
            
            if (socketedAbilityInstance != null)
            {
                // Initialize with spell data and moderate rarity scaling
                socketedAbilityInstance.Initialize(spell, 1.2f); // Slight boost for socketed spells
                
                // Add to player's ability collection if needed
                // Note: This might need adjustment based on how abilities are managed
            }
        }
        
        UpdateSocketVisuals();
        Debug.Log($"[SpellSocket] Socketed spell: {spell.abilityName} in {socketType} socket");
        return true;
    }
    
    public void RemoveSocketedSpell()
    {
        if (socketedSpell != null)
        {
            Debug.Log($"[SpellSocket] Removing spell: {socketedSpell.abilityName} from {socketType} socket");
        }
        
        // Destroy ability instance
        if (socketedAbilityInstance != null)
        {
            Destroy(socketedAbilityInstance.gameObject);
            socketedAbilityInstance = null;
        }
        
        socketedSpell = null;
        UpdateSocketVisuals();
    }
    
    private SpellProperty GetRequiredSpellProperty()
    {
        return socketType switch
        {
            SpellSocketType.Movement => SpellProperty.PaceMovement,
            SpellSocketType.Utility => SpellProperty.PaceUtility,
            SpellSocketType.AP => SpellProperty.PaceAP,
            SpellSocketType.AD => SpellProperty.PaceAD,
            _ => SpellProperty.None
        };
    }
    
    private void UpdateSocketVisuals()
    {
        if (socketIcon != null)
        {
            if (socketedSpell != null && socketedSpell.icon != null)
            {
                socketIcon.sprite = socketedSpell.icon;
            }
            else
            {
                socketIcon.sprite = GetSocketTypeSprite();
            }
        }
        
        if (lockedOverlay != null)
        {
            lockedOverlay.gameObject.SetActive(!isUnlocked);
        }
        
        if (socketButton != null)
        {
            socketButton.interactable = isUnlocked;
        }
    }
    
    private Sprite GetSocketTypeSprite()
    {
        return socketType switch
        {
            SpellSocketType.Movement => movementSocketSprite ?? emptySocketSprite,
            SpellSocketType.Utility => utilitySocketSprite ?? emptySocketSprite,
            SpellSocketType.AP => apSocketSprite ?? emptySocketSprite,
            SpellSocketType.AD => adSocketSprite ?? emptySocketSprite,
            _ => emptySocketSprite
        };
    }
    
    private void OnSocketClicked()
    {
        if (!isUnlocked) return;
        
        // Open spell selection UI for this socket type
        if (SpellSocketManager.instance != null)
        {
            SpellSocketManager.instance.OpenSpellSelectionForSocket(this);
        }
    }
    
    public string GetSocketTypeName()
    {
        return socketType switch
        {
            SpellSocketType.Movement => "Movement",
            SpellSocketType.Utility => "Utility", 
            SpellSocketType.AP => "Ability Power",
            SpellSocketType.AD => "Attack Damage",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Defines the type of spell socket
/// </summary>
public enum SpellSocketType
{
    Movement,   // Enhances movement and positioning
    Utility,    // Provides utility effects that improve pace
    AP,         // Ability Power focused pace enhancement 
    AD          // Attack Damage focused pace enhancement
}