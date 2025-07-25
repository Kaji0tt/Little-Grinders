using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Siehe #Lootbox - eine gesonderte Klasse für die Lootbox im Tutorial.
public class TutorialLootBox : MonoBehaviour
{
    private const string Message = "Ein Item in 'firstItems' ist null. Bitte überprüfen Sie die Zuweisung.";
    SpriteRenderer sprite;
    public Sprite newSprite;
    private bool lootBoxOpened = false;


    [SerializeField]
    private GameObject tutorialUI1;

    [SerializeField]
    private GameObject tutorialUI2;

    [SerializeField]
    private GameObject tutorialUI3;

    public Item[] firstItems;

    public Int_SlotBtn weaponSlotBtn;

    public EnemyController armoredEnemy;

    // ALT: public List<ItemMod> mods = new List<ItemMod>();
    // NEU: Eine Liste von Mod-Definitionen, die du im Inspector zuweisen kannst.
    [Header("Mods to apply for testing")]
    public List<ItemModDefinition> modsToApply = new List<ItemModDefinition>();

    [SerializeField]
    Tutorial tutorialBox;

    void OnEnable()
    {
        PlayerStats.eventLevelUp += ShowNextUI;
        PlayerStats.eventLevelUp += CheckLevelUpTutorial;
        // Event für Waffen-Ausrüstung hinzufügen
        StartCoroutine(CheckWeaponEquipped());
        // Event für Talentpunkt-Verwendung hinzufügen
        StartCoroutine(CheckTalentPointSpent());
        // Event für gepanzerten Gegner hinzufügen
        StartCoroutine(CheckArmoredEnemyDefeated());
    }

    private void Start()
    {
        StartCoroutine(ShowIntroLogs());
    }

    private IEnumerator ShowIntroLogs()
    {
        yield return new WaitUntil(() => LogScript.instance != null); // Warten bis LogScript bereit ist

        LogScript.instance.ShowLog("Hey Adventurer!");
        yield return new WaitForSeconds(2f);

        LogScript.instance.ShowLog("Welcome to the world of Little Grinders!");
        yield return new WaitForSeconds(3f);

        LogScript.instance.ShowLog("Use WASD to move around and open the Chest to prepare for Battle!");
        yield return new WaitForSeconds(5f);
    }

    void OnDisable()
    {
        PlayerStats.eventLevelUp -= ShowNextUI;
        PlayerStats.eventLevelUp -= CheckLevelUpTutorial;
        // Coroutines werden automatisch gestoppt
    }


    private void OnTriggerStay(Collider collider)
    {
        if (Input.GetKeyDown(KeyCode.F) && collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && lootBoxOpened == false)
        {
            lootBoxOpened = true;

            Vector3 spawnPos = new Vector3(PlayerManager.instance.player.transform.position.x + .5f, gameObject.transform.position.y, PlayerManager.instance.player.transform.position.z + .9f);

            foreach (Item item in firstItems)
            {
                if (item == null)
                {
                    Debug.LogWarning(message: Message);
                    continue; // Überspringe null Items
                }

                ItemInstance firstItem = new ItemInstance(item);
                
                // --- NEUE LOGIK ZUM ANWENDEN VON MODS ---
                foreach (var modDef in modsToApply)
                {
                    // Prüfe, ob der Item-Typ des Items in den erlaubten Typen des Mods enthalten ist.
                    if (modDef != null && (modDef.allowedItemTypes & firstItem.itemType) != 0)
                    {
                        // Erstelle eine neue Mod-Instanz aus der Definition
                        ItemMod newMod = new ItemMod { definition = modDef };
                        
                        // Für Testzwecke weisen wir eine feste Seltenheit zu.
                        newMod.rolledRarity = Rarity.Rare; 
                        
                        // Initialisiere den Mod, um den 'rolledValue' zu berechnen
                        int mapLevel = GlobalMap.instance != null ? GlobalMap.instance.currentMap.mapLevel : 1;
                        newMod.Initialize(mapLevel);

                        // Füge den gültigen Mod zum Item hinzu
                        firstItem.addedItemMods.Add(newMod);
                    }
                }

                // Wende die hinzugefügten Mods an (aktualisiert Stats, Namen, etc.)
                firstItem.ApplyItemMods();

                ItemWorld.SpawnItemWorld(spawnPos, firstItem);
            }


            LootBoxOpenedSprite();
        }
    }

    private void LootBoxOpenedSprite()
    {
        if (lootBoxOpened)
        {
            sprite = GetComponentInParent<SpriteRenderer>();
            sprite.sprite = newSprite;

            tutorialUI1.gameObject.SetActive(false);

            StartCoroutine(PostLootLog()); // NEU

            // ...
        }
    }

    private IEnumerator PostLootLog()
    {
        LogScript.instance.ShowLog("Great! Get the sword!");

        yield return new WaitForSeconds(3.5f); // oder automatisch
        
        LogScript.instance.ShowLog("Open the inventory on \"C\" and do \"Mouse1\" on the sword to equip it.");

    }

    private void ShowNextUI()
    {
        if(PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() == 2)
        {
            tutorialUI2.gameObject.SetActive(false);

            tutorialUI3.gameObject.SetActive(true);

            //tutorialUI3.GetComponent<Animator>().SetInteger("Arrow", 2);
        }

    }

    private IEnumerator CheckWeaponEquipped()
    {
        // Warten bis weaponSlotBtn verfügbar ist
        yield return new WaitUntil(() => weaponSlotBtn != null);
        
        // Überwachen ob eine Waffe ausgerüstet wurde
        while (weaponSlotBtn.GetEquippedItem() == null)
        {
            yield return new WaitForSeconds(0.1f); // Alle 0.1 Sekunden prüfen
        }
        
        // Waffe wurde ausgerüstet - Log anzeigen
        StartCoroutine(PostEquipLog());
    }

    private IEnumerator PostEquipLog()
    {
        LogScript.instance.ShowLog("You seem to be prepared. Give attention to the \"Voidwither\" effect on your weapon.");
        
        yield return new WaitForSeconds(3f);
        
        LogScript.instance.ShowLog("Activate it by pressing \"Mouse2\" before entering the fight to gather some experience.");
    }

    private void CheckLevelUpTutorial()
    {
        if (PlayerManager.instance.playerStats.Get_level() == 2)
        {
            StartCoroutine(ShowTalentTreeTutorial());
        }
    }

    private IEnumerator ShowTalentTreeTutorial()
    {
        yield return new WaitForSeconds(1f); // Kurze Verzögerung nach Level-Up
        
        LogScript.instance.ShowLog("Congratulations! You reached Level 2!");
        
        yield return new WaitForSeconds(3f);
        
        LogScript.instance.ShowLog("Press \"X\" to open your Talent Tree and spend your Skill Point!");
        
        yield return new WaitForSeconds(6f);
        
        LogScript.instance.ShowLog("Choose wisely - each point shapes your adventure!");
        
    }

    private IEnumerator CheckTalentPointSpent()
    {
        // Warten bis TalentTreeManager verfügbar ist
        yield return new WaitUntil(() => TalentTreeManager.instance != null && TalentTreeManager.instance.defaultTalent != null);
        
        // Überwachen ob das defaultTalent geskillt wurde
        while (TalentTreeManager.instance.defaultTalent.currentCount == 0)
        {
            yield return new WaitForSeconds(0.3f); // Alle 0.3 Sekunden prüfen
        }
        
        // DefaultTalent wurde geskillt - Tutorial anzeigen
        StartCoroutine(ShowTalentSpentTutorial());
    }

    private IEnumerator ShowTalentSpentTutorial()
    {
        yield return new WaitForSeconds(1f); // Kurze Verzögerung
        
        LogScript.instance.ShowLog("Excellent! You unlocked regeneration. From now on, you're growing stronger!");
        
        yield return new WaitForSeconds(3f);
        
        LogScript.instance.ShowLog("There's an armored enemy waiting for you west of the camp.");
        
        yield return new WaitForSeconds(4f);
        
        LogScript.instance.ShowLog("Remember: Ability Power damage ignores armor completely!");
        
    }

    private IEnumerator CheckArmoredEnemyDefeated()
    {
        // Warten bis armoredEnemy verfügbar ist
        yield return new WaitUntil(() => armoredEnemy != null);
        
        // Überwachen ob der gepanzerte Gegner besiegt wurde
        while (!armoredEnemy.isDead)
        {
            yield return new WaitForSeconds(0.5f); // Alle 0.5 Sekunden prüfen
        }
        
        // Gepanzerter Gegner wurde besiegt - Finales Tutorial anzeigen
        StartCoroutine(ShowFinalTutorial());
    }

    private IEnumerator ShowFinalTutorial()
    {
        yield return new WaitForSeconds(0.5f); // Kurze Verzögerung nach dem Kampf

        LogScript.instance.ShowLog("Excellent! You are now well prepared for your adventures!");

        yield return new WaitForSeconds(3f);

        LogScript.instance.ShowLog("Enter the open world to the west. Use \"M\" to navigate during your explorations. Good luck, adventurer!");
        
        yield return new WaitForSeconds(5f);
        
        LogScript.instance.ShowLog("Good luck, little grinder!");
    }
}
