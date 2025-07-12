using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    #region Singleton
    // Die statische Instanz des Singletons
    public static GameEvents Instance { get; private set; }

    private void Awake()
    {
        // Überprüfen, ob eine Instanz bereits existiert
        if (Instance == null)
        {
            Instance = this; // Setze die aktuelle Instanz
            DontDestroyOnLoad(gameObject); // Optional: Behalte dieses Objekt bei Szenenwechseln
        }
        else if (Instance != this)
        {
            // Falls eine andere Instanz bereits existiert, zerstöre diese neue Instanz
            Destroy(gameObject);
        }
    }
    #endregion


    // Events für Ausrüstungsgegenstände
    public event Action<ItemInstance> OnEquipKopf;
    public event Action<ItemInstance> OnEquipSchuhe;
    public event Action<ItemInstance> OnEquipHose;
    public event Action<ItemInstance> OnEquipBrust;
    public event Action<ItemInstance> OnEquipWeapon;
    public event Action<ItemInstance> OnEquipSchmuck;

    // Kampf-Events
    public event Action<float> OnPlayerHasAttacked;
    public event Action<float> OnPlayerWasAttacked;
    public event Action<float> OnEnemyHasAttacked;
    public event Action<float, Transform, bool> OnEnemyWasAttacked;



    // Sound-Event
    public static event Action<string> PlaySound;

    public void EquipChanged(ItemInstance item)
    {
        switch (item.itemType)
        {
            case ItemType.Kopf:
                OnEquipKopf?.Invoke(item);
                break;
            case ItemType.Brust:
                OnEquipBrust?.Invoke(item);
                break;
            case ItemType.Beine:
                OnEquipHose?.Invoke(item);
                break;
            case ItemType.Schuhe:
                OnEquipSchuhe?.Invoke(item);
                break;
            case ItemType.Schmuck:
                OnEquipSchmuck?.Invoke(item);
                break;
            case ItemType.Weapon:
                OnEquipWeapon?.Invoke(item);
                break;
            default:
                Debug.LogWarning("Unknown item type: " + item.itemType);
                return; // Beenden, wenn der Typ unbekannt ist.
        }

        // Der GameEvent-Manager ruft jetzt nur noch die zentrale UI-Methode auf.
        // Er muss nicht mehr wissen, WIE das UI die Fähigkeit zuweist.
    }

    // Kampfaktionen
    public void PlayerHasAttacked(float damage)
    {
        OnPlayerHasAttacked?.Invoke(damage);
    }
    public void PlayerWasAttacked(float damage)
    {
        OnPlayerWasAttacked?.Invoke(damage);
    }
    public void EnemyWasAttacked(float damage, Transform enemyTransform, bool crit)
    {
        OnEnemyWasAttacked?.Invoke(damage, enemyTransform, crit);
    }

    public void EnemyWasAttacked(float damage)
    {
        //OnEnemyWasAttacked?.Invoke(damage);

    }
}