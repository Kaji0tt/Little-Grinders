﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

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
    private Button aBtn1, aBtn2, aBtn3, aBtn4, aBtn5;


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

    private List<CanvasGroup> canvasGroup = new List<CanvasGroup>();

    bool allCanvasGroupsClosed;


    private List<KeyCode> keyCodes = new List<KeyCode>();

    private string bindName;

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

        foreach(GameObject keyBindBtn in keyBindButtons)
        {
            keyBindBtn.AddComponent<UI_Btn_Listener>();
        }

        SceneManager.activeSceneChanged += OnChangedScene;

    }


    /*
     * Richtig Chaos hier.
     * Alle Elemente sollten während des HauptMenüs bereits da sein.
     * CanvasGroup Ingame sollte deaktiviert werden, sobald alle Überschreibungen für KeyManager da sind. 
     * Also: CurrentScene = HauptMenü -> CanvasIngame.active(false)
     *       CurrentScene != HauptmMenü -> CanvasIngame.active(true)
     *       
     * So kann ein Schreiben später relevanter Daten bereits im Startbildschirm geschehen.
     * On
     * 
     * 
     * 
     * 
     */


    /*
    private void DeactivateIngameInterface()
    {

        canvasIG.SetActive(false);
    }
    */

    private void Start()
    {
        

        CheckForUI_Elements();


        //BindKeysInManager();

        RefreshKeyBindText();


        mainMenuKey = KeyCode.Escape;

        if (KeyManager.MyInstance != null)
            pickKey = KeyManager.MyInstance.Keybinds["PICK"];

        toggleCamKey = KeyCode.Tab;


        /*
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            canvasIG.SetActive(false);
        }
        */
        //isometricPlayer = GetComponent<IsometricPlayer>();




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

    public void BindKeysInManager()
    {

        KeyManager.MyInstance.BindKey("UP", KeyCode.W);
        KeyManager.MyInstance.BindKey("LEFT", KeyCode.A);
        KeyManager.MyInstance.BindKey("DOWN", KeyCode.S);
        KeyManager.MyInstance.BindKey("RIGHT", KeyCode.D);

        KeyManager.MyInstance.BindKey("STATS", KeyCode.E);
        KeyManager.MyInstance.BindKey("SKILLS", KeyCode.P);
        KeyManager.MyInstance.BindKey("PICK", KeyCode.Q);
        KeyManager.MyInstance.BindKey("MAP", KeyCode.M);

        KeyManager.MyInstance.BindKey("SLOT1", KeyCode.Alpha1);
        KeyManager.MyInstance.BindKey("SLOT2", KeyCode.Alpha2);
        KeyManager.MyInstance.BindKey("SLOT3", KeyCode.Alpha3);
        KeyManager.MyInstance.BindKey("SLOT4", KeyCode.Alpha4);
        KeyManager.MyInstance.BindKey("SLOT5", KeyCode.Alpha5);
    }

    private void RefreshKeyBindText()
    {
        //Debug.Log("Inside: RefreshKeyBindText()  Method!");
        foreach (KeyValuePair<string, KeyCode> kvp in KeyManager.MyInstance.Keybinds)
        {
            //Debug.Log("Inside: KeyManager.MyInstance.KeyBinds collection!! Key:" + kvp.Key.ToString());
            UpdateKeyText(kvp.Key, kvp.Value);
        }
    }

    //Es sollte noch geschaut werden, inwiefern das UI nach Szenen-Wechsel gespeichert werden kann.
    public void UpdateKeyText(string key, KeyCode code)
    {
        //Debug.Log("Inside: UpdateKeyText for Key:" + key + " with Code: " + code.ToString());
        TextMeshProUGUI tmp = Array.Find(keyBindButtons, x => x.name == key).GetComponentInChildren<TextMeshProUGUI>();


        //Debug.Log("you got updatekeytext running.");
        if (code.ToString().Contains("Alpha"))
        {
            tmp.text = code.ToString().Substring(5);
        }
        else
            tmp.text = code.ToString();

    }


    private bool IsInMainMenu()
    {
        // Beispiel: Hauptmenü hat den Build-Index 0
        return SceneManager.GetActiveScene().buildIndex == 0;
    }
    private void Update()
    {

        if(!IsInMainMenu())
        {
            CheckForUserAction();
        }
    }

    [SerializeField]
    private Item[] itemsOnStart;

    private void CheckForUserAction()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            PlayerManager.instance.player.GetComponent<PlayerStats>().Gain_xp(600);
            foreach (Item item in itemsOnStart)
            PlayerManager.instance.player.Inventory.AddItem(new ItemInstance(item));

        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["SLOT1"]) && !GameIsPaused)
        {
            ActionButtonOnClick(0);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["SLOT2"]) && !GameIsPaused)
        {
            ActionButtonOnClick(1);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["SLOT3"]) && !GameIsPaused)
        {
            ActionButtonOnClick(2);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["SLOT4"]) && !GameIsPaused)
        {
            ActionButtonOnClick(3);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.ActionBinds["SLOT5"]) && !GameIsPaused)
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
                    AudioManager.instance.Play("OpenSkills");
                else
                if (AudioManager.instance != null)
                    AudioManager.instance.Play("CloseMenu");

        }

        if (Input.GetKeyDown(KeyManager.MyInstance.Keybinds["STATS"]) && !GameIsPaused)
        {
            OpenCloseMenu(inventoryTab);
        }

        if (Input.GetKeyDown(KeyManager.MyInstance.Keybinds["MAP"]) && !GameIsPaused)
        {
            OpenCloseMenu(mapTab);
            if (mapTab.alpha == 0)
                if (AudioManager.instance != null)
                    AudioManager.instance.Play("OpenMap");
                else
                if (AudioManager.instance != null)
                    AudioManager.instance.Play("CloseMenu");
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
            string color = string.Empty;
            //In dieser Line sollten in Dependency of item.Rarity <color> Hexes definiert werden. (Geschlossen um item.ItemName)
            if (item.itemRarity == "Legendär")
                color = "#db8535";

            if (item.itemRarity == "Episch")
                color = "#783391";

            if (item.itemRarity == "Selten")
                color = "#282b8f";

            if (item.itemRarity == "Ungewöhnlich")
                color = "#30bf4f";

            if (item.itemRarity == "Gewöhnlich")
                color = "#c9c9c9";

            if (item.itemRarity == "Unbrauchbar")
                color = "#8c6d6d";

            string itemDescription = string.Format("<b><color={0}> {1} </color></b>\n{2}\n{3}", color, item.ItemName, item.ItemDescription, item.ItemValueInfo);
            
            Tooltip.instance.SetText(itemDescription);
            //tooltip.SetText() = string.Format("<b><color={0}> {1} </color></b>\n{2}\n{3}", color, item.ItemName, item.ItemDescription, item.ItemValueInfo);
        }
    }

    public void ShowTooltip(string description)
    {
        /*
        if (description != null)
        {
            tooltipText.text = description;
        }
        */
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
                case InterfaceElementDeclaration.AB1:

                    aBtn1 = interfaceElement.GetComponent<Button>();

                    break;

                case InterfaceElementDeclaration.AB2:

                    aBtn2 = interfaceElement.GetComponent<Button>();

                    break;

                case InterfaceElementDeclaration.AB3:

                    aBtn3 = interfaceElement.GetComponent<Button>();

                    break;

                case InterfaceElementDeclaration.AB4:

                    aBtn4 = interfaceElement.GetComponent<Button>();

                    break;

                case InterfaceElementDeclaration.AB5:

                    aBtn5 = interfaceElement.GetComponent<Button>();

                    break;

                default:
                    break;

            }
        }


    }

}
