using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{

    #region Singleton
    public static PlayerManager instance;


    private void Awake()
    {
        instance = this;
        
        // Versuche Player zu finden
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj != null)
        {
            player = playerObj.GetComponent<IsometricPlayer>();
            
            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
                Debug.Log("[PlayerManager] Player erfolgreich initialisiert");
            }
            else
            {
                Debug.LogError("[PlayerManager] IsometricPlayer Komponente nicht gefunden auf Player GameObject!");
            }
        }
        else
        {
            Debug.LogError("[PlayerManager] Kein GameObject mit Tag 'Player' gefunden! Bitte Tag setzen.");
        }
    }
    
    // NEU: Fallback-Methode falls Awake fehlschlägt
    private void Start()
    {
        // Falls in Awake nicht gefunden, nochmal versuchen
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            
            if (playerObj != null)
            {
                player = playerObj.GetComponent<IsometricPlayer>();
                playerStats = player?.GetComponent<PlayerStats>();
                Debug.Log("[PlayerManager] Player in Start() nachgeladen");
            }
        }
    }

    public void SetPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj != null)
        {
            player = playerObj.GetComponent<IsometricPlayer>();
            playerStats = player.GetComponent<PlayerStats>();
            Debug.Log("[PlayerManager] Player via SetPlayer() gesetzt");
        }
        else
        {
            Debug.LogError("[PlayerManager] Kein GameObject mit Tag 'Player' gefunden in SetPlayer()!");
        }
    }

    public void SetPlayerStats()
    {
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            Debug.Log("[PlayerManager] PlayerStats via SetPlayerStats() gesetzt");
        }
        else
        {
            Debug.LogError("[PlayerManager] Player ist null in SetPlayerStats()!");
        }
    }

    #endregion
    public static ItemInstance GetEquippedWeapon()
    {
        var allSlots = FindObjectsOfType<Int_SlotBtn>();
        var weaponSlot = allSlots.FirstOrDefault(slot => slot.slotType == ItemType.Weapon);
        return weaponSlot?.GetEquippedItem();
    }

    public IsometricPlayer player { get; private set; }

    public PlayerStats playerStats { get; private set; }


    public Image xpFill, hpFill;

    public TextMeshProUGUI xp_Text;

    public GameObject hp_Text;

}
