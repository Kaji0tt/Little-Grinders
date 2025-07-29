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
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public Dictionary<string, KeyCode> Keybinds { get; private set; }
    public Dictionary<string, KeyCode> ActionBinds { get; private set; }

    private string bindName;

    public string[] keyNames = { "UP", "LEFT", "DOWN", "RIGHT", "STATS", "SKILLS", "CAMERA LOCK", "SHOW VALUES", "PICK", "MAP", "WEAPON", "KOPF", "BRUST", "BEINE", "SCHUHE" };

    void Start()
    {
        // Initialisiere Dictionaries
        Keybinds = new Dictionary<string, KeyCode>();
        ActionBinds = new Dictionary<string, KeyCode>();

        // Lade oder setze Defaults für alle Keys
        InitializeKeyBindings();

        // Debug-Output
        Debug.Log($"KeyManager initialized. CAMERA LOCK is bound to: {Keybinds["CAMERA LOCK"]}");
    }

    private void InitializeKeyBindings()
    {
        foreach (var key in keyNames)
        {
            KeyCode defaultKey = GetDefaultKeyCode(key);
            string prefKey = "KeyBind_" + key;

            if (PlayerPrefs.HasKey(prefKey))
            {
                // Lade aus PlayerPrefs
                KeyCode savedKey = (KeyCode)PlayerPrefs.GetInt(prefKey);
                SetKeyBinding(key, savedKey);
            }
            else
            {
                // Setze Default und speichere es
                SetKeyBinding(key, defaultKey);
                PlayerPrefs.SetInt(prefKey, (int)defaultKey);
            }
        }

        PlayerPrefs.Save();

        // Nach dem Initialisieren alle KeyBindButtons updaten
        if (UI_Manager.instance != null)
            UI_Manager.instance.UpdateAllKeyTexts();
    }

    private KeyCode GetDefaultKeyCode(string key)
    {
        switch (key)
        {
            case "UP": return KeyCode.W;
            case "LEFT": return KeyCode.A;
            case "DOWN": return KeyCode.S;
            case "RIGHT": return KeyCode.D;
            case "STATS": return KeyCode.C;
            case "SKILLS": return KeyCode.X;
            case "CAMERA LOCK": return KeyCode.Mouse2; // Mittlere Maustaste
            case "SHOW VALUES": return KeyCode.LeftAlt;
            case "PICK": return KeyCode.F;
            case "MAP": return KeyCode.M;
            case "WEAPON": return KeyCode.Mouse1;
            case "KOPF": return KeyCode.E;
            case "BRUST": return KeyCode.R;
            case "BEINE": return KeyCode.LeftShift;
            case "SCHUHE": return KeyCode.Space;
            default: return KeyCode.None;
        }
    }

    private void SetKeyBinding(string key, KeyCode keyCode)
    {
        // Bestimme das richtige Dictionary
        Dictionary<string, KeyCode> targetDict = IsActionKey(key) ? ActionBinds : Keybinds;

        // Setze das Key Binding
        targetDict[key] = keyCode;

        // Update UI falls vorhanden
        if (UI_Manager.instance != null)
            UI_Manager.instance.UpdateAllKeyTexts(); // <-- immer alle Buttons updaten!
    }

    private bool IsActionKey(string key)
    {
        return key == "WEAPON" || key == "KOPF" || key == "BRUST" || key == "BEINE" || key == "SCHUHE";
    }

    public void BindKey(string key, KeyCode keyBind)
    {
        // Null/Empty Check hinzufügen
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"BindKey called with null/empty key! KeyCode: {keyBind}");
            return;
        }

        // Prüfe, ob es sich um einen Action-Slot handelt
        Dictionary<string, KeyCode> currentDictionary = IsActionKey(key) ? ActionBinds : Keybinds;

        // Rest bleibt gleich...
        if (currentDictionary.ContainsValue(keyBind))
        {
            string conflictKey = currentDictionary.FirstOrDefault(x => x.Value == keyBind).Key;
            if (conflictKey != key)
            {
                currentDictionary[conflictKey] = KeyCode.None;
                if (UI_Manager.instance != null)
                    UI_Manager.instance.UpdateAllKeyTexts(); // <-- Konflikt-Button auch updaten
            }
        }

        currentDictionary[key] = keyBind;
        if (UI_Manager.instance != null)
            UI_Manager.instance.UpdateAllKeyTexts(); // <-- immer alle Buttons updaten

        PlayerPrefs.SetInt("KeyBind_" + key, (int)keyBind);
        PlayerPrefs.Save();

        bindName = string.Empty;

        Debug.Log($"Key binding updated: {key} = {keyBind}");
    }

    public void KeyBindOnClick(string bindName)
    {
        this.bindName = bindName;
    }

    private void OnGUI()
    {
        // Nur wenn bindName NICHT null oder leer ist
        if (!string.IsNullOrEmpty(bindName))
        {
            Event e = Event.current;

            // Nur auf KeyDown-Events reagieren
            if (e.type == EventType.KeyDown)
            {
                // Escape zum Abbrechen
                if (e.keyCode == KeyCode.Escape)
                {
                    Debug.Log($"Key binding for '{bindName}' cancelled.");
                    bindName = string.Empty;
                    return;
                }

                // Verhindere problematische Keys
                if (e.keyCode != KeyCode.None)
                {
                    BindKey(bindName, e.keyCode);
                }
            }
        }
    }

    // Debug-Methode zum Testen
    [ContextMenu("Debug Key Bindings")]
    public void DebugKeyBindings()
    {
        Debug.Log("=== Key Bindings Debug ===");
        foreach (var kvp in Keybinds)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
        foreach (var kvp in ActionBinds)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} (Action)");
        }
    }

    // Methode zum Zurücksetzen der PlayerPrefs (falls nötig)
    [ContextMenu("Reset All Key Bindings")]
    public void ResetKeyBindings()
    {
        foreach (var key in keyNames)
        {
            PlayerPrefs.DeleteKey("KeyBind_" + key);
        }
        PlayerPrefs.Save();
        Debug.Log("All key bindings reset. Restart the game to apply defaults.");
    }
}
