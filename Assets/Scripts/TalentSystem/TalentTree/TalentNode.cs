using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



// Klasse für eine einzelne Talent-Node
public class TalentNode
{
    /// <summary>
    /// Data Structure
    /// </summary>
    public int ID; // Eindeutige ID

    public List<TalentNode> myConnectedNodes = new();

    public int Depth; // Entfernung von der Wurzel

    public List<TalentType> myTypes = new List<TalentType>(); // 📌 Ändert sich von einem einzelnen Typ zu einer Liste
    //public TalentType myType;

    public Transform uiElement; // UI-Element zur Darstellung

    public Vector2 myPosition;

    public Talent_UI myTalentUI { get; private set; }

    /// <summary>
    /// Player Values
    /// </summary>

    public int myCurrentCount;

    public int myMaxCount;

    public float myValue;

    public bool isUnlocked { get; private set; } = false;

    public bool hasExpanded = false;

    // === GEM SOCKET EIGENSCHAFTEN ===
    /// <summary>
    /// Ist dieser Node ein Gem-Socket? Wenn ja, hat er keine TalentTypes/myValue
    /// </summary>
    public bool isGemSocket = false;

    /// <summary>
    /// Das aktuell in diesem Socket ausgerüstete Gem (null wenn leer)
    /// </summary>
    public ItemInstance equippedGem = null;

    /// <summary>
    /// Distanz zum nächsten Gem-Socket (wird für Wahrscheinlichkeitsberechnung verwendet)
    /// </summary>
    public int distanceToNearestGemSocket = int.MaxValue;

    // Statische Tracking für Ability-Gems pro GemType (nur 1 Ability-Gem pro GemType erlaubt)
    private static Dictionary<GemType, TalentNode> abilityGemSockets = new Dictionary<GemType, TalentNode>();

    /// <summary>
    /// Speichert alle StatModifier die dieser Socket für das equipped Gem erstellt hat
    /// Key: EntitieStats (z.B. Hp), Value: Liste von Modifiern (1 pro Skillpunkt)
    /// </summary>
    private Dictionary<EntitieStats, List<StatModifier>> socketModifiers = new Dictionary<EntitieStats, List<StatModifier>>();

    public void Unlock()
    {
        isUnlocked = true;
        myTalentUI?.Unlock();
        TalentTreeManager.instance.UpdateTalentTree(this);
    }

    //Generierung der Urpsrungstalente 
    public TalentNode(int id, TalentNode parent, int depth, TalentType type)
    {
        myConnectedNodes = new List<TalentNode>();
        if (parent != null)
            myConnectedNodes.Add(parent);

        myTypes.Add(type);

        ID = id;
        Depth = depth;

        myCurrentCount = 0;
        myMaxCount = (depth == 0) ? 3 : UnityEngine.Random.Range(1, 6);
        myValue = CalculateValue(depth);
    }

    public TalentNode(int id, TalentNode parent, int depth)
    {
        myConnectedNodes = new List<TalentNode>();
        if (parent != null)
            myConnectedNodes.Add(parent);

        myTypes = new List<TalentType>();

        ID = id;
        Depth = depth;

        myCurrentCount = 0;
        myMaxCount = (depth == 0) ? 3 : UnityEngine.Random.Range(1, 6);
        myValue = CalculateValue(depth);
    }

    public void SetGameObject(Talent_UI talentUI)
    {
        myTalentUI = talentUI;
    }

    // Berechnet myValue basierend auf Depth mit einer Variation von ±50%, aber mindestens 1
    private float CalculateValue(int depth)
    {
        if (depth != 0)
        {
            float baseValue = 1 + (depth * 0.2f); // Grundwert steigt mit Tiefe an
            float variation = baseValue * UnityEngine.Random.Range(-0.5f, 0.5f); // ±50% Zufallsvariation
            float finalValue = Mathf.Max(1, baseValue + variation); // Niemals unter 1 fallen
            return (float)Math.Round(finalValue, 1); // Auf eine Nachkommastelle runden
        }
        else return 1;

    }

    public bool IsExpanded()
    {
        if (myConnectedNodes == null || myConnectedNodes.Count == 0)
            return false;

        foreach (TalentNode connectedNode in myConnectedNodes)
        {
            if (connectedNode.Depth > Depth)
            {
                Debug.Log(connectedNode.ID + " is in Depth: " + connectedNode.Depth + ". Thats deeper than " + Depth + " of current Talent: " + ID);
                return true;
            }
        }

        return false;
    }

    // === GEM SOCKET METHODEN ===

    /// <summary>
    /// Erstellt einen neuen Gem-Socket-Node
    /// </summary>
    public static TalentNode CreateGemSocket(int id, TalentNode parent, int depth)
    {
        var socket = new TalentNode(id, parent, depth)
        {
            isGemSocket = true,
            myTypes = new List<TalentType>(), // Keine TalentTypes für Gem-Sockets
            myValue = 0,
            myMaxCount = 1, // Sockets können nur 1x geskillt werden (= Gem equipped)
            myCurrentCount = 0
        };
        return socket;
    }

    /// <summary>
    /// Prüft ob ein Gem in diesem Socket ausgerüstet werden kann
    /// </summary>
    public bool CanEquipGem(ItemInstance gem)
    {
        if (!isGemSocket) return false;
        if (gem == null || gem.itemType != ItemType.Gem) return false;
        if (equippedGem != null) return false; // Socket bereits belegt

        // Prüfe ob Gem eine Ability hat (über gemAbility-Referenz)
        bool hasAbility = (gem.gemAbility != null);
        
        if (hasAbility)
        {
            // Prüfe ob bereits ein Ability-Gem dieses GemTypes existiert
            if (abilityGemSockets.ContainsKey(gem.gemType) && abilityGemSockets[gem.gemType] != null)
            {
                Debug.LogWarning($"Bereits ein Ability-Gem vom Typ {gem.gemType} ausgerüstet!");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Rüstet ein Gem in diesem Socket aus
    /// </summary>
    public bool EquipGem(ItemInstance gem)
    {
        if (!CanEquipGem(gem)) return false;

        equippedGem = gem;

        // Wende Gem-Stats auf den Spieler an
        // Verwende PlayerStats.instance direkt (Singleton)
        if (PlayerStats.instance != null)
        {
            int multiplier = Mathf.Max(1, myCurrentCount); // Mindestens 1x
            
            // Erstelle für jeden Skillpunkt eigene Modifier direkt aus den Gem-Stats
            // OHNE das Gem-Objekt selbst zu verändern!
            
            // FlatStats verarbeiten
            foreach (var kvp in gem.flatStats)
            {
                if (!socketModifiers.ContainsKey(kvp.Key))
                    socketModifiers[kvp.Key] = new List<StatModifier>();
                
                // Erstelle einen Modifier pro Skillpunkt
                for (int i = 0; i < multiplier; i++)
                {
                    var mod = new StatModifier(kvp.Value, StatModType.Flat, this);
                    socketModifiers[kvp.Key].Add(mod);
                    PlayerStats.instance.GetStat(kvp.Key).AddModifier(mod);
                }
                
                Debug.Log($"[TalentNode] {multiplier}x FlatStat {kvp.Key} +{kvp.Value} = +{kvp.Value * multiplier} hinzugefügt");
            }
            
            // PercentStats verarbeiten
            foreach (var kvp in gem.percentStats)
            {
                if (!socketModifiers.ContainsKey(kvp.Key))
                    socketModifiers[kvp.Key] = new List<StatModifier>();
                
                // Erstelle einen Modifier pro Skillpunkt
                for (int i = 0; i < multiplier; i++)
                {
                    var mod = new StatModifier(kvp.Value, StatModType.PercentAdd, this);
                    socketModifiers[kvp.Key].Add(mod);
                    PlayerStats.instance.GetStat(kvp.Key).AddModifier(mod);
                }
                
                Debug.Log($"[TalentNode] {multiplier}x PercentStat {kvp.Key} +{kvp.Value * 100}% = +{kvp.Value * multiplier * 100}% hinzugefügt");
            }
            
            Debug.Log($"Gem '{gem.ItemName}' Stats angewendet auf Spieler (Multiplier: {multiplier}x wegen {myCurrentCount} Skillpunkten)");
        }
        else
        {
            Debug.LogError($"PlayerStats.instance ist null - Gem Stats konnten nicht angewendet werden!");
        }

        // Registriere Ability-Gems (prüfe gemAbility-Referenz)
        bool hasAbility = (gem.gemAbility != null);
        if (hasAbility)
        {
            abilityGemSockets[gem.gemType] = this;
            Debug.Log($"Ability-Gem vom Typ {gem.gemType} in Socket {ID} registriert");
        }

        return true;
    }

    /// <summary>
    /// Entfernt das Gem aus diesem Socket
    /// </summary>
    public ItemInstance UnequipGem()
    {
        if (equippedGem == null) return null;

        var gem = equippedGem;
        
        // Entferne Gem-Stats vom Spieler
        // Verwende PlayerStats.instance direkt (Singleton)
        if (PlayerStats.instance != null)
        {
            // Entferne alle Modifier die wir beim Equippen erstellt haben
            foreach (var kvp in socketModifiers)
            {
                foreach (var mod in kvp.Value)
                {
                    PlayerStats.instance.GetStat(kvp.Key).RemoveModifier(mod);
                }
                Debug.Log($"[TalentNode] {kvp.Value.Count}x Modifier für {kvp.Key} entfernt");
            }
            
            // Leere die Modifier-Listen
            socketModifiers.Clear();
            
            Debug.Log($"Gem '{gem.ItemName}' Stats vom Spieler entfernt");
        }
        else
        {
            Debug.LogError($"PlayerStats.instance ist null - Gem Stats konnten nicht entfernt werden!");
        }
        
        // Deregistriere Ability-Gems (prüfe gemAbility-Referenz)
        bool hasAbility = (gem.gemAbility != null);
        if (hasAbility && abilityGemSockets.ContainsKey(gem.gemType))
        {
            if (abilityGemSockets[gem.gemType] == this)
            {
                abilityGemSockets.Remove(gem.gemType);
                Debug.Log($"Ability-Gem vom Typ {gem.gemType} aus Socket {ID} deregistriert");
            }
        }

        equippedGem = null;
        // myCurrentCount bleibt erhalten - Skillpunkte nicht zurücksetzen!

        return gem;
    }

    /// <summary>
    /// Gibt den aktuellen Ability-Gem-Socket für einen GemType zurück (oder null)
    /// </summary>
    public static TalentNode GetAbilityGemSocket(GemType gemType)
    {
        return abilityGemSockets.ContainsKey(gemType) ? abilityGemSockets[gemType] : null;
    }

    /// <summary>
    /// Berechnet die Gesamtzahl der Skillpunkte, die in einen bestimmten GemType investiert wurden
    /// (für Ability-Verstärkung durch Support-Gems)
    /// </summary>
    public static int GetTotalSkillpointsForGemType(GemType gemType)
    {
        if (TalentTreeGenerator.instance == null) return 0;

        int totalSkillpoints = 0;
        
        foreach (var node in TalentTreeGenerator.instance.allNodes)
        {
            if (node.isGemSocket && node.equippedGem != null && node.equippedGem.gemType == gemType)
            {
                totalSkillpoints += node.myCurrentCount; // Sollte immer 1 sein für equipped Gems
            }
        }

        Debug.Log($"Gesamte Skillpunkte für GemType {gemType}: {totalSkillpoints}");
        return totalSkillpoints;
    }

    /// <summary>
    /// Setzt das Ability-Gem-Tracking zurück (z.B. beim Laden eines Savegames)
    /// </summary>
    public static void ResetAbilityGemTracking()
    {
        abilityGemSockets.Clear();
        Debug.Log("Ability-Gem-Tracking zurückgesetzt");
    }

}



// Definiert die verschiedenen Talentarten
public enum TalentType
{
    HP, AS, AD, AR, AP, RE //MS als Node ist bisschen OP..
}
