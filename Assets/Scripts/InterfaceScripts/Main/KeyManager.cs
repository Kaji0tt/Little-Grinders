using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyManager : MonoBehaviour
{
    private static KeyManager instance;

    public static KeyManager MyInstance
    {
        get
        {
            KeyManager[] instances = FindObjectsOfType<KeyManager>();

            if (instance == null)
            {

                instance = instances[0];
            }
            if (instances.Length > 1)
            {
                Destroy(instances[1]);
            }

            return instance;
        }
    }

    public Dictionary<string, KeyCode> Keybinds { get; private set; }

    public Dictionary<string, KeyCode> ActionBinds { get; private set; }

    private string bindName;

    public string[] keyNames = { "UP", "LEFT", "DOWN", "RIGHT", "STATS", "SKILLS", "PICK", "MAP", "SLOT1", "SLOT2", "SLOT3", "SLOT4", "SLOT5" };

    void Awake()
    {

        //Der KeyManager muss Szeneübergreifen bestehen bleiben.
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        Keybinds = new Dictionary<string, KeyCode>();

        ActionBinds = new Dictionary<string, KeyCode>();



        //Possibly use Playerprefs on BindKey - e.g. PlayerPrefs.SetInt(key, KeyCode-to-number);
        //Then on load or start, if(PlayerPrefs.GetKey(key)), BindKey(PlayerPrefs.GetKey)
        //else do this assignment called below.
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

            UI_Manager.instance.UpdateKeyText(key, keyBind);

        }
        else if(currentDictionary.ContainsValue(keyBind))
        {
            string myKey = currentDictionary.FirstOrDefault(x => x.Value == keyBind).Key;

            currentDictionary[myKey] = KeyCode.None;

            UI_Manager.instance.UpdateKeyText(key, KeyCode.None);

        }

        currentDictionary[key] = keyBind;

        UI_Manager.instance.UpdateKeyText(key, keyBind);

        bindName = string.Empty;
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
