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

    //public List<TalentNode> myChildren; // Verknüpfte Talente

    //public List<TalentNode> myParents; // Vorgänger-Knoten

    public List<TalentNode> myConnectedNodes = new List<TalentNode>();

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


    //Generierung der Urpsrungstalente 
    public TalentNode(int id, TalentNode parent, int depth, TalentType type)
    {
        ///Initialisieren
        //myParents = new List<TalentNode>();
        //myChildren = new List<TalentNode>();
        myConnectedNodes = new List<TalentNode>();
        myConnectedNodes.Add(parent);

        //if (parent != null) myConnectedNodes.Add(parent);
        myTypes.Add(type);


        ID = id;

        Depth = depth;


        // Startwerte setzen
        myCurrentCount = 0;

        // Hauptäste (Tiefe 0) haben immer myMaxCount = 3, andere variiert zwischen 1 und 5
        myMaxCount = (depth == 0) ? 3 : UnityEngine.Random.Range(1, 6);

        // myValue abhängig von der Tiefe berechnen
        myValue = CalculateValue(depth);


    }

    //Konstruktor ohny TalenType, für spätere Zuweisung im Generator.
    public TalentNode(int id, TalentNode parent, int depth)
    {
        //myParents = new List<TalentNode>();
        //myChildren = new List<TalentNode>();
        myConnectedNodes = new List<TalentNode>();
        myConnectedNodes.Add(parent);
        myTypes = new List<TalentType>(); // Noch leer
        //myPosition = position;

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
