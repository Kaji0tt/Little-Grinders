using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // MyUseable bleibt die zentrale Referenz zur Fähigkeit.
    public IUseable MyUseable { get; private set; }

    // Gem-System: ActionButtons sind jetzt an GemTypes gebunden
    [Tooltip("Für welchen Gem-Slot ist dieser Button zuständig?")]
    public GemType myGemType;

    // Referenzen zu den UI-Komponenten
    public Button MyButton { get; private set; }
    private Image myIcon;
    private GameObject cdOverlay;
    private Text cdText;

    // Hält eine Referenz zum erstellten Fähigkeits-Objekt, um es später aufräumen zu können.
    private GameObject currentAbilityObject;

    [Header("Visual Feedback Settings")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private float pulseSpeed = 2.5f;

    [Header("Target Range Visual Feedback")]
    [SerializeField] private Color outOfRangeColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color inRangeColor = Color.white;

    void Awake()
    {
        // Referenzen holen
        MyButton = GetComponent<Button>();
        
        // Wir suchen jetzt explizit nach dem Kind-Objekt namens "Image",
        // um sicherzustellen, dass 'myIcon' auf die korrekte, sichtbare Komponente verweist.
        Transform iconTransform = transform.Find("Image");
        if (iconTransform != null)
        {
            myIcon = iconTransform.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning($"ActionButton '{gameObject.name}' konnte kein Kind-Objekt namens 'Image' für das Icon finden.", this);
        }

        // Annahme: Das Cooldown-Overlay ist ein Kind-Objekt namens "CD"
        Transform overlayTransform = transform.Find("CD");
        if (overlayTransform != null)
        {
            cdOverlay = overlayTransform.gameObject;
            cdText = cdOverlay.GetComponentInChildren<Text>();

            //Outline für den Cooldown-Text hinzufügen
            if (cdText != null)
            {
                // Hole die Outline-Komponente oder füge eine neue hinzu, falls keine existiert.
                Outline outline = cdText.GetComponent<Outline>() ?? cdText.gameObject.AddComponent<Outline>();
                
                // Konfiguriere die Outline nach deinem Wunsch.
                outline.effectColor = Color.green;
                outline.effectDistance = new Vector2(1.2f, -1.2f);
            }
        }

        // Listener für den Klick hinzufügen
        MyButton.onClick.AddListener(OnClick);

        // Button initial leeren
        Clear();
    }
    
    /// <summary>
    /// Wird aufgerufen, wenn der Button geklickt wird.
    /// Die Logik ist jetzt extrem einfach.
    /// </summary>
    public void OnClick()
    {
        if (MyUseable != null)
        {
            // Ruft einfach die Use-Methode der Fähigkeit auf.
            // Die Fähigkeit selbst weiß, ob sie bereit ist oder nicht.
            MyUseable.Use();
        }
    }

    /// <summary>
    /// Aktualisiert die visuelle Darstellung des Buttons (Cooldown, Aktiv-Status, Aufladungen).
    /// </summary>
    void Update()
    {
        if (MyUseable == null)
        {
            return; // Nichts zu tun, wenn keine Fähigkeit zugewiesen ist.
        }

        // Zustand 1: Fähigkeit ist AKTIV (z.B. Channeling, Persistent)
        if (MyUseable.IsActive())
        {
            if (cdOverlay != null) cdOverlay.SetActive(true);
            if (myIcon != null)
            {
                // Pulsierender Effekt für die aktive Zeit
                float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
                Color darkPulseColor = activeColor * 0.6f;
                darkPulseColor.a = activeColor.a;
                myIcon.color = Color.Lerp(activeColor, darkPulseColor, pingPong);
            }
            if (cdText != null)
            {
                cdText.text = MyUseable.GetActiveTime().ToString("F1");
            }
        }
        // Zustand 2: Fähigkeit ist nicht aktiv (Bereit, Cooldown oder hat Aufladungen)
        else
        {
            int currentCharges = MyUseable.GetCurrentCharges();
            int maxCharges = MyUseable.GetMaxCharges();

            // Fall A: Die Fähigkeit hat mehrere Aufladungen
            if (maxCharges > 1)
            {
                if(cdOverlay != null) cdOverlay.SetActive(true);
                
                // NEU: Range-Check für Multi-Charge Abilities
                Color iconColor = (currentCharges > 0) ? GetRangeBasedColor() : cooldownColor;
                if (myIcon != null) myIcon.color = iconColor;

                string chargesText = currentCharges.ToString();
                if (currentCharges < maxCharges)
                {
                    chargesText += $": {MyUseable.GetCooldown():F1}";
                }
                cdText.text = chargesText;
                cdText.color = Color.white;
                var outline = cdText.GetComponent<Outline>();
                if (outline != null) outline.effectColor = Color.black;
            }
            // Fall B: Standard-Fähigkeit mit einer Aufladung
            else
            {
                float cooldown = MyUseable.GetCooldown();
                if (cooldown > 0)
                {
                    if (cdOverlay != null) cdOverlay.SetActive(true);
                    if (myIcon != null) myIcon.color = cooldownColor;
                    if (cdText != null)
                    {
                        cdText.text = cooldown.ToString("F1");
                        cdText.color = new Color(0.85f, 0.85f, 0.85f); // Hellgrau
                        var outline = cdText.GetComponent<Outline>();
                        if (outline != null) outline.effectColor = Color.black;
                    }
                }
                else // Bereit
                {
                    if (cdOverlay != null) cdOverlay.SetActive(false);
                    
                    // NEU: Range-Check für Single-Charge Abilities
                    if (myIcon != null) myIcon.color = GetRangeBasedColor();
                    
                    if (cdText != null)
                    {
                        cdText.text = "";
                        cdText.color = Color.white;
                        var outline = cdText.GetComponent<Outline>();
                        if (outline != null) outline.effectColor = Color.black;
                    }
                }
            }
        }
    }

    /// <summary>
    /// NEU: Prüft die Reichweite zur aktuellen Zielposition und gibt die entsprechende Farbe zurück.
    /// </summary>
    private Color GetRangeBasedColor()
    {
        if (MyUseable == null) return inRangeColor;

        if (MyUseable is Ability ability && ability.HasRange())
        {
            return ability.IsInRange() ? inRangeColor : outOfRangeColor;
        }

        return inRangeColor;
    }

    // NEU: Tooltip-Funktionalität beim Hovern
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MyUseable == null)
        {
            return;
        }
        
        if (UI_Manager.instance == null)
        {
            return;
        }
        
        string description = MyUseable.GetDescription();
        
        if (string.IsNullOrEmpty(description))
        {
            return;
        }
        
        UI_Manager.instance.ShowTooltip(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UI_Manager.instance == null)
        {
            return;
        }
        
        UI_Manager.instance.HideTooltip();
    }

    /// <summary>
    /// Weist diesem Button eine neue Fähigkeit basierend auf den Daten zu.
    /// Erstellt eine Instanz der Fähigkeit aus dem Prefab.
    /// </summary>
    public void SetAbility(AbilityData data, float rarityMultiplier)
    {
        // Zuerst die alte Fähigkeit entfernen und aufräumen.
        Clear();

        if (data == null || data.abilityPrefab == null)
        {
            return; // Nichts zu tun, wenn keine gültigen Daten vorhanden sind.
        }

        // Erstelle eine neue Instanz der Fähigkeit aus dem Prefab.
        currentAbilityObject = Instantiate(data.abilityPrefab, transform);
        
        // Holen Sie sich die Ability-Komponente von der neuen Instanz.
        Ability abilityComponent = currentAbilityObject.GetComponent<Ability>();

        if (abilityComponent != null)
        {
            // --- DAS IST DER ENTSCHEIDENDE FIX ---
            // Initialisiere die neue Fähigkeit mit den übergebenen Daten.
            abilityComponent.Initialize(data, rarityMultiplier);

            // Setze den SlotNamen der Fähigkeit.
            // Dies ist wichtig, damit die Fähigkeit weiß, über welche Taste sie im KeyManager aufgerufen wird.
            abilityComponent.SetSlotName(myGemType.ToString().ToUpper());

            //abilityComponent.SetRarityScaling(rarityMultiplier); // NEU: Wert setzen!

            MyUseable = abilityComponent;

            // Aktualisiere das Icon auf dem Button.
            if (myIcon != null)
            {
                myIcon.sprite = data.icon;
                myIcon.enabled = true;
                myIcon.color = Color.white;
            }
        }
        else
        {
            Debug.LogError($"Das Prefab '{data.abilityPrefab.name}' in AbilityData '{data.name}' hat keine 'Ability'-Komponente!");
            Destroy(currentAbilityObject); // Zerstöre das nutzlose Objekt.
        }
    }

    /// <summary>
    /// Setzt den Button zurück und entfernt die Fähigkeit.
    /// </summary>
    public void Clear()
    {
        // Zerstöre das alte Fähigkeits-GameObject, falls vorhanden.
        if (currentAbilityObject != null)
        {
            Destroy(currentAbilityObject);
        }

        MyUseable = null;
        currentAbilityObject = null;

        if (myIcon != null)
        {
            myIcon.sprite = null;
            myIcon.enabled = false;
        }
        if (cdOverlay != null)
        {
            cdOverlay.SetActive(false);
        }
    }

    public ItemInstance CurrentGemInstance { get; private set; }

    /// <summary>
    /// VERALTET: Wird nicht mehr verwendet (Abilities kommen jetzt nur von Gems)
    /// </summary>
    [Obsolete("Use SetAbilityFromGem instead")]
    public void SetItemInstance(ItemInstance item)
    {
        Debug.LogWarning("SetItemInstance ist veraltet! Abilities werden nur noch über Gems im Talentbaum verwaltet.");
        Clear();
    }

    /// <summary>
    /// Setzt die Ability basierend auf einem Gem aus dem Talentbaum
    /// </summary>
    /// <param name="gem">Das Gem mit der Ability</param>
    /// <param name="totalSkillpoints">Gesamtanzahl Skillpunkte für diesen GemType (für Ability-Verstärkung)</param>
    public void SetAbilityFromGem(ItemInstance gem, int totalSkillpoints)
    {
        Clear();
        CurrentGemInstance = gem;

        if (gem == null || gem.itemType != ItemType.Gem)
        {
            Debug.LogWarning("Kein gültiges Gem zum Setzen der Ability!");
            return;
        }

        // Prüfe ob das Gem eine Ability hat
        if (gem.gemAbility == null)
        {
            Debug.LogWarning($"Gem {gem.ItemName} hat keine Ability (gemAbility ist null)!");
            return;
        }

        // Berechne Rarity-Multiplier basierend auf totalSkillpoints
        // Pro Skillpoint: +10% Stärke (z.B. 5 Skillpoints = 1.5x Stärke)
        float rarityMultiplier = 1f + (totalSkillpoints * 0.1f);
        
        Debug.Log($"[ActionButton] Setze Ability '{gem.gemAbility.abilityName}' für GemType {myGemType} mit Multiplier: {rarityMultiplier:F2} ({totalSkillpoints} Skillpoints)");
        
        // Setze die Ability mit dem berechneten Multiplier
        SetAbility(gem.gemAbility, rarityMultiplier);
    }

    /// <summary>
    /// Prüft ob diese Ability gerade auf Cooldown ist (verhindert Gem-Tausch während Cooldown)
    /// </summary>
    public bool IsOnCooldown()
    {
        if (MyUseable == null) return false;
        return MyUseable.IsOnCooldown();
    }
}
