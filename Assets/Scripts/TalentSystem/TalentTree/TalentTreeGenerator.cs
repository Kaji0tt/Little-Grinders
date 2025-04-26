using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TalentTreeGenerator : MonoBehaviour
{
    public static TalentTreeGenerator instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Design TalentTree")]
    public float baseChildRadius = 120f;  // Basisabstand vom Parent
    public float depthScalingFactor = 0.3f; // Skalierung je Tiefe


    public float angleVariation = 50f;  // Zufällige Streuung des Winkels
                                         // -> Sollte eigentlich 360° sein, damit auch "rückwärts" richtung Ursprung erkundet werden kann.

    public float sectionWeightMultiplier = 15;
    //public float hybridBorderProbability = 5;
    public int depthGeneration = 2; // Ab wann die nächsten Nodes gespawned werden sollen. "Sichtweite im TalentTree"

    // Definiere das Mindestgewicht für weit entfernte TalentTypes
    private float minWeight = 0.01f; // Beispielwert, anpassen je nach Bedarf

    [Header("Editor Components and Instances")]
    public Talent_UI myTalentUI; // UI Prefab für Nodes
    public Transform canvasTransform; // Referenz zum Canvas
    public Transform talentLinesParent;
    public LineRenderer linePrefab; // Prefab für Verbindungen
    public Talent_UI Ursprung;


    public List<TalentNode> allNodes = new List<TalentNode>(); // Alle erzeugten Knoten
    private int nextID = 0;



    private void Start()
    {
        //1.
        GenerateTree();

        List<TalentNode> hybridTalents = new List<TalentNode>();

        //Debug.Log um zu überprüfen, ob hybride Talente generiert worden sind.
        foreach (TalentNode talent in allNodes)
        {


            if(talent.myTypes.Count > 1)
            {
                hybridTalents.Add(talent);
            }
        }

        //Debug.Log(hybridTalents.Count);
    }

    //2.
    public void GenerateTree()
    { 
        TalentType[] rootTypes = { TalentType.HP, TalentType.AS, TalentType.AD, TalentType.AR, TalentType.AP, TalentType.RE };

        // 2.1. Root-Nodes erstellen (ohne ExpandNode)
        for (int i = 0; i < 6; i++)
        {
            TalentNode rootNode = new TalentNode(nextID++, null, 0, rootTypes[i]);
            allNodes.Add(rootNode);

        }


        // 2.2. Erst jetzt UI-Elemente für alle Root-Nodes zeichnen
        SetAndDrawRootNodes();

        // 2.3. Erzeuge eine Kopie der ursprünglichen Root-Nodes und expandiere von diesen ausgehend.
        List<TalentNode> rootNodes = new List<TalentNode>(allNodes.FindAll(n => n.Depth == 0));

        foreach (TalentNode root in rootNodes)
        {
           
            ExpandNode(root);
            TalentTreeManager.instance.UpdateTalentTree(root);

        }

        // 2.4. Generiere den Tree bis zum Ende von depthGeneration
        for(int i = 1; i <= depthGeneration; i++)
        {
 
            foreach(TalentNode node in GetNodesAtDepth(i))
            {
                ExpandNode(node);
            }

        }
    }

    List<TalentNode> GetNodesAtDepth(int i)
    {
        List<TalentNode> depthNodes = new List<TalentNode>(allNodes.FindAll(n => n.Depth == i));

        return depthNodes;
    }

    /// <summary>
    /// Configuring the Rootnodes of the TalentTree and adding them to the Index.
    /// </summary>
    private void SetAndDrawRootNodes()
    {
        float radius = 300f; // Abstand der Starttalente vom Zentrum
        //Vector3 origin = new Vector3(0, 0, 0); // Ursprung des Talentbaums


        foreach (TalentNode rootNode in allNodes)
        {
            Vector3 position;

            //Für alle Ursprungstalente
            if (rootNode.Depth == 0)
            {
                // Index für korrekten Winkel
                int index = allNodes.IndexOf(rootNode);

                // Gleichmäßige Verteilung um den Ursprung
                float angle = (index / 6f) * 360f;

                // Grad in Radiant umwandeln
                float radians = angle * Mathf.Deg2Rad; 

                position = new Vector3(
                    Mathf.Cos(radians) * radius, // X-Position
                    Mathf.Sin(radians) * radius, // Y-Position
                    0
                );
            }

            //Scheint keine großen Sinn zu verfolgen aktuell
            else
            {
                // Normal weiter nach unten verzweigen
                float xOffset = 200f;
                float yOffset = -100f;
                position = new Vector3(rootNode.Depth * xOffset, rootNode.ID * yOffset, 0);
            }



            //Setze die entsprechenden Variabeln
            rootNode.myPosition = position;

            //Generiere das Interface UI Objekt
            Talent_UI newNode_UI = Instantiate(myTalentUI, canvasTransform);
            newNode_UI.SetNode(rootNode);

        }
    }

    //Methode um den Talentbaum ausgehend von TalentNode parent zu vergrößern
    public void ExpandNode(TalentNode parent)
    {
        Debug.Log("Called by TalentID: " + parent.ID + " with Value " + string.Join("/", parent.myTypes));
        //Würfel eine zufällige Zahl für die neuen Kinder
        int numChildren = UnityEngine.Random.Range(2, 5); // 2 bis 5 Kinder

        Debug.Log("Should Generate atleast " + numChildren + " Children");

        /*
        //Bei Ursprungstalenten erstelle aufjedenfall 3 Nodes
        if (parent.Depth == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                TalentNode newNode = CreateChildNode(parent);

                Vector2 validPosition;
                TalentNode overlappingNode;

                // Wiederhole solange, bis ein Overlap gefunden wurde:
                while (!CheckForOverlap(parent, out validPosition, out overlappingNode))
                {
                    // Hier könntest du ggf. Debug.Log schreiben oder Position neu berechnen
                }

                // Jetzt kannst du z. B. den gefundenen Overlap verwenden
                Debug.Log("Overlapping Node gefunden: " + overlappingNode.ID);
            }
        }
        */

        //Für jedes Kind
        for (int i = 0; i < numChildren; i++)
        {
            //Erstelle eine ChildNode mit den Infos: Tiefe, ID und Parent als connectedTalent.
            TalentNode newNode = CreateChildNode(parent);

            Vector2 validPosition;

            if (!CheckForOverlap(parent, out validPosition, out TalentNode overlappingNode))
            {
                Debug.Log("Child " + i + " hat keine Überlappung gefunden. Erstelle neue Node.");

                //Füge anhand der Position des Talents ein Type hinzu.
                newNode.myPosition = validPosition;
                newNode.myTypes.Add(GetTalentType(newNode.myPosition));

                // Erstellt das UI-Element NACH erfolgreicher Positionierung
                Talent_UI uiNode = Instantiate(myTalentUI, canvasTransform);

                //newNode.uiElement = uiNode.GetComponent<Transform>();

                uiNode.SetNode(newNode);

                // Füge die grafische Oberfläche des neuen Nodes hinzu und überarbeite ziehe eine Verbindung
                allNodes.Add(newNode);

                parent.myConnectedNodes.Add(newNode);
                newNode.myConnectedNodes.Add(parent);

                // **Verbindung sofort zeichnen**: Parent zu Child
                if (newNode.Depth != 0) 
                {

                    DrawConnection(parent.uiElement.GetComponent<RectTransform>(), uiNode.myRectTransform);


                    // Überprüft, ob das Talent ein Hybrid-Talent sein sollte
                    CheckAndAssignHybridType(newNode, parent);

                    if (newNode.myTypes.Count > 1)
                        newNode.myValue /= 2;
                }




            }
            else if(!parent.myConnectedNodes.Contains(overlappingNode))
            {
                Debug.Log("Child " + i + " hat eine Überlappung gefunden. Ergänze Verbindung, da nicht vorhanden.");
                // **Verbindung zum neuen Knoten zeichnen
                DrawConnection(parent.uiElement.GetComponent<RectTransform>(), overlappingNode.uiElement.GetComponent<RectTransform>());
                overlappingNode.myConnectedNodes.Add(parent);
                overlappingNode.uiElement.GetComponent<Talent_UI>().Unlockable();

                parent.myConnectedNodes.Add(overlappingNode);
                parent.uiElement.GetComponent<Talent_UI>().Unlockable();
            }
            else
            {
                Debug.Log("Child " + i + " hat eine Überlappung gefunden. Verbindung bereits vorhanden.");
            }

            //uiNode.GetComponentInChildren<Text>().text = string.Join("/", childNode.myTypes) + ": " + childNode.myCurrentCount + "/" + childNode.myMaxCount;
        }
    }

    private TalentNode CreateChildNode(TalentNode parent)
    {
        // Erstellt ein neues TalentNode mit einer eindeutigen ID und zugehörigen Informationen
        int childID = nextID++;
        int newDepth = parent.Depth + 1;

      

            return new TalentNode(childID, parent, newDepth);
    }


    private bool CheckForOverlap(TalentNode parent, out Vector2 validPosition, out TalentNode overlappingNode)
    {
        validPosition = Vector2.zero;
        //overlappingNode = null;

        //Abstand zum Ursprungstalent:
        //Erschließt sich aus den im Inspektor definierbaren Wert "baseChildRadius" und dem "depthScalingFactor
        float childRadius = UnityEngine.Random.Range(baseChildRadius, baseChildRadius * (1 + depthScalingFactor));

        //Winkel zum Ursprungstalent:
        //Erschließt sich ebenfalls aus dem im Inspektor definierten Wert.
        float angleOffset = GetAngleOfChild(parent.Depth);

        //Unsicher, Magie von Chatti:
        //Vermutlich der Winkel des Ursprungstalents im Verhältnis zum "Ursprung" des UIs(0.0)
        float parentAngle = Mathf.Atan2(
            parent.myPosition.y,
            parent.myPosition.x) * Mathf.Rad2Deg;

        //Berechnung des letztlich gültigen Winkels im Versatz...
        float childAngle = (parentAngle + angleOffset) * Mathf.Deg2Rad;

        Debug.Log("Winkel des Kindes: " + childAngle);
        //und übernahme in 2 floats zur Konstruktion eines Vektors.
        float childX = parent.myPosition.x + Mathf.Cos(childAngle) * childRadius;
        float childY = parent.myPosition.y + Mathf.Sin(childAngle) * childRadius;
        Vector2 newPosition = new Vector2(childX, childY);

        //Überprüfe ob es an der neuen Position bereits einen Node gibt, ...
        TalentNode foundNode = IsOverlappingWithNode(newPosition);

        //... fals kein Node an der "newPosition gefunden wurde:
        if (foundNode == null)
        {
            //Gebe die neue Position aus, setze Bool auf True
            //Debug.Log("Es wurde kein Node zum Überlappen gefunden! Stattdessen wird eine Position zurück gegeben:" + newPosition);
            validPosition = newPosition;
            overlappingNode = null;
            return false;
        }
        else
        {
            //Ansonsten gebe die gefundene Node aus, setze Bool auf False
            overlappingNode = foundNode;
            //Debug.Log("Node wurde gefunden! Wert:" + string.Join(", ", foundNode.myTypes));
            return true;
        }



    }

    private float GetAngleOfChild(int depth)
    {
        // Variablen aus dem Inspector oder definier sie oben
        float minAngle = angleVariation; // z. B. 45°
        float maxAngle = 210f; // Maximales erlaubtes Offset bei großem Depth

        // Depth-basierte Skalierung (z. B. Depth 0 = min, Depth 10+ = max)
        float t = Mathf.Clamp01(depth / 8f); // normalisiert zwischen 0 und 1
        float allowedAngleRange = Mathf.Lerp(minAngle, maxAngle, t); // Interpoliert von min zu max

        // Zufälliges Offset innerhalb des erlaubten Bereichs
        return UnityEngine.Random.Range(-allowedAngleRange, allowedAngleRange);
    }

    public List<TalentNode> ClaculateBranch(TalentNode startNode, int targetDepth)
    {
        // Ergebnisliste für alle Nodes in der gewünschten Tiefe
        List<TalentNode> result = new List<TalentNode>();

        // Set zur Vermeidung von Zyklen – speichert alle bereits besuchten Nodes
        HashSet<TalentNode> visited = new HashSet<TalentNode>();

        // Warteschlange für die BFS. Jeder Eintrag besteht aus einem Tuple<Node, aktuelle Tiefe>
        Queue<(TalentNode node, int depth)> queue = new Queue<(TalentNode, int)>();

        // Initialisiere die Suche mit dem Startknoten auf Tiefe 0
        queue.Enqueue((startNode, 0));
        visited.Add(startNode);

        while (queue.Count > targetDepth)
        {
            var (currentNode, currentDepth) = queue.Dequeue();

            // Wenn wir die gewünschte Tiefe erreicht haben, füge den Node zur Ergebnisliste hinzu
            if (currentDepth <= targetDepth)
            {
                //Debug.Log("Füge Node-ID:" + currentNode.ID + " mit dem Value:" + currentNode.myValue + " hinzu.");
                result.Add(currentNode);
                continue;
            }

            // Wenn die Tiefe kleiner ist, durchlaufe die Nachbarn

            foreach (var neighbor in currentNode.myConnectedNodes)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, currentDepth + 1));
                }
            }

        }

        return result;
    }



    private void CheckAndAssignHybridType(TalentNode childNode, TalentNode parent)
    {
        if (parent.Depth <= 1) return; // Keine Hybrid-Talente für Root-Nodes

        float sectorSize = 360f / 6; // 6 Talent-Sektoren
        Vector2 childPos = childNode.uiElement.GetComponent<RectTransform>().anchoredPosition;
        float childAngle = Mathf.Atan2(childPos.y, childPos.x) * Mathf.Rad2Deg;
        if (childAngle < 0) childAngle += 360f; // Winkel positiv machen

        // Berechne den Ursprung des aktuellen Sektors (Mitte des Sektors)
        int sectorIndex = Mathf.FloorToInt(childAngle / sectorSize);  // Bestimmt, in welchem Sektor der Winkel des ChildNodes liegt
        float sectorCenter = sectorIndex * sectorSize; // Zentrum des Sektors

        // Berechne den Abstand zwischen dem Winkel des ChildNodes und dem Ursprung des aktuellen Sektors
        float distanceFromCenter = Mathf.Abs(Mathf.DeltaAngle(childAngle, sectorCenter));

        // Definiere einen Schwellenwert, um zu überprüfen, ob der Winkel weit genug vom Ursprung entfernt ist
        float hybridBorderThreshold = sectorSize / 4; // Beispielwert: 1/4 des Sektorbereichs

        bool isFarFromCenter = distanceFromCenter > hybridBorderThreshold;

        if (isFarFromCenter)
        {
            TalentType hybridType;
            int safetyCounter = 10; // Falls die Talentverteilung zu stark limitiert ist
            do
            {
                hybridType = GetTalentType(childPos);
                safetyCounter--;
            }
            while (childNode.myTypes.Contains(hybridType) && safetyCounter > 0);

            if (!childNode.myTypes.Contains(hybridType))
            {
                childNode.myTypes.Add(hybridType);

            }
        }
    }


    private TalentNode IsOverlappingWithNode(Vector2 position)
    {
        //Setze Mindest-Distanz für "Overlapping"
        float minDistance = 150f;

        //Durchsuche alle Talente nach einem "Overlapp"
        foreach (TalentNode other in allNodes)
        {
            float distance = Vector2.Distance(position, other.myPosition);
            if (distance < minDistance)
                //Falls "Overlapp" in allen Talenten gefunden wurde, gebe diesen aus.
                return other;
        }
        //Ansonsten gib null zurück.
        return null;
    }

    private TalentType GetTalentType(Vector2 position)
    {
        TalentType[] types = (TalentType[])Enum.GetValues(typeof(TalentType));
        int typeCount = types.Length;

        // Berechne den Winkel relativ zur Mitte
        float angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360; // Winkel positiv machen

        // Definiere die Mittelpunkte der TalentType-Sektoren
        float sectorSize = 360f / typeCount;
        Dictionary<TalentType, float> probabilities = new Dictionary<TalentType, float>();

        float totalWeight = 0f;

        // Berechne die Wahrscheinlichkeit für jeden TalentType
        foreach (TalentType type in types)
        {
            float sectorCenter = Array.IndexOf(types, type) * sectorSize;
            float distance = Mathf.Abs(Mathf.DeltaAngle(angle, sectorCenter)); // Kleinster Abstand in Grad

            // Je näher am Mittelpunkt des Sektors, desto höher die Wahrscheinlichkeit
            float weight = Mathf.Exp(-distance / sectionWeightMultiplier); // sectionWeightMultiplier sorgt dafür, wie stark die Nähe zum TalentType-Strang ins Gewicht fällt

            // Setze ein Mindestgewicht für weit entfernte TalentTypes
            weight = Mathf.Max(weight, minWeight);

            //Debug.Log(type.ToString() + " hat ein gewicht von " + weight);
            probabilities[type] = weight;
            totalWeight += weight;
        }


        //Debug.Log(" Das Gesamtgewicht: " + totalWeight);
        // Wähle einen TalentType basierend auf der Wahrscheinlichkeit
        float randomPoint = UnityEngine.Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var kvp in probabilities)
        {
            cumulative += kvp.Value;
            if (randomPoint <= cumulative)
            {
                return kvp.Key;
            }
        }

        return types[0]; // Fallback (sollte nie eintreten)
    }

    private TalentNode FindClosestNeighbor(TalentNode node)
    {
        float maxHybridDistance = 250f; // Maximale Entfernung für gültige Hybrid-Verbindung
        TalentNode closestNeighbor = null;
        float minDistance = maxHybridDistance; // Nur Knoten innerhalb dieser Distanz zulassen

        foreach (TalentNode other in allNodes)
        {
            if (other == node) continue;

            float distance = Vector2.Distance(
                node.uiElement.GetComponent<RectTransform>().anchoredPosition,
                other.uiElement.GetComponent<RectTransform>().anchoredPosition);

            if (distance < minDistance && SameSector(node, other))
            {
                minDistance = distance;
                closestNeighbor = other;
            }
        }
        return closestNeighbor;
    }

    // Prüft, ob zwei Knoten im selben Talent-Sektor liegen
    private bool SameSector(TalentNode a, TalentNode b)
    {
        float sectorSize = 360f / 6; // Sechs Talent-Sektoren
        float angleA = GetAngle(a);
        float angleB = GetAngle(b);

        int sectorA = Mathf.FloorToInt(angleA / sectorSize);
        int sectorB = Mathf.FloorToInt(angleB / sectorSize);

        return Mathf.Abs(sectorA - sectorB) <= 1; // Direkt benachbarte Sektoren sind erlaubt
    }

    // Berechnet den Winkel eines Knotens
    private float GetAngle(TalentNode node)
    {
        Vector2 pos = node.uiElement.GetComponent<RectTransform>().anchoredPosition;
        return Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
    }


    public void DrawConnection(RectTransform parentRect, RectTransform childRect)
    {
        GameObject lineGO = new GameObject("_TalentLine", typeof(UILineRenderer));
        lineGO.transform.SetParent(talentLinesParent, false); // Setzt das Parent Transform auf das übergeordnete Node

        UILineRenderer lineRend = lineGO.GetComponent<UILineRenderer>();
        lineRend.color = new Color(50f / 255f, 50f / 255f, 50f / 255f, 0.8f);
        lineRend.LineThickness = 4f;

        // Mittelpunkt der RectTransforms berechnen
        Vector2 parentCenter = parentRect.anchoredPosition + parentRect.sizeDelta / 2f;
        Vector2 childCenter = childRect.anchoredPosition + childRect.sizeDelta / 2f;

        List<Vector2> linePoints = new List<Vector2> { parentCenter, childCenter };
        lineRend.Points = linePoints.ToArray();
    }

}


