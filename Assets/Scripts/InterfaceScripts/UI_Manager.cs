using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class UI_Manager : MonoBehaviour
{
    #region Singleton
    private static UI_Manager myInstance;

    public static UI_Manager instance
    {
        get
        {
            if (myInstance == null)
            {
                Debug.LogError("UI_Manager instance is null. Make sure it's initialized correctly.");
            }
            return myInstance;
        }
    }

    private void Awake()
    {
        if (myInstance == null)
        {
            myInstance = this;
            DontDestroyOnLoad(gameObject); // Hält UI_Manager über Szenenwechsel hinweg
        }
        else if (myInstance != this)
        {
            Destroy(gameObject); // Zerstört doppelte Instanzen
        }
    }

    #endregion

    public GameObject tooltip;

    //private Text tooltipText;

    //[SerializeField]
    //private Button[] actionButtons;


    //private IsometricPlayer isometricPlayer;

    private KeyCode mainMenuKey;


    public KeyCode toggleCamKey, pickKey;

    private GameObject[] keyBindButtons;

    //Die Zuweisung über den Inspector ist scheiße, da bei SzeneWechsel die Einstellungen / Zuweisung verloren geht.
    //Hilfreich wäre ein "OnSceneChange" Event oder so.
    //[SerializeField]
    private CanvasGroup inventoryTab;

    //[SerializeField]
    private CanvasGroup skillTab;

    //[SerializeField]
    private CanvasGroup mainMenu;  //hier sollte später das komplette Hauptmenü drin liegen.

    //[SerializeField]
    private CanvasGroup actionBar;

    //[SerializeField]
    private CanvasGroup mapTab;


    //BadManner WorkAround für MainMenü
    //[SerializeField]
    //private GameObject canvasIG;


    public static bool GameIsPaused = false;


    private void OnChangedScene(Scene current, Scene next)
    {
        string currentName = current.name;

        if (currentName == null)
        {
            // Scene1 has been removed
            currentName = "Replaced";
        }

        Debug.Log("Scenes: " + currentName + ", " + next.name);
        //Zuweisungen der Komponenten ohne Inspektor (Insbesondere Wichtig für Szenewechsel oder die Szeneübergreifende Anwendung von KeyBinds.

        if (next.name != "MainMenu")
        {
            AllStartMethods();
        }
        else
        {
            //DeactivateIngameInterface();
            AllStartMethods();
        }

    }

    private void OnDisable()
    {

        SceneManager.activeSceneChanged -= OnChangedScene;

    }

    private void OnEnable()
    {
        GameIsPaused = false;

        keyBindButtons = GameObject.FindGameObjectsWithTag("KeyBindings");

        foreach (GameObject keyBindBtn in keyBindButtons)
        {
            keyBindBtn.AddComponent<UI_Btn_Listener>();
        }

        SceneManager.activeSceneChanged += OnChangedScene;

    }

    private void Start()
    {


        CheckForUI_Elements();


        mainMenuKey = KeyCode.Escape;

        if (KeyManager.MyInstance != null)
            pickKey = KeyManager.MyInstance.Keybinds["PICK"];

        toggleCamKey = KeyCode.Tab;



    }

    private void AllStartMethods()
    {
        //Die KeyBindButtons sind die Anzeigen der Zugewiesenen KeyBindings im Menü. Bsp.: Key
        keyBindButtons = GameObject.FindGameObjectsWithTag("KeyBindings");

        //Debug.Log("Hey i got called! We rached starter Method!");

        CheckForUI_Elements();

        //RefreshKeyBindText();

        //Debug.Log("We got beanth the main check methods!");
        mainMenuKey = KeyCode.Escape;

        /*
        if (KeyManager.MyInstance != null)
            pickKey = KeyManager.MyInstance.Keybinds["PICK"];
        */
        toggleCamKey = KeyCode.Tab;
    }
    public void UpdateAllKeyTexts()
    {
        foreach (var button in FindObjectsOfType<KeyBindButton>())
        {
            button.UpdateKeyText();
        }
    }


    private bool IsInMainMenu()
    {
        // Beispiel: Hauptmenü hat den Build-Index 0
        return SceneManager.GetActiveScene().buildIndex == 0;
    }
    private void Update()
    {

        if (!IsInMainMenu())
        {
            CheckForUserAction();
        }
    }

    [SerializeField]
    private Item[] itemsOnStart;

    private void CheckForUserAction()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            PlayerManager.instance.player.GetComponent<PlayerStats>().Gain_xp(600);
            foreach (Item item in itemsOnStart)
                UI_Inventory.instance.inventory.AddItemToFirstFreeSlot(new ItemInstance(item));

        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["WEAPON"]) && !GameIsPaused)
        {
            ActionButtonOnClick(0);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["KOPF"]) && !GameIsPaused)
        {
            ActionButtonOnClick(1);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["BRUST"]) && !GameIsPaused)
        {
            ActionButtonOnClick(2);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["BEINE"]) && !GameIsPaused)
        {
            ActionButtonOnClick(3);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["SCHUHE"]) && !GameIsPaused)
        {
            ActionButtonOnClick(4);
        }

        if (Input.GetKeyDown(mainMenuKey)) //&&MainMenuClosed
        {

            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        //Interface

        if (Input.GetKeyDown(KeyManager.MyInstance.Keybinds["SKILLS"]) && !GameIsPaused)
        {
            OpenCloseMenu(skillTab);
            if (skillTab.alpha == 0)
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySound("Ingame_SubMenuOpen");
                else
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySound("Ingame_SubMenuClose");

        }

        if (Input.GetKeyDown(KeyManager.MyInstance.Keybinds["STATS"]) && !GameIsPaused)
        {
            OpenCloseMenu(inventoryTab);
            AudioManager.instance.PlaySound("Ingame_SubMenuOpen");
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.Keybinds["MAP"]) && !GameIsPaused)
        {
            OpenCloseMenu(mapTab);
            if (mapTab.alpha == 0)
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySound("Ingame_SubMenuOpen");
                else
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySound("Ingame_SubMenuClose");
        }

        //Make it possible to close all Menues by ESCAPE
        /*
        if(Input.GetKeyDown(KeyCode.Escape) && !GameIsPaused)
        {
            foreach(CanvasGroup canvas in canvasGroup)
            {
                if (canvas.alpha == 1)
                    OpenCloseMenu(canvas);
            }
        }
        */

    }

    private void Pause()
    {
        OpenCloseMenu(mainMenu);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void Resume()
    {
        OpenCloseMenu(mainMenu);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void OpenCloseMainMenu()
    {
        mainMenu.alpha = mainMenu.alpha > 0 ? 0 : 1;
        mainMenu.blocksRaycasts = mainMenu.blocksRaycasts == true ? false : true;

        actionBar.alpha = actionBar.alpha > 0 ? 0 : 1;
        actionBar.blocksRaycasts = actionBar.blocksRaycasts == true ? false : true;


    }

    /// <summary>
    /// Action Button Segment
    /// </summary>

    private Button aBtn1, aBtn2, aBtn3, aBtn4, aBtn5;

    public List<ActionButton> actionButtons { get; private set; } = new List<ActionButton>();

    /// <summary>
    /// Weist eine Fähigkeit aus einem Mod dem korrekten ActionButton zu.
    /// </summary>
    private void AssignAbilityFromMod(ItemInstance item, ItemType itemType)
    {
        if (item == null) return;

        ActionButton targetButton = actionButtons.FirstOrDefault(btn => btn.myItemType == itemType);

        if (targetButton != null)
        {
            //Debug.Log($"'{targetButton.name}' wird mit ItemInstance '{item.ItemName}' belegt.");
            targetButton.SetItemInstance(item);
        }
        else
        {
            Debug.LogWarning($"Kein ActionButton für den Item-Typ '{itemType}' im UI_Manager gefunden.");
        }
    }

    /// <summary>
    /// Aktualisiert den entsprechenden ActionButton für ein ausgerüstetes Item.
    /// Weist die Fähigkeit zu, falls eine vorhanden ist, oder leert den Button, falls nicht.
    /// Diese Methode wird von GameEvents aufgerufen, wenn sich die Ausrüstung ändert.
    /// </summary>
    public void UpdateAbilityForEquippedItem(ItemInstance equippedItem, ItemType itemType = ItemType.None)
    {
        if (equippedItem == null)
        {
            // Nur den spezifischen Button leeren, wenn itemType bekannt ist
            if (itemType != ItemType.None)
            {
                var buttonToClear = actionButtons.FirstOrDefault(btn => btn.myItemType == itemType);
                if (buttonToClear != null)
                    buttonToClear.SetItemInstance(null);
            }
            return;
        }

        var targetButton = actionButtons.FirstOrDefault(btn => btn.myItemType == equippedItem.itemType);
        if (targetButton != null)
        {
            if (equippedItem.addedItemMods == null || !equippedItem.addedItemMods.Any(m => m.definition != null && m.definition.modAbilityData != null))
            {
                targetButton.SetItemInstance(null);
            }
            else
            {
                targetButton.SetItemInstance(equippedItem);
            }
        }

        if (equippedItem.addedItemMods == null || !equippedItem.addedItemMods.Any())
            return;

        var abilityMods = equippedItem.addedItemMods.Where(mod => mod.definition != null && mod.definition.modAbilityData != null).ToList();
        if (!abilityMods.Any())
            return;

        AssignAbilityFromMod(equippedItem, equippedItem.itemType);
    }

    private void ActionButtonOnClick(int btnIndex)
    {
        switch (btnIndex)
        {
            case 0:

                aBtn1.onClick.Invoke();

                break;

            case 1:

                aBtn2.onClick.Invoke();

                break;

            case 2:

                aBtn3.onClick.Invoke();

                break;

            case 3:

                aBtn4.onClick.Invoke();

                break;

            case 4:

                aBtn5.onClick.Invoke();

                break;

            default:
                break;

        }

        //Debug.Log(actionButtons[btnIndex].gameObject.name);
        //actionButtons[btnIndex].onClick.Invoke();
    }

    public void ShowItemTooltip(Vector3 position, ItemInstance item)
    {
        tooltip.SetActive(true);
        tooltip.transform.position = position;

        if (item != null)
        {
            string colorHex = GetRarityColor(item.itemRarity);

            string itemDescription = string.Format(
                "<b><color={0}> {1} </color></b>\n{2}\n{3}",
                colorHex,
                item.ItemName,
                item.ItemDescription,
                item.ItemValueInfo
            );

            Tooltip.instance.SetText(itemDescription);
        }
    }

    private string GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Legendary: return "#db8535";
            case Rarity.Epic: return "#783391";
            case Rarity.Rare: return "#282b8f";
            case Rarity.Uncommon: return "#30bf4f";
            case Rarity.Common: return "#c9c9c9";
            default: return "#8c6d6d"; // Optional: für unbekannt oder "Schrott"
        }
    }

    public void ShowTooltip(string description)
    {
        tooltip.SetActive(true);


        Tooltip.instance.SetText(description);
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }


    public void OpenCloseMenu(CanvasGroup canvas)
    {
        if (canvas == null)
            CheckForUI_Elements();
        canvas.alpha = canvas.alpha > 0 ? 0 : 1; 
        canvas.blocksRaycasts = canvas.blocksRaycasts == true ? false : true;
    }




    //Überprüfe, ob die UI Elemente wirklich nicht Null sind.
    void CheckForUI_Elements()
    {
        actionButtons.Clear();

        InterfaceElement[] interfaceElements = FindObjectsOfType<InterfaceElement>();

        foreach(InterfaceElement interfaceElement in interfaceElements)
        {
            //Debug.Log(SceneManager.GetActiveScene().name + interfaceElement.gameObject.name);

            interfaceElement.InitialisizeUIElement(interfaceElement);

            switch (interfaceElement.interfaceElementEnum)
            {
                case InterfaceElementDeclaration.Inventar:

                    //Debug.Log("also eigentlich sollte das Inventar fesstgelegt worden sein. Hier meine CanvasGroup: ");
                    inventoryTab = interfaceElement.GetComponent<CanvasGroup>();

                    break;

                case InterfaceElementDeclaration.Tooltip:

                    //Debug.Log(interfaceElement.gameObject.name); <- In Procedural Map war Tooltip aus, deshalb zunächst nicht gefunden.

                    tooltip = interfaceElement.gameObject;

                    HideTooltip();

                    break;

                case InterfaceElementDeclaration.Map:

                    mapTab = interfaceElement.GetComponent<CanvasGroup>();

                    break;

                case InterfaceElementDeclaration.MainMenu:

                    mainMenu = interfaceElement.GetComponent<CanvasGroup>();

                    break;

                case InterfaceElementDeclaration.Skilltab:

                    skillTab = interfaceElement.GetComponent<CanvasGroup>();

                    break;

                    /*
                case InterfaceElementDeclaration.Tooltip:

                    interfaceElement.InitialisizeUIElement(interfaceElement);
                    tooltip = interfaceElement.interfaceGameObject;

                    break;
                    */
                case InterfaceElementDeclaration.Weapon:
                    aBtn1 = interfaceElement.GetComponent<Button>();
                    ActionButton _aBtn1 = aBtn1.GetComponent<ActionButton>();
                    actionButtons.Add(_aBtn1);
                    break;
                case InterfaceElementDeclaration.Kopf:
                    aBtn2 = interfaceElement.GetComponent<Button>();
                    ActionButton _aBtn2 = aBtn2.GetComponent<ActionButton>();
                    actionButtons.Add(_aBtn2);
                    break;
                case InterfaceElementDeclaration.Brust:
                    aBtn3 = interfaceElement.GetComponent<Button>();
                    ActionButton _aBtn3 = aBtn3.GetComponent<ActionButton>();
                    actionButtons.Add(_aBtn3);
                    break;
                case InterfaceElementDeclaration.Beine:
                    aBtn4 = interfaceElement.GetComponent<Button>();
                    ActionButton _aBtn4 = aBtn4.GetComponent<ActionButton>();
                    actionButtons.Add(_aBtn4);
                    break;
                case InterfaceElementDeclaration.Schuhe:
                    aBtn5 = interfaceElement.GetComponent<Button>();
                    ActionButton _aBtn5 = aBtn5.GetComponent<ActionButton>();
                    actionButtons.Add(_aBtn5);
                    break;

                default:
                    break;

            }

            foreach (var btn in actionButtons)
            {
                //Debug.Log($"Button: {btn.name} | ItemType: {btn.myItemType}");
            }

        }

    }

}
