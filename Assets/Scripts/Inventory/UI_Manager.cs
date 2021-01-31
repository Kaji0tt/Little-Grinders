using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class UI_Manager : MonoBehaviour
{
    #region Singleton
    public static UI_Manager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    [SerializeField]
    private GameObject tooltip;

    private Text tooltipText;

    [SerializeField]
    private Button[] actionButtons;

    private Spell[] spell;


    private IsometricPlayer isometricPlayer;

    private KeyCode action1, action2, action3, action4, action5, inventoryKey, skillKey, mainMenuKey;


    [SerializeField]
    private CanvasGroup characterMenu; // character Menue, weitergegeben wird ebenfalls welches fenster auf ist.

    [SerializeField]
    private CanvasGroup inventoryTab;

    [SerializeField]
    private CanvasGroup skillTab;

    [SerializeField]
    private CanvasGroup mainMenu;  //hier sollte später das komplette Hauptmenü drin liegen.

    [SerializeField]
    private CanvasGroup actionBar;


    //behinderte Methode die unter Menüs des Charakter Menüs zu deklarieren.
    //public GameObject talentTree, inventoryMenu; //Muss auch über Canvas group geregelt werden, da zu beginn nicht aktivierte Spielobjekte einen Null-Error ergeben!

    //private bool mouseInterface;

    private bool skillTabOpen, inventoryTabOpen;

    public static bool GameIsPaused = false;

    private void OnEnable()
    {
        GameIsPaused = false;
    }

    private void Start()
    {
        tooltipText = tooltip.GetComponentInChildren<Text>();

        //SetUseable(actionButtons[0], )
        action1 = KeyCode.Alpha1;

        action2 = KeyCode.Alpha2;

        action3 = KeyCode.Alpha3;

        action4 = KeyCode.Alpha4;

        action5 = KeyCode.Alpha5;

        inventoryKey = KeyCode.E;

        skillKey = KeyCode.P;

        mainMenuKey = KeyCode.Escape;


        isometricPlayer = GetComponent<IsometricPlayer>();
    }

    private void Update()
    {
        
        //Interface Abfrage des Cursos sollte implementiert werden.

        //Action-Bars

        if (Input.GetKeyDown(action1))
        {
            ActionButtonOnClick(0);
        }

        if (Input.GetKeyDown(action2))
        {
            ActionButtonOnClick(1);
        }

        if (Input.GetKeyDown(action3))
        {
            ActionButtonOnClick(2);
        }

        if (Input.GetKeyDown(action4))
        {
            ActionButtonOnClick(3);
        }

        if (Input.GetKeyDown(action5))
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

        if (Input.GetKeyDown(skillKey))
        {
            OpenCloseMenu(skillTab);
        }

        if (Input.GetKeyDown(inventoryKey))
        {
            OpenCloseMenu(inventoryTab);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (skillTabOpen)
            {
                OpenCloseMenu(skillTab);
                OpenCloseMenu(inventoryTab);
            }


            if (inventoryTabOpen)
            {
                OpenCloseMenu(inventoryTab);
                OpenCloseMenu(skillTab);
            }

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

    public void ShowItemTooltip(Vector3 position, Item item)
    {
 
        tooltip.SetActive(true);
        //tooltip.GetComponentInChildren<GameObject>().SetActive(true);
        tooltip.transform.position = position;
        //print("you got here");
        if (item != null)
        {
            //print("and for some suspicious reason you got here aswell. Hi there" + item.ItemName);
            //tooltipText = tooltip.GetComponentInChildren<Text>();

            tooltipText.text = "<b>"+item.ItemName + "</b>\n"+ item.ItemDescription + "\n\n" + item.GetValueDescription();
        }
    }

    public void ShowTooltip(Vector3 position, string description)
    {

        tooltip.SetActive(true);
        tooltip.transform.position = position;

        if (description != null)
        {
            tooltipText.text = description;
        }
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }


    public void OpenCloseMenu(CanvasGroup canvas)
    {
        canvas.alpha = canvas.alpha > 0 ? 0 : 1; 
        canvas.blocksRaycasts = canvas.blocksRaycasts == true ? false : true;


        //Extra Abfrage für Character Tab, sollte in eigener Methode überarbeitet werden eigentlich.
        if (canvas.name == "skillTab")
        {
            if(skillTabOpen)
            {
                skillTabOpen = false;
                inventoryTabOpen = true;
            }

            else if (!skillTabOpen)
            {
                skillTabOpen = true;
                inventoryTabOpen = false;
            }


        }

        else if (canvas.name == "inventoryTab")
        {
            if(!inventoryTabOpen)
            {
                inventoryTabOpen = true;
                skillTabOpen = false;
            }


            else if (inventoryTabOpen)
            {
                inventoryTabOpen = false;
                skillTabOpen = true;
            }

        }

    }


}
