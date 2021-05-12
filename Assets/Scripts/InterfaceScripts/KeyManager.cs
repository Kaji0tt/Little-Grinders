using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    private static KeyManager instance;

    public static KeyManager MyInstance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<KeyManager>();
            }

            return instance;
        }
    }

    public Dictionary<string, KeyCode> Keybinds { get; private set; }

    public Dictionary<string, KeyCode> ActionBinds { get; private set; }

    private string bindName;

    private GameObject[] keybindButtons;

    void Awake()
    {
        keybindButtons = GameObject.FindGameObjectsWithTag("KeyControl");

        //Der KeyManager muss Szeneübergreifen bestehen bleiben.
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        Keybinds = new Dictionary<string, KeyCode>();

        ActionBinds = new Dictionary<string, KeyCode>();

        BindKey("UP", KeyCode.W);
        BindKey("LEFT", KeyCode.A);
        BindKey("DOWN", KeyCode.S);
        BindKey("RIGHT", KeyCode.D);

        BindKey("STATS", KeyCode.E);
        BindKey("SKILLS", KeyCode.P);
        BindKey("PICK", KeyCode.Q);
        BindKey("MAP", KeyCode.M);

        BindKey("SLOT1", KeyCode.Alpha1);
        BindKey("SLOT2", KeyCode.Alpha2);
        BindKey("SLOT3", KeyCode.Alpha3);
        BindKey("SLOT4", KeyCode.Alpha4);
        BindKey("SLOT5", KeyCode.Alpha5);
    }

    public void BindKey(string key, KeyCode keyBind)
    {
        Dictionary<string, KeyCode> currentDictionary = Keybinds;
        if (key.Contains("SLOT"))
        {
            currentDictionary = ActionBinds;
        }
        if(!currentDictionary.ContainsKey(key))
        {
            currentDictionary.Add(key, keyBind);

            UpdateKeyText(key, keyBind);

        }
        else if(currentDictionary.ContainsValue(keyBind))
        {
            string myKey = currentDictionary.FirstOrDefault(x => x.Value == keyBind).Key;

            currentDictionary[myKey] = KeyCode.None;

            UpdateKeyText(key, KeyCode.None);

        }

        currentDictionary[key] = keyBind;

        UpdateKeyText(key, keyBind);

        bindName = string.Empty;
    }

    public void UpdateKeyText(string key, KeyCode code)
    {
        TextMeshProUGUI tmp = Array.Find(keybindButtons, x => x.name == key).GetComponentInChildren<TextMeshProUGUI>();

        if(code.ToString().Contains("Alpha"))
        {
            tmp.text = code.ToString().Substring(5);
        }
        else
        tmp.text = code.ToString();

    }

    public void KeyBindOnClick(string bindName)
    {
        this.bindName = bindName;
    }

    private void OnGUI()
    {
        if(bindName != string.Empty)
        {
            Event e = Event.current;

            if(e.isKey)
            {
                BindKey(bindName, e.keyCode);
            }
        }
    }
}
