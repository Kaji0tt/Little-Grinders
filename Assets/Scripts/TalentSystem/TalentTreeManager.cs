using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;



public class TalentTreeManager : MonoBehaviour
{

    #region Singleton
    public static TalentTreeManager instance;
    private void Awake()
    {
        instance = this;

        //FindObjectsByType<Talent_UI>(FindObjectsSortMode.None); // true, um auch inaktive Objekte einzuschließen

        allTalents.Clear();

        foreach (Talent_UI talent in FindObjectsByType<Talent_UI>(FindObjectsSortMode.None))
        {
            allTalents.Add(talent);
            talent.SetTalentUIVariables();
        }

        //Sorge dafür, das alle Talente locked sind, welche zu Beginn nicht freigeschaltet sind.
        ResetTalents();

        //Ziehe die 
        //DrawTalentTreeLines();

        UpdateTalentPointText();



    }


    #endregion

    //Verweis auf ein Talent im Inspektor, das als Grundlage für die Erstellung aus der Datenbank des TalentTreeGenerators gilt.
    public Talent_UI talentUI;

    public Transform parentGameObject;

    public Ability startAbility; // Ursprung ist eine Ability

    public List<Talent_UI> allTalents = new List<Talent_UI>();  // Alle erzeugten Talente im UI

    //talentLineParent sollte jedes AbilityTalent sein.
    public Transform talentLineParent;




    [SerializeField]
    public Talent_UI defaultTalent; 

    [SerializeField]
    private Text talentPointText;


    public Sprite hpIcon, arIcon, asIcon, adIcon, apIcon, reIcon, defaultIcon;


    public void TryUseTalent(Talent_UI clickedTalent)
    {

        
        //Nur wenn der Spieler auch über Skillpunkte verfügt
        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && clickedTalent.Click())
        {
            //Füge die EIgenschaften des Talents dem Spieler hinzu
            clickedTalent.ApplyPassivePointsAndEffects(clickedTalent);

            PlayerManager.instance.player.GetComponent<PlayerStats>().Decrease_SkillPoints(1);
            UpdateTalentPointText();
            if(clickedTalent.myNode != null)
            {
                clickedTalent.myNode.myCurrentCount++;
                UpdateTalentTree(clickedTalent.myNode);
            }


            //Zusätzliche Abfrage für die Root Ability (Sie kann durch ein bool in UseBase nur 1x verwendet werden)
            if (clickedTalent.myAbility != null)
                clickedTalent.myAbility.UseBase(PlayerStats.instance);

            //Füge das Talent der Liste von Talenten hinzu.
            allTalents.Add(clickedTalent);

            //if (ClaculateBranch(clickedTalent.myNode, TalentTreeGenerator.instance.depthGeneration + clickedTalent.myNode.Depth).Count >= 2)
            /*
            if(GetLeafNodes(clickedTalent.myNode).Count >= 2)
            foreach(TalentNode leaf in GetLeafNodes(clickedTalent.myNode))
            {
                if(leaf.Depth <= clickedTalent.myNode.Depth + TalentTreeGenerator.instance.depthGeneration)
                {
                    TalentTreeGenerator.instance.ExpandNode(leaf);
                }
            }
            else
            {
                //ClaculateBranch(clickedTalent.myNode, clickedTalent.myNode.Depth + TalentTreeGenerator.instance.depthGeneration);
            }
            */

        }        

    }



    void OnEnable()
    {
        PlayerStats.eventLevelUp += UpdateTalentPointText;


    }

    void OnDisable()
    {
        PlayerStats.eventLevelUp -= UpdateTalentPointText;
    }



    public void ResetTalents()
    {
        foreach (Talent_UI talent in allTalents)
        {
            if (talent == defaultTalent)
                talent.Unlock();
            else
                talent.Lock();
        }

        UpdateTalentPointText();
    }

    public void UpdateTalentPointText()
    {
       
        if(PlayerManager.instance.player != null)
        {
            talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();
        }

    }
    public void UpdateTalentTree(TalentNode node)
    {
        // Aktualisiere den Text auf dem UI-Element (z.B. 1/3)
        node.uiElement.GetComponentInChildren<Text>().text = node.myCurrentCount + "/" + node.myMaxCount;

        // Falls es sich um ein Wurzeltalent handelt (Tiefe == 0)
        if (node.Depth == 0)
        {
            // Wenn Spieler eine bestimmte Fähigkeit (Regeneration) besitzt
            if (PlayerStats.instance.Regeneration.Value >= 1)
                node.uiElement.GetComponent<Talent_UI>().Unlock();
            else
                node.uiElement.GetComponent<Talent_UI>().Lock();
        }

        // Finde alle Endpunkte (Leafs) des aktuellen Talentbaums
        /*
        List<TalentNode> leafNodes = GetLeafNodes(node);

        foreach (TalentNode leaf in leafNodes)
        {
            if(leaf.Depth >= 0)
            {
                var nodesAtDepth = GetDepthDistance(leaf, TalentTreeGenerator.instance.depthGeneration);

                if (nodesAtDepth.Count == 0)
                {
                    // Keine weiteren Talente in Reichweite → Expand vom Ende aus
                    TalentTreeGenerator.instance.ExpandNode(leaf);
                }
            }

        }*/
        /*
        foreach(TalentNode leaf in ClaculateBranch(node, TalentTreeGenerator.instance.depthGeneration))
        {
            // Keine weiteren Talente in Reichweite → Expand vom Ende aus
            TalentTreeGenerator.instance.ExpandNode(leaf);
        }
        */
        // Schalte verbundene Talente frei, wenn dieser Knoten voll geskillt ist
        foreach (TalentNode neighborNode in node.myConnectedNodes)
        {
            if (neighborNode != null && node.myCurrentCount >= node.myMaxCount)
            {
                neighborNode.uiElement.GetComponent<Talent_UI>().Unlock();
            }

        }
        
    }

    private bool IsAnyNeighborFullySkilled(TalentNode node)
    {
        if (node?.myConnectedNodes == null || node.myConnectedNodes.Count == 0)
            return false;

        foreach (TalentNode neighbor in node.myConnectedNodes)
        {
            
            if (neighbor != null && neighbor.myCurrentCount >= neighbor.myMaxCount)
                return true;  // Mindestens ein Elternteil ist voll geskillt → return true
        }

        return false; // Kein Elternteil ist voll geskillt → return false
    }

    // Gibt alle TalentNodes zurück, die in einer bestimmten Tiefe (Entfernung in "Hops") vom StartNode aus liegen.
    // Die Tiefe wird hier als Anzahl der Verbindungen (Edges) gezählt.
    // Verwendet eine Breitensuche (Breadth-First Search), um die TalentNodes auf der gewünschten Tiefe zu finden.
    // Es wird nicht mehr mit einer Parent/Child-Hierarchie gearbeitet, sondern mit dem Graph der verbundenen Knoten.
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
                Debug.Log("Füge Node-ID:" + currentNode.ID + " mit dem Value:" + currentNode.myValue + " hinzu.");
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

    // Gibt alle "Enden" des Talentbaums zurück, beginnend beim gegebenen Startknoten.
    // Ein Leaf ist ein Knoten, der entweder keine Connections hat oder nur zu bereits besuchten Nodes.
    public List<TalentNode> GetLeafNodes(TalentNode startNode)
    {
        List<TalentNode> leafNodes = new List<TalentNode>();
        HashSet<TalentNode> visited = new HashSet<TalentNode>();
        Queue<TalentNode> queue = new Queue<TalentNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            TalentNode current = queue.Dequeue();
            bool hasUnvisitedChildren = false;

            foreach (var neighbor in current.myConnectedNodes)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    hasUnvisitedChildren = true;
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            if (!hasUnvisitedChildren)
            {
                // Wenn keine weiteren Verbindungen → dieser Node ist ein "Leaf"
                leafNodes.Add(current);
            }
        }

        return leafNodes;
    }
}
