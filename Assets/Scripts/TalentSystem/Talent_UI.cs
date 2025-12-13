using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Jedes Talent sollte sich auf eine Ability beziehen. 
//In der Ability, werden in Abhängigkeit von den gewählten Talente Werte geändert, welche die Ability und ihre Spezialisierung beeinflussen.
//Alle Informationen über mögliche Ausrichtungen von Abilities, liegen somit in jeweils einzelnen Ability-Scriptable Objects.

//Die Base-Ability (derzeit Heilung) sollte kein SO sein, sondern von Monobehaviour geerbt werden. (Derzeit stellt Spell diese Erbschaft dar.)
//Das wäre vergleichbar zum Ability-Holder im Video: https://www.youtube.com/watch?v=ry4I6QyPw4E

//Damit wäre ein Talent der entsprechende Ability Holder - wenn ein Talent geskilled wird, werden in Abhängigkeit von TalentType die AbilityType Integers erhöht.

//values for passive talents
//public enum TalentType { Health, Regenration, Armor, AttackDamage, AbilityPower, Movement };

public class Talent_UI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    //Is this talent a passive talent? If not, set the ability.
    public bool passive;

    //if its not a passive, set the talent via inspector
    //HINT: the "EditorTalentPassivToggle.cs" manages the display in unity editor
    public Ability myAbility;

    //name of the talent, needed for loading / saving
    public string talentName;

    //set the value of this passive
    public List<TalentType> myTypes;

    public TalentNode myNode { get; private set; }

    //set the value of % increase, gained by 1 point in talent count
    public float value;

    //the commulated amount of the value, according to its set skillpoints.
    public int currentCount { get; private set; }
    public RectTransform circle;

    public RectTransform myRectTransform { get; private set;} 

    // Speichere die Original-Größe des Circles, um wiederholtes Multiplizieren zu vermeiden
    private Vector2 originalCircleSize; 

    public void Set_currentCount(int newCount)
    {
        currentCount = newCount;
    }

    //the maximum count of this passive talent
    [SerializeField]
    private int maxCount;

    //the descirption of the talent will be generated automaticly
    private string description;


    private void OwnStart()
    {
        myRectTransform = GetComponent<RectTransform>();

        if (myAbility != null)
        {
            Debug.Log("Diese my Ability ist nicht null wird gecalled.");
            description = "<b>Divine Root</b>\nUnlocks the talent tree and provides 1 passive health regeneration. \nThis is the foundation of your character's survival capabilities.";
            maxCount = 1;
        }

        //Debug.Log("I got enabled!");
        if (myNode == null)
        {
            myNode = GetComponent<TalentNode>();
            myNode.IsExpanded();
        }



        image = GetComponent<Image>();

        StartCoroutine(RotateCircle());
        circle.gameObject.SetActive(false);

    }





    public void SetNode(TalentNode node)
    {



        node.uiElement = GetComponent<Transform>();
        myRectTransform = GetComponent<RectTransform>();
        myNode = node;
        image = GetComponent<Image>();
        textComponent = GetComponentInChildren<Text>();

        // Speichere die Original-Größe des Circles beim ersten Aufruf
        if (originalCircleSize == Vector2.zero && circle != null)
        {
            originalCircleSize = circle.sizeDelta;
            Debug.Log($"[Talent_UI] originalCircleSize gespeichert: {originalCircleSize}");
        }

        if (node != null)
        {
            SetNodeInfo();
            SetTalentUIVariables();
            Unlockable();
            Lock();
            UpdateTalent();
            node.SetGameObject(this);
            
            // Gem-Visualisierung aktualisieren (wichtig für geladene Savegames)
            if (node.isGemSocket)
            {
                UpdateGemVisual();
            }
        }
        OwnStart();
    }

    private void SetNodeInfo()
    {

        //Debug.Log(myNode.myTypes.Count);
        //Setze Werte

        myTypes = myNode.myTypes;

        // === GEM SOCKET HANDLING ===
        if (myNode.isGemSocket)
        {
            // Zeige Gem-Socket-Sprite
            if (TalentTreeGenerator.instance.gemSocketSprite != null)
            {
                image.sprite = TalentTreeGenerator.instance.gemSocketSprite;
            }

            // Wenn ein Gem equipped ist, zeige dessen Icon
            if (myNode.equippedGem != null)
            {
                // TODO: Gem-Icon overlay zeigen
                // Könnte ein zweites Image-Element sein, das über dem Socket liegt
            }

            // Setze Größe basierend auf Original (nicht multiplizieren!)
            if (originalCircleSize != Vector2.zero)
            {
                circle.sizeDelta = originalCircleSize * 1.8f; // Gem Sockets am größten
                Debug.Log($"[Talent_UI] Gem Socket circle.sizeDelta gesetzt: {circle.sizeDelta}");
            }
        }
        else
        {
            // Normale Talent-Node: Setze Icon basierend auf TalentType
            var manager = TalentTreeManager.instance;
            foreach (TalentType type in myTypes)
            {
                icon = type switch
                {
                    TalentType.HP => image.sprite = manager.hpIcon,
                    TalentType.AP => image.sprite = manager.apIcon,
                    TalentType.AD => image.sprite = manager.adIcon,
                    TalentType.AR => image.sprite = manager.arIcon,
                    TalentType.AS => image.sprite = manager.asIcon,
                    TalentType.RE => image.sprite = manager.reIcon,
                    _ => image.sprite = manager.defaultIcon // Falls kein passendes TalentType existiert
                };
            }

            //Setze Count, bzw. modifiziere Sprite in Abhängigkeit der Werte
            if(myNode.myTypes.Count > 1)
            {
                // Setze Größe basierend auf Original (nicht multiplizieren!)
                if (originalCircleSize != Vector2.zero)
                {
                    circle.sizeDelta = originalCircleSize * 2.2f; // Hybrid Talents mittelgroß
                    Debug.Log($"[Talent_UI] Hybrid Talent circle.sizeDelta gesetzt: {circle.sizeDelta}");
                }
                Outline hybrid = gameObject.GetComponent<Outline>();

                hybrid.enabled = true;
            }
            else
            {
                // Setze Größe basierend auf Original (nicht multiplizieren!)
                if (originalCircleSize != Vector2.zero)
                {
                    circle.sizeDelta = originalCircleSize * 1.6f; // Normale Talents am kleinsten
                    Debug.Log($"[Talent_UI] Normal Talent circle.sizeDelta gesetzt: {circle.sizeDelta}");
                }
            }
        }

        //Setze Werte
        currentCount = myNode.myCurrentCount;
        maxCount = myNode.myMaxCount;
        value = myNode.myValue;

        //Setze UI Paramenter
        gameObject.GetComponent<RectTransform>().anchoredPosition = myNode.myPosition;
        //gameObject.GetComponentInChildren<Text>().text = string.Join(", ", myTypes);

        SetDescription();

    }

    public void SetDescription()
    {
        if (myNode.isGemSocket)
        {
            // Gem-Socket Beschreibung
            description = "<b>Gem Socket</b>\n";
            
            if (myNode.equippedGem != null)
            {
                var gem = myNode.equippedGem;
                description += $"\n<color=yellow>Equipped: {gem.ItemName}</color>\n";
                description += $"GemType: {gem.gemType}\n";
                
                // Prüfe ob Gem eine Ability hat (über gemAbility-Referenz)
                if (gem.gemAbility != null)
                {
                    description += $"\n<color=cyan>Ability: {gem.gemAbility.abilityName}</color>\n";
                    description += $"{gem.gemAbility.description}\n";
                    
                    // Zeige Skillpoint-Bonus
                    int totalSkillpoints = TalentNode.GetTotalSkillpointsForGemType(gem.gemType);
                    description += $"\n<color=lime>Total Skillpoints ({gem.gemType}): {totalSkillpoints}</color>";
                }
                else
                {
                    description += "\n<color=gray>Support Gem (kein Ability)</color>";
                }
            }
            else
            {
                description += "\n<color=gray>Empty Socket</color>\n";
                description += "Drag a Gem here to equip it.\n";
                description += "All GemTypes can be equipped in any socket.";
            }
        }
        else if (myAbility == null)
        {
            description = "<b>ID: " + myNode.ID + "</b>\n Increases the players <b>" + string.Join(", ", myTypes) + "</b> by <b>" + value.ToString() + "%</b> per skillpoint invested. ";
        }
        else
        {
            Debug.Log("###Wird nicht gecalled.");
            description = "<b>Divine Root</b>\n\nUnlocks the talent tree and provides 1 passive health regeneration. This is the foundation of your character's survival capabilities.";

        }
           
    }



    private Image image;

    public Sprite icon;
    

    private Text textComponent;

    // Für Gem-Visualisierung
    private Image gemIconImage; // Image-Component für das Gem-Icon über dem Socket



    [HideInInspector]
    public bool unlocked;

    //[SerializeField]
    public List<Talent_UI> childTalent = new List<Talent_UI>();



    //Die Weiterleitung zu anderen Talenten macht er in Abhängig von "Pfeilen", also wenn ein PfeilSprite auf eine andere Fähigkeit zeigt, bzw. sie einen Pfeil besitzt, dann wird das Folgetalent freigeschaltet.
    //Kp ob ich das so machen wollen würde, entsprechendes Video hier:
    // https://youtu.be/NEqaBBnAFfM?t=406

    //Drag and Drop Magie
    //https://youtu.be/ILaDr3CE7QY?t=615


    public string GetName()
    {
            return talentName;
    }


 
    private void RunThroughChildTalents(Talent_UI talent)
    {
        //Und füge dieses der entsprechenden Spezialisierungsliste hinzu. (Spec1 / Spec2 / Spec3)
        //AddSpecializationTalents(talent);
        if (talent.childTalent.Count > 0)
        {
            foreach (Talent_UI childTalent in talent.childTalent)
            {
                RunThroughChildTalents(childTalent);
            }


        }
    }


    //Void to be called in TalentTree.cs for structural purpose.
    //Setzt Image und Count Text für jedes einzelne Talent.
    public void SetTalentUIVariables()
    {
        //image = GetComponentInChildren<Image>().sprite;
        icon = GetComponentInChildren<Image>().sprite;

        // Hole alle direkten Kind-GameObjects
        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                if(myNode != null)
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.text = $"ID: {myNode.ID}:{currentCount}/{maxCount}";

                else
                {
                    //Debug.Log("In: SetTalentUIVariables() konnte kein Node gefunden werden.");
                    textComponent.text = $"{currentCount}/{maxCount}";
                }

            }
        }
    }

    public bool Click()
    {
        textComponent = transform.GetComponentInChildren<Text>();
        if (currentCount < maxCount && unlocked)
        {
            currentCount++;

            if(myNode != null)
            textComponent.text = $"ID: {myNode.ID}:{currentCount}/{maxCount}";

            else
            {
                Debug.Log("In: Click() konnte kein Node gefunden werden.");
                textComponent.text = $"{currentCount}/{maxCount}";
            }
               // textComponent.text = $"{currentCount}/{maxCount}";

            // Nur wenn es sich nicht um den Urpsrung des TalentTrees handelt!
            if (myAbility == null)
            // 💥 Neuer Code: Bei erster Investition expandieren
            if (currentCount == 1)   
            {
                // Nur einmalig expandieren!
                //TalentTreeGenerator.instance.ExpandNode(myNode);
            }

            if (currentCount == maxCount)
            {
                for (int i = 0; i < childTalent.Count; i++)
                {
                    if (childTalent[i] != null)
                        childTalent[i].Unlock();
                }
            }

            return true;
        }
        return false;
    }

    public void ChangeImageToExpanded()
    {
        image = GetComponent<Image>();
        image.color = Color.yellow;
    }


    public bool Unlockable()
    {
        if (myNode != null)
        {
            if (unlocked) return true;


            foreach (TalentNode neighbor in myNode.myConnectedNodes)
                if (neighbor != null && neighbor.myCurrentCount >= neighbor.myMaxCount)
                    Unlock();

            return false;
        }
        return false;

    }



    public void Lock()
    {
        //Debug.Log("Lets lock " + gameObject.name);
        image = GetComponent<Image>();
        image.color = Color.grey;

        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.color = Color.grey;
            }
        }

        unlocked = false;
    }

    public void Unlock()
    {
        image = GetComponent<Image>();
        image.color = Color.white;

        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.color = Color.white;
            }
        }

        unlocked = true;
    }

    public void UpdateTalent()
    {
        image = GetComponent<Image>();

        //if(myNode != null)
        //textComponent.text = $"{myNode.myCurrentCount}/{myNode.myMaxCount}";

        if (unlocked)
        {
            Unlock();
        }



    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Rechtsklick auf Gem-Socket: Gem unequippen
        if (eventData.button == PointerEventData.InputButton.Right && myNode.isGemSocket && myNode.equippedGem != null)
        {
            ItemInstance gem = myNode.equippedGem;
            Debug.Log($"[Talent_UI] Rechtsklick: Versuche Gem '{gem.GetName()}' zu unequippen");

            // Unequip über TalentTreeManager (inkl. Cooldown-Check über ActionButton)
            ItemInstance unequippedGem = TalentTreeManager.instance.UnequipGemFromSocket(myNode);

            if (unequippedGem != null)
            {
                // Füge Gem zurück ins Inventar hinzu
                bool added = UI_Inventory.instance.inventory.AddItemToFirstFreeSlot(unequippedGem);

                if (added)
                {
                    // Visualisierung aktualisieren
                    UpdateGemVisual();

                    // Tooltip aktualisieren
                    SetDescription();

                    Debug.Log($"[Talent_UI] Gem '{unequippedGem.GetName()}' erfolgreich unequipped und zurück ins Inventar");
                }
                else
                {
                    // Kein freier Slot: Gem wieder equippen (Rollback)
                    Debug.LogWarning($"[Talent_UI] Inventar voll - Gem '{unequippedGem.GetName()}' wird wieder equipped");
                    myNode.EquipGem(unequippedGem);
                    UpdateGemVisual();
                    UI_Manager.instance.ShowTooltip("Inventory is full! Cannot unequip Gem.");
                }
            }
            else
            {
                Debug.LogWarning($"[Talent_UI] Gem '{gem.GetName()}' konnte nicht unequipped werden (Cooldown aktiv oder Fehler)");
                UI_Manager.instance.ShowTooltip("Cannot unequip Gem while ability is on cooldown!");
            }
        }
        // Linksklick: Normale Talent-Click-Logik
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            TalentTreeManager.instance.TryUseTalent(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(myAbility != null)
        {
            description = "<b>Divine Root</b>\nUnlocks the talent tree and provides 1 passive health regeneration. \nThis is the foundation of your character's survival capabilities.";
        }
        UI_Manager.instance.ShowTooltip(description);
        circle.gameObject.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
        circle.gameObject.SetActive(false);

    }

    public void OnDrop(PointerEventData eventData)
    {
        // Nur für Gem-Sockets relevant
        if (!myNode.isGemSocket)
            return;

        // Hole das gedraggede Item aus HandScript
        IMoveable moveable = HandScript.instance.MyMoveable;
        ItemInstance draggedItem = moveable as ItemInstance;

        if (draggedItem == null)
        {
            Debug.LogWarning("[Talent_UI] OnDrop: Kein ItemInstance gefunden");
            return;
        }

        // Prüfe, ob es ein Gem ist
        if (draggedItem.itemType != ItemType.Gem)
        {
            Debug.LogWarning($"[Talent_UI] OnDrop: Item '{draggedItem.GetName()}' ist kein Gem (Type: {draggedItem.itemType})");
            UI_Manager.instance.ShowTooltip("Only Gems can be equipped in Gem Sockets!");
            return;
        }

        // Prüfe, ob Socket bereits belegt ist
        if (myNode.equippedGem != null)
        {
            Debug.LogWarning($"[Talent_UI] OnDrop: Socket bereits belegt mit '{myNode.equippedGem.GetName()}'");
            UI_Manager.instance.ShowTooltip("This socket is already occupied. Right-click to unequip first.");
            return;
        }

        Debug.Log($"[Talent_UI] OnDrop: Versuche Gem '{draggedItem.GetName()}' (Type: {draggedItem.gemType}) zu equippen");

        // Rufe TalentTreeManager auf, um das Gem zu equippen (inkl. Cooldown-Check)
        bool success = TalentTreeManager.instance.EquipGemToSocket(draggedItem, myNode);

        if (success)
        {
            // Item aus Hand entfernen
            HandScript.instance.Put();

            // Visualisierung aktualisieren
            UpdateGemVisual();

            // Tooltip aktualisieren
            SetDescription();

            Debug.Log($"[Talent_UI] Gem '{draggedItem.GetName()}' erfolgreich in Socket equipped");
        }
        else
        {
            Debug.LogWarning($"[Talent_UI] Gem '{draggedItem.GetName()}' konnte nicht equipped werden (Cooldown aktiv?)");
        }
    }

    // Aktualisiert die visuelle Darstellung des Gems über dem Socket
    private void UpdateGemVisual()
    {
        if (!myNode.isGemSocket)
            return;

        // Finde oder erstelle das Gem-Icon-Image
        if (gemIconImage == null)
        {
            // Suche nach einem existierenden Child mit dem Namen "GemIcon"
            Transform gemIconTransform = transform.Find("GemIcon");

            if (gemIconTransform == null)
            {
                // Erstelle ein neues Child-GameObject für das Gem-Icon
                GameObject gemIconObj = new GameObject("GemIcon");
                gemIconObj.transform.SetParent(transform, false);

                // Füge Image-Component hinzu
                gemIconImage = gemIconObj.AddComponent<Image>();
                gemIconImage.raycastTarget = false; // Verhindert, dass das Icon Raycasts blockiert

                // Positioniere das Icon über dem Socket (zentriert, etwas verkleinert)
                RectTransform iconRect = gemIconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(40, 40); // Etwas kleiner als der Socket selbst
            }
            else
            {
                gemIconImage = gemIconTransform.GetComponent<Image>();
            }
        }

        // Setze das Icon-Sprite basierend auf dem equipped Gem
        if (myNode.equippedGem != null)
        {
            gemIconImage.sprite = myNode.equippedGem.icon;
            gemIconImage.color = Color.white;
            gemIconImage.enabled = true;
            Debug.Log($"[Talent_UI] Gem-Icon angezeigt: {myNode.equippedGem.GetName()}");
        }
        else
        {
            gemIconImage.sprite = null;
            gemIconImage.enabled = false;
            Debug.Log("[Talent_UI] Gem-Icon versteckt (Socket leer)");
        }
    }



    private IEnumerator RotateCircle()
    {
        while (true)
        {
            circle.Rotate(0, 0, 20f * Time.deltaTime); // 50° pro Sekunde
            yield return null;
        }
    }

}
