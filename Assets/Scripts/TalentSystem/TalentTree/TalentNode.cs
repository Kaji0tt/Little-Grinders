using System;
using System.Collections.Generic;
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

}



// Definiert die verschiedenen Talentarten
public enum TalentType
{
    HP, AS, AD, AR, AP, RE //MS als Node ist bisschen OP..
}
