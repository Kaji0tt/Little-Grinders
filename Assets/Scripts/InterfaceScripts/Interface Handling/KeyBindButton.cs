// KeyBindButton.cs
using UnityEngine;
using TMPro;

/// <summary>
/// Dieses Script liest den Key automatisch aus dem GameObject-Namen.
/// Der Name des GameObjects MUSS exakt mit dem Key im KeyManager übereinstimmen (z.B. "UP", "LEFT", ...).
/// Das Textfeld (keyText) wird automatisch per GetComponentInChildren<TextMeshProUGUI>() gesucht.
/// </summary>
[DisallowMultipleComponent]
public class KeyBindButton : MonoBehaviour
{
    [Tooltip("Wird automatisch aus dem GameObject-Namen gesetzt! Der Name MUSS exakt wie im KeyManager sein.")]
    public string keyName;

    [Tooltip("Wird automatisch gesucht. Zeigt den aktuellen Key an.")]
    public TextMeshProUGUI keyText;

    void Awake()
    {
        // Automatisch den Key aus dem GameObject-Namen setzen
        keyName = gameObject.name;

        // Automatisch das Textfeld suchen
        if (keyText == null)
            keyText = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateKeyText();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Editor-Hinweis
        keyName = gameObject.name;
        if (keyText == null)
            keyText = GetComponentInChildren<TextMeshProUGUI>();
    }
#endif

    public void UpdateKeyText()
    {
        if (KeyManager.MyInstance != null && keyText != null)
        {
            KeyCode code = KeyCode.None;
            // Prüfe zuerst ActionBinds, dann Keybinds
            if (KeyManager.MyInstance.ActionBinds != null && KeyManager.MyInstance.ActionBinds.ContainsKey(keyName))
            {
                code = KeyManager.MyInstance.ActionBinds[keyName];
            }
            else if (KeyManager.MyInstance.Keybinds != null && KeyManager.MyInstance.Keybinds.ContainsKey(keyName))
            {
                code = KeyManager.MyInstance.Keybinds[keyName];
            }
            keyText.text = code.ToString();
        }
    }
}