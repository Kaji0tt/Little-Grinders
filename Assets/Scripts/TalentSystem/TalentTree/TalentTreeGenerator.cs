using System;
using System.Collections.Generic;
using System.Linq;
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

    public float sectionWeightMultiplier = 15; // sectionWeightMultiplier sorgt dafür, wie stark die Nähe zum TalentType-Strang bei der Auswahl eines TalentTyps ins Gewicht fällt
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

        // 2.3 Erweitere den Ast der Baumes ausgehend von den Rootnodes
        for (int i = 0; i <= depthGeneration; i++)
        {
            foreach (TalentNode node in GetNodesAtDepth(i))
            ExpandNode(node);         
        }

    }

    //3.1 Führe BFS (Bredth First Search) aus, indem alle Nodes ausgehend von einem bestimmten (dem geklickten Talent) durchgegnagen werden.
    //Wenn in einer Entfernungn von 2 (depthGeneration) zum geklickten Talent, Talente gefunden werden, gebe diese in einer Liste wieder.
    public void ExpandBranch(TalentNode clickedTalent)
    {

        List<TalentNode> nodes2faraway;
        nodes2faraway = ClaculateAllNodesInDistanceTo(clickedTalent, depthGeneration, true);
        foreach (TalentNode foundNode in nodes2faraway)
        {
            ExpandNode(foundNode);
        }
    }

    //Erhalte alle Nodes in einer bestimmten Tiefe.
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

            //Setze die entsprechenden Variabeln
            rootNode.myPosition = position;

            //Generiere das Interface UI Objekt
            Talent_UI uiNode = Instantiate(myTalentUI, canvasTransform);

            //Setze die Node infos.
            uiNode.SetNode(rootNode);

        }
    }

    /// <summary>
    /// Erweitert den Talentbaum ausgehend vom gegebenen Parent-Node.
    /// Fügt genau numChildren neue, tieferliegende Nodes hinzu oder verbindet sie.
    /// </summary>
    /// <param name="parent">Der Ausgangs-Knoten, von dem neue Talente verzweigen sollen</param>
    public void ExpandNode(TalentNode parent)
    {

        // Wenn der Knoten bereits das Maximum an Verbindungen hat, verlasse die Methode
        if (parent.myConnectedNodes.Count >= 3)
        {
            //Debug.Log($"Node {parent.ID} hat bereits die maximale Anzahl an Verbindungen (3). Abbruch.");
            return;
        }

        int numChildren;

        // Bestimme, wie viele Kinder erstellt werden sollen
        if (parent.Depth == 0)
        {
            numChildren = 3; // Wurzel-Nodes starten mit 3 Kindern
        }
        else
        {
            numChildren = UnityEngine.Random.Range(2, 5); // 2 bis 4 Kinder bei tieferen Knoten
        }

        //Debug.Log("Starte Expansion für Node " + parent.ID + " → Ziel: " + numChildren + " neue Verbindungen.");

        int createdChildren = 0;                // Zähler für erfolgreich hinzugefügte Nodes oder Verbindungen
        int attempts = 0;                       // Wie oft versucht wurde, eine Position zu generieren
        int maxAttempts = numChildren * 10;     // Sicherheitsgrenze (z. B. 30 Versuche bei 3 Kindern)

        // Wiederhole solange, bis genug Kinder erstellt wurden oder zu viele Versuche unternommen wurden
        while (createdChildren < numChildren && attempts < maxAttempts)
        {
            attempts++;                         // Jeder Schleifendurchlauf zählt als Versuch

            // Versuche, eine neue Position zu finden, ohne mit bestehenden Nodes zu überlappen
            if (!IsTheNewNodeOverlaping(parent, out Vector2 validPosition, out TalentNode overlappingNode))
            {
                //Debug.Log($"[Versuch {attempts}] Keine Überlappung gefunden – neue Node wird erstellt.");

                // Erstelle neue Node, weise Position und Typ zu
                TalentNode newNode = CreateChildNode(parent);
                newNode.myPosition = validPosition;
                newNode.myTypes.Add(GetTalentType(newNode.myPosition));

                // UI-Element instanziieren und dem Node zuweisen
                Talent_UI uiNode = Instantiate(myTalentUI, canvasTransform);
                uiNode.SetNode(newNode);

                // Node in Datenstruktur aufnehmen
                allNodes.Add(newNode);
                parent.myConnectedNodes.Add(newNode);
                newNode.myConnectedNodes.Add(parent);

                // Verbindung zeichnen und ggf. Typ-Anpassung bei Hybrid-Talenten
                if (newNode.Depth != 0)
                {
                    DrawConnection(parent.uiElement.GetComponent<RectTransform>(), uiNode.myRectTransform);
                    CheckAndAssignHybridType(newNode, parent);

                    // Hybrid-Talente sind schwächer: Wert halbieren
                    if (newNode.myTypes.Count > 1)
                        newNode.myValue /= 2;
                }

                createdChildren++; // Erfolgreich hinzugefügt
            }
            else
            {
                // Überlappender Knoten darf nicht bereits verbunden sein UND beide Knoten dürfen nicht voll sein
                bool canConnect = !parent.myConnectedNodes.Contains(overlappingNode) &&
                                  parent.myConnectedNodes.Count < 3 &&
                                  overlappingNode.myConnectedNodes.Count < 3 &&
                                  overlappingNode.Depth > parent.Depth;


                if (canConnect)
                {
                    // Die Position ist belegt, aber eine gültige Verbindung ist noch nicht vorhanden
                    //Debug.Log($"[Versuch {attempts}] Überlappung mit Node {overlappingNode.ID}, noch keine Verbindung – Verbindung wird hergestellt.");

                    // Zeichne Verbindung in UI
                    DrawConnection(
                        parent.uiElement.GetComponent<RectTransform>(),
                        overlappingNode.uiElement.GetComponent<RectTransform>());

                    // Füge Verbindung hinzu
                    overlappingNode.myConnectedNodes.Add(parent);
                    overlappingNode.myTalentUI.Unlockable();

                    parent.myConnectedNodes.Add(overlappingNode);
                    parent.myTalentUI.Unlockable();

                    createdChildren++; // Verbindung zählt als Kind
                }
                else
                {
                    // Entweder bereits verbunden oder Tiefe passt nicht – ignoriere diesen Versuch
                    //Debug.Log($"[Versuch {attempts}] Überlappung mit ungültigem oder bereits verbundenem Node – wird übersprungen.");
                }
            }
 
        }

        // Wenn maximale Anzahl an Versuchen erreicht wurde, gib eine Warnung aus
        if (attempts >= maxAttempts)
        {
            //Debug.LogWarning($"Maximale Versuchsanzahl erreicht für Node {parent.ID}. Nur {createdChildren} von {numChildren} Kindern konnten erstellt werden.");
        }

        // Markiere Node als "erweitert"
        parent.hasExpanded = true;
    }

    private TalentNode CreateChildNode(TalentNode parent)
    {
        // Erstellt ein neues TalentNode mit einer eindeutigen ID und zugehörigen Informationen
        int childID = nextID++;
        int newDepth = parent.Depth + 1;



        return new TalentNode(childID, parent, newDepth);
    }

    /// <summary>
    /// Überprüft, ob an der potenziellen Position eines neuen Child-Nodes bereits ein anderer Node existiert.
    /// Gibt entweder eine gültige neue Position oder einen existierenden überlappenden Node zurück.
    /// </summary>
    /// <param name="parent">Der übergeordnete Knoten, von dem der neue Child-Node ausgeht</param>
    /// <param name="validPosition">Ausgabeparameter: gültige neue Position für den Child-Node</param>
    /// <param name="overlappingNode">Ausgabeparameter: bereits existierender Node an der Zielposition (falls vorhanden)</param>
    /// <returns>
    /// true  → ein überlappender Node wurde gefunden (validPosition ist ungültig)
    /// false → keine Überlappung, validPosition ist nutzbar
    /// </returns>
    private bool IsTheNewNodeOverlaping(TalentNode parent, out Vector2 validPosition, out TalentNode overlappingNode)
    {
        // Initialisierung der Rückgabeparameter
        validPosition = Vector2.zero;
        overlappingNode = null;

        // --- Positionierung des neuen Child-Nodes ---

        // Bestimme zufällige Distanz (Radius) vom Parent zum Kind
        float childRadius = UnityEngine.Random.Range(baseChildRadius, baseChildRadius * (1 + depthScalingFactor));

        // Berechne den Winkel, in dem das neue Kind relativ zum Parent platziert werden soll
        float angleOffset = GetAngleOfChild(parent.Depth); // abhängig von der Tiefe

        // Aktueller Winkel des Parent-Nodes (vom Ursprung aus gesehen)
        float parentAngle = Mathf.Atan2(parent.myPosition.y, parent.myPosition.x) * Mathf.Rad2Deg;

        // Gesamtwinkel für das Kind = Parent-Winkel + Versatz
        float childAngle = (parentAngle + angleOffset) * Mathf.Deg2Rad;

        // Berechne Position des Kindes auf Basis von Radius und Winkel
        float childX = parent.myPosition.x + Mathf.Cos(childAngle) * childRadius;
        float childY = parent.myPosition.y + Mathf.Sin(childAngle) * childRadius;
        Vector2 newPosition = new Vector2(childX, childY);

        // --- Überprüfung auf Überlappung mit bestehenden Nodes ---

        // Finde, falls vorhanden, einen existierenden Node an der berechneten Position
        TalentNode foundNode = IsOverlappingWithNode(newPosition);

        if (foundNode == null)
        {
            // Keine Überlappung: gültige Position gefunden
            validPosition = newPosition;
            return false;
        }
        else
        {
            // Überlappung vorhanden: gebe den überlappenden Node zurück
            overlappingNode = foundNode;
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


    /// <summary>
    /// Ermittelt mit Hilfe von Breadth-First Search (BFS) alle TalentNodes,
    /// die innerhalb einer bestimmten Tiefe vom Start-Talent aus erreichbar sind.
    /// Optional kann angegeben werden, ob nur tieferliegende Nodes berücksichtigt werden sollen.
    /// </summary>
    /// <param name="clickedTalent">Start-Talentknoten für die Suche</param>
    /// <param name="onlyDeeperNodes">
    /// Wenn true, werden nur Nodes mit höherer Tiefe als der Startknoten zurückgegeben.
    /// Wenn false, werden alle Nodes innerhalb der erlaubten Tiefe zurückgegeben.
    /// </param>
    /// <returns>Liste erreichbarer TalentNodes je nach Tiefe und Auswahl</returns>
    public List<TalentNode> ClaculateAllNodesInDistanceTo(TalentNode clickedTalent, int distance, bool onlyDeeperNodes)
    {
        List<TalentNode> result = new();                        // Ergebnisliste
        HashSet<TalentNode> visited = new();                    // Um doppelte Besuche zu vermeiden
        Queue<(TalentNode node, int depth)> queue = new();      // BFS-Warteschlange (Node + aktuelle Tiefe)

        queue.Enqueue((clickedTalent, 0));                      // Startknoten einfügen
        visited.Add(clickedTalent);                             // Als besucht markieren

        int targetDepth = distance;                             // Maximale Tiefe aus Konfiguration

        while (queue.Count > 0)
        {
            var (currentNode, currentDepth) = queue.Dequeue();  // Nächstes Element verarbeiten

            // Kriterium: Nur dann zur Ergebnisliste hinzufügen, wenn Bedingung erfüllt ist
            if (!onlyDeeperNodes || currentNode.Depth > clickedTalent.Depth)
            {
                result.Add(currentNode);
            }

            // Wenn Tiefe noch nicht erreicht → Nachbarn durchsuchen
            if (currentDepth < targetDepth)
            {
                foreach (TalentNode neighbor in currentNode.myConnectedNodes)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);                             // Besucht markieren
                        queue.Enqueue((neighbor, currentDepth + 1));       // In Queue mit nächster Tiefe einfügen
                    }
                }
            }
        }

        // Debug-Ausgabe zur Analyse
        //Debug.Log("You Found " + result.Count + " Nodes via " + clickedTalent.ID);
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
                childNode.myTalentUI.SetNode(childNode);

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


