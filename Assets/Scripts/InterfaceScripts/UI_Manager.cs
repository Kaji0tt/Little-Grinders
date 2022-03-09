using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    #region Singleton
    public static UI_Manager instance;
    private void Awake()
    {
        UI_Manager[] sceneInstances = FindObjectsOfType<UI_Manager>();
        if(sceneInstances.Length >= 2)
        {
            Destroy(sceneInstances[0]);
        }
        instance = this;

    }

    #endregion
    [SerializeField]
    private GameObject tooltip;

    private Text tooltipText;

    [SerializeField]
    private Button[] actionButtons;


    private IsometricPlayer isometricPlayer;

    private KeyCode mainMenuKey;


    public KeyCode toggleCamKey, pickKey;

    private GameObject[] keyBindButtons;


    [SerializeField]
    private CanvasGroup inventoryTab;

    [SerializeField]
    private CanvasGroup skillTab;

    [SerializeField]
    private CanvasGroup mainMenu;  //hier sollte später das komplette Hauptmenü drin liegen.

    [SerializeField]
    private CanvasGroup actionBar;

    [SerializeField]
    private CanvasGroup mapTab;


    private List<KeyCode> keyCodes = new List<KeyCode>();

    private string bindName;

    public static bool GameIsPaused = false;



    private void OnEnable()
    {
        GameIsPaused = false;
        keyBindButtons = GameObject.FindGameObjectsWithTag("KeyBindings");
    }


    private void Start()
    {
        if(tooltip != null)
        tooltipText = tooltip.GetComponentInChildren<Text>();

        mainMenuKey = KeyCode.Escape;

        pickKey = KeyManager.MyInstance.Keybinds["PICK"];

        toggleCamKey = KeyCode.Tab;


        isometricPlayer = GetComponent<IsometricPlayer>();

        RefreshKeyBindText();

    }

    private void RefreshKeyBindText()
    {
        foreach(KeyValuePair<string, KeyCode> kvp in KeyManager.MyInstance.Keybinds)
        {
            UpdateKeyText(kvp.Key, kvp.Value);
        }
    }

    private void Update()
    {
            //Interface Abfrage des Cursos sollte implementiert werden.

            //Action-Bars

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

        if (Input.GetKeyDown(mainMenuKey))
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
                AudioManager.instance.Play("OpenSkills");
            else
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
                AudioManager.instance.Play("OpenMap");
            else
                AudioManager.instance.Play("CloseMenu");
        }

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
        actionButtons[btnIndex].onClick.Invoke();
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

            
            tooltipText.text = string.Format("<b><color={0}> {1} </color></b>\n{2}\n{3}", color, item.ItemName, item.ItemDescription, item.ItemValueInfo);
        }
    }

    public void ShowTooltip(Vector3 position, string description, GameObject obj)
    {
        //RectTransform rect = tooltip.GetComponent<RectTransform>();


        if (description != null)
        {
            tooltipText.text = description;
        }

        Vector2 newPos = new Vector2(obj.transform.position.x, obj.transform.position.y);


        /*
        if (tooltip.GetComponent<Tooltip>().IsFullyOnScreen())
        {
            rect.pivot = new Vector2(0, 1);
        }
        else
            rect.pivot = new Vector2(1, 0);
        */

        tooltip.transform.position = newPos;

        tooltip.SetActive(true);

    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }


    public void OpenCloseMenu(CanvasGroup canvas)
    {
        canvas.alpha = canvas.alpha > 0 ? 0 : 1; 
        canvas.blocksRaycasts = canvas.blocksRaycasts == true ? false : true;

    }

    //Es sollte noch geschaut werden, inwiefern das UI nach Szenen-Wechsel gespeichert werden kann.
    public void UpdateKeyText(string key, KeyCode code)
    {
        TextMeshProUGUI tmp = Array.Find(keyBindButtons, x => x.name == key).GetComponentInChildren<TextMeshProUGUI>();

        if (code.ToString().Contains("Alpha"))
        {
            tmp.text = code.ToString().Substring(5);
        }
        else
            tmp.text = code.ToString();

    }


}
