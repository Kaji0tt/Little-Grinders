using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages all spell sockets in the talent tree and provides spell selection UI
/// </summary>
public class SpellSocketManager : MonoBehaviour
{
    #region Singleton
    public static SpellSocketManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    [Header("Socket Management")]
    public List<SpellSocket> allSockets = new List<SpellSocket>();
    
    [Header("Spell Selection UI")]
    public GameObject spellSelectionPanel;
    public Transform spellButtonParent;
    public GameObject spellButtonPrefab;
    public Text socketTypeLabel;
    public Button closeButton;
    
    [Header("Available Pace Spells")]
    public List<AbilityData> availablePaceSpells = new List<AbilityData>();
    
    private SpellSocket currentSelectedSocket;
    private List<GameObject> spawnedSpellButtons = new List<GameObject>();
    
    private void Start()
    {
        // Find all sockets in the scene
        FindAllSockets();
        
        // Setup UI
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSpellSelection);
        }
        
        if (spellSelectionPanel != null)
        {
            spellSelectionPanel.SetActive(false);
        }
    }
    
    private void FindAllSockets()
    {
        SpellSocket[] foundSockets = FindObjectsOfType<SpellSocket>();
        allSockets.Clear();
        allSockets.AddRange(foundSockets);
        
        Debug.Log($"[SpellSocketManager] Found {allSockets.Count} spell sockets");
    }
    
    public void UnlockSocket(SpellSocketType socketType, int socketIndex = 0)
    {
        var socket = GetSocket(socketType, socketIndex);
        if (socket != null)
        {
            socket.UnlockSocket();
            Debug.Log($"[SpellSocketManager] Unlocked {socketType} socket #{socketIndex}");
        }
    }
    
    public void LockSocket(SpellSocketType socketType, int socketIndex = 0)
    {
        var socket = GetSocket(socketType, socketIndex);
        if (socket != null)
        {
            socket.LockSocket();
            Debug.Log($"[SpellSocketManager] Locked {socketType} socket #{socketIndex}");
        }
    }
    
    private SpellSocket GetSocket(SpellSocketType socketType, int index)
    {
        int count = 0;
        foreach (var socket in allSockets)
        {
            if (socket.socketType == socketType)
            {
                if (count == index)
                    return socket;
                count++;
            }
        }
        return null;
    }
    
    public void OpenSpellSelectionForSocket(SpellSocket socket)
    {
        if (socket == null) return;
        
        currentSelectedSocket = socket;
        
        if (spellSelectionPanel != null)
        {
            spellSelectionPanel.SetActive(true);
        }
        
        if (socketTypeLabel != null)
        {
            socketTypeLabel.text = $"Select {socket.GetSocketTypeName()} Spell";
        }
        
        PopulateSpellSelection();
    }
    
    private void PopulateSpellSelection()
    {
        // Clear existing buttons
        ClearSpellButtons();
        
        if (currentSelectedSocket == null) return;
        
        // Filter spells for current socket type
        SpellProperty requiredProperty = GetRequiredSpellProperty(currentSelectedSocket.socketType);
        
        foreach (var spell in availablePaceSpells)
        {
            if (spell.properties.HasFlag(requiredProperty))
            {
                CreateSpellButton(spell);
            }
        }
        
        // Add option to remove current spell
        if (currentSelectedSocket.socketedSpell != null)
        {
            CreateRemoveSpellButton();
        }
    }
    
    private void CreateSpellButton(AbilityData spell)
    {
        if (spellButtonPrefab == null || spellButtonParent == null) return;
        
        GameObject buttonObj = Instantiate(spellButtonPrefab, spellButtonParent);
        spawnedSpellButtons.Add(buttonObj);
        
        // Setup button components
        Button button = buttonObj.GetComponent<Button>();
        Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        Text nameText = buttonObj.transform.Find("Name")?.GetComponent<Text>();
        Text descText = buttonObj.transform.Find("Description")?.GetComponent<Text>();
        
        if (icon != null && spell.icon != null)
        {
            icon.sprite = spell.icon;
        }
        
        if (nameText != null)
        {
            nameText.text = spell.abilityName;
        }
        
        if (descText != null)
        {
            descText.text = spell.description;
        }
        
        if (button != null)
        {
            button.onClick.AddListener(() => SelectSpell(spell));
        }
    }
    
    private void CreateRemoveSpellButton()
    {
        if (spellButtonPrefab == null || spellButtonParent == null) return;
        
        GameObject buttonObj = Instantiate(spellButtonPrefab, spellButtonParent);
        spawnedSpellButtons.Add(buttonObj);
        
        // Setup as remove button
        Button button = buttonObj.GetComponent<Button>();
        Text nameText = buttonObj.transform.Find("Name")?.GetComponent<Text>();
        Text descText = buttonObj.transform.Find("Description")?.GetComponent<Text>();
        
        if (nameText != null)
        {
            nameText.text = "Remove Spell";
        }
        
        if (descText != null)
        {
            descText.text = "Remove the currently socketed spell";
        }
        
        if (button != null)
        {
            button.onClick.AddListener(RemoveCurrentSpell);
        }
    }
    
    private void SelectSpell(AbilityData spell)
    {
        if (currentSelectedSocket != null)
        {
            if (currentSelectedSocket.SocketSpell(spell))
            {
                Debug.Log($"[SpellSocketManager] Successfully socketed {spell.abilityName}");
            }
            else
            {
                Debug.LogWarning($"[SpellSocketManager] Failed to socket {spell.abilityName}");
            }
        }
        
        CloseSpellSelection();
    }
    
    private void RemoveCurrentSpell()
    {
        if (currentSelectedSocket != null)
        {
            currentSelectedSocket.RemoveSocketedSpell();
        }
        
        CloseSpellSelection();
    }
    
    public void CloseSpellSelection()
    {
        if (spellSelectionPanel != null)
        {
            spellSelectionPanel.SetActive(false);
        }
        
        currentSelectedSocket = null;
        ClearSpellButtons();
    }
    
    private void ClearSpellButtons()
    {
        foreach (var button in spawnedSpellButtons)
        {
            if (button != null)
            {
                Destroy(button);
            }
        }
        spawnedSpellButtons.Clear();
    }
    
    private SpellProperty GetRequiredSpellProperty(SpellSocketType socketType)
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
    
    public void AddAvailableSpell(AbilityData spell)
    {
        if (!availablePaceSpells.Contains(spell))
        {
            availablePaceSpells.Add(spell);
            Debug.Log($"[SpellSocketManager] Added available spell: {spell.abilityName}");
        }
    }
    
    public void RemoveAvailableSpell(AbilityData spell)
    {
        if (availablePaceSpells.Contains(spell))
        {
            availablePaceSpells.Remove(spell);
            Debug.Log($"[SpellSocketManager] Removed available spell: {spell.abilityName}");
        }
    }
    
    public int GetUnlockedSocketCount(SpellSocketType socketType)
    {
        int count = 0;
        foreach (var socket in allSockets)
        {
            if (socket.socketType == socketType && socket.isUnlocked)
            {
                count++;
            }
        }
        return count;
    }
    
    public int GetTotalSocketCount(SpellSocketType socketType)
    {
        int count = 0;
        foreach (var socket in allSockets)
        {
            if (socket.socketType == socketType)
            {
                count++;
            }
        }
        return count;
    }
}