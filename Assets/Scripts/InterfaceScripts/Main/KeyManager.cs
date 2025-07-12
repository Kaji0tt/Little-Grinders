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
            if (instance == null)
            {
                Debug.LogError("KeyManager instance is null. Make sure it's initialized properly.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Hält den KeyManager über Szenenwechsel hinweg
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Falls eine weitere Instanz erstellt wird, zerstöre sie
        }
    }


    public Dictionary<string, KeyCode> Keybinds { get; private set; }

    public Dictionary<string, KeyCode> ActionBinds { get; private set; }

    private string bindName;

    public string[] keyNames = { "UP", "LEFT", "DOWN", "RIGHT", "STATS", "SKILLS", "PICK", "MAP", "WEAPON", "KOPF", "BRUST", "BEINE", "SCHUHE" };



    void Start()
    {
        Keybinds = new Dictionary<string, KeyCode>();

        ActionBinds = new Dictionary<string, KeyCode>();



        //Possibly use Playerprefs on BindKey - e.g. PlayerPrefs.SetInt(key, KeyCode-to-number);
        //Then on load or start, if(PlayerPrefs.GetKey(key)), BindKey(PlayerPrefs.GetKey)
        //else do this assignment called below.

        foreach (var key in keyNames)
        {
            string prefKey = "KeyBind_" + key;
            if (PlayerPrefs.HasKey(prefKey))
            {
                BindKey(key, (KeyCode)PlayerPrefs.GetInt(prefKey));
            }
            else
            {
                // Standardwerte setzen
                switch (key)
                {
                    case "UP":
                        BindKey(key, KeyCode.W);
                        break;
                    case "LEFT":
                        BindKey(key, KeyCode.A);
                        break;
                    case "DOWN":
                        BindKey(key, KeyCode.S);
                        break;
                    case "RIGHT":
                        BindKey(key, KeyCode.D);
                        break;
                    case "STATS":
                        BindKey(key, KeyCode.C);
                        break;
                    case "SKILLS":
                        BindKey(key, KeyCode.X);
                        break;
                    case "PICK":
                        BindKey(key, KeyCode.F);
                        break;
                    case "MAP":
                        BindKey(key, KeyCode.M);
                        break;
                    case "WEAPON":
                        BindKey(key, KeyCode.Mouse1);
                        break;
                    case "KOPF":
                        BindKey(key, KeyCode.E);
                        break;
                    case "BRUST":
                        BindKey(key, KeyCode.R);
                        break;
                    case "BEINE":
                        BindKey(key, KeyCode.Q);
                        break;
                    case "SCHUHE":
                        BindKey(key, KeyCode.Space);
                        break;
                }
            }
        }
    }



    public void BindKey(string key, KeyCode keyBind)
    {
        // Prüfe, ob es sich um einen Action-Slot handelt
        Dictionary<string, KeyCode> currentDictionary = Keybinds;
        switch (key)
        {
            case "WEAPON":
            case "KOPF":
            case "BRUST":
            case "BEINE":
            case "SCHUHE":
                currentDictionary = ActionBinds;
                break;
            default:
                currentDictionary = Keybinds;
                break;
        }

        if (!currentDictionary.ContainsKey(key))
        {
            currentDictionary.Add(key, keyBind);
            UI_Manager.instance.UpdateKeyText(key, keyBind);
        }
        else if (currentDictionary.ContainsValue(keyBind))
        {
            string myKey = currentDictionary.FirstOrDefault(x => x.Value == keyBind).Key;
            currentDictionary[myKey] = KeyCode.None;
            UI_Manager.instance.UpdateKeyText(key, KeyCode.None);
        }

        currentDictionary[key] = keyBind;
        UI_Manager.instance.UpdateKeyText(key, keyBind);

        // Speichern in PlayerPrefs
        PlayerPrefs.SetInt("KeyBind_" + key, (int)keyBind);
        PlayerPrefs.Save();

        bindName = string.Empty;
    }



    public void KeyBindOnClick(string bindName)
    {
        this.bindName = bindName;
    }

    private void OnGUI()
    {
        if (bindName != string.Empty)
        {
            Event e = Event.current;

            if (e.isKey)
            {
                BindKey(bindName, e.keyCode);
            }
        }
    }
    

    /*
    Dein KeyManager ist schon ziemlich solide und übersichtlich!
Er trennt Action-Slots und normale Keybinds, unterstützt dynamisches Rebinding und ist Singleton-basiert.
Hier sind einige Verbesserungsvorschläge für mehr Robustheit und Komfort:

1. PlayerPrefs für Persistenz
Speichere die Keybinds beim Ändern und lade sie beim Start, damit die Einstellungen nach einem Neustart erhalten bleiben.

2. Doppelte Keybinds verhindern
Aktuell kann ein Key mehreren Aktionen zugewiesen werden.
Du könntest eine Warnung ausgeben oder das verhindern.

3. UI-Feedback beim Rebinding
Zeige im UI an, dass gerade ein Key geändert wird (z.B. „Drücke eine Taste...“).

4. Mouse- und Sondertasten-Support
Prüfe, ob auch Maustasten und Sondertasten (z.B. Escape, Tab) korrekt behandelt werden.

5. Refactoring: Dictionary-Auswahl
Du könntest die Dictionary-Auswahl in eine eigene Methode auslagern, um Redundanz zu vermeiden.

6. Events für Keybind-Änderungen
Falls andere Systeme auf Keybind-Änderungen reagieren sollen, könntest du ein Event auslösen.

7. Fehlerbehandlung
Prüfe, ob der Key existiert, bevor du ihn bindest, und gib ggf. eine Warnung aus.

Fazit:
Dein System ist schon sehr gut!
Mit Persistenz, besserem UI-Feedback und etwas Refactoring wird es noch benutzerfreundlicher und robuster.
    */
}
