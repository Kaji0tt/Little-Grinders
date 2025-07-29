using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public abstract class Ability : MonoBehaviour, IUseable
{
    #region Ability Data
    // --- DATEN, DIE VON ABILITYDATA GELADEN WERDEN ---
    protected SpellProperty properties;
    protected string abilityName;
    public string description;
    protected Sprite icon;
    protected float cooldownTime;
    protected float activeTime;
    protected float tickTimer;
    protected float range;
    protected float areaOfEffectRadius;
    protected GameObject projectilePrefab;
    protected float projectileSpeed;
    protected float channelTime;
    protected int maxCharges;
    protected float rarityScaling;

    #endregion

    #region State Machine
    // --- ZUSTANDS-MASCHINE ---
    public enum AbilityState { Ready, Active, Channeling, Cooldown }
    protected AbilityState state = AbilityState.Ready;

    // --- LAUFZEIT-TIMER UND ZÄHLER ---
    private int currentCharges;
    private float chargeCooldownTimer; // Timer für die nächste Aufladung
    private float activeTimer;         // Timer für die Dauer von 'Persistent'
    private float channelTimer;        // Timer für die Dauer von 'Channeling'
    private float tickCooldownTimer;   // Timer für den nächsten Tick
    #endregion

    /// <summary>
    /// Initialisiert die Fähigkeit mit allen Werten aus dem AbilityData-Asset.
    /// </summary>
    public void Initialize(AbilityData data, float rarityMultiplier)
    {
        if (data == null) return;

        // Lade alle Daten aus dem ScriptableObject
        this.properties = data.properties;
        this.abilityName = data.abilityName;
        this.description = data.description;
        this.icon = data.icon;
        this.cooldownTime = data.cooldownTime;
        this.activeTime = data.activeTime;
        this.tickTimer = data.tickTimer;
        this.range = data.range;
        this.areaOfEffectRadius = data.areaOfEffectRadius;
        this.projectilePrefab = data.projectilePrefab;
        this.projectileSpeed = data.projectileSpeed;
        this.channelTime = data.channelTime;
        this.maxCharges = data.maxCharges;

        // Initialisiere das Aufladungssystem
        this.currentCharges = this.maxCharges;
        this.chargeCooldownTimer = 0;
        SetRarityScaling(rarityMultiplier); // NEU: Rarity Scaling anwenden
    }


    protected abstract void ApplyRarityScaling(float rarityScaling); // NEU

    public void SetRarityScaling(float value)
    {
        rarityScaling = value;
        ApplyRarityScaling(rarityScaling); // Jede Kindklasse muss jetzt reagieren!
    }
    // NEU: Für Reichweiten-Checks
    protected Vector3 currentTargetPosition;
   
    /// <summary>
    /// Die zentrale Update-Logik, die alle Timer und Zustände verwaltet.
    /// </summary>
    protected virtual void Update()
    {
        // NEU: Zielposition aktualisieren (nur für Targeted Abilities)
        UpdateTargetPosition();

        HandleChargeCooldown();
        HandleChanneling();
        HandleActiveDuration();
        HandleCooldown();
        OnUpdateAbility();

        if (state == AbilityState.Channeling)
        {
            if (channelSlowMod == null)
                OnChannelStart();

            KeyCode slotKey = KeyManager.MyInstance.ActionBinds.ContainsKey(myHotkey)
                ? KeyManager.MyInstance.ActionBinds[myHotkey]
                : KeyCode.None;

            // Channeling beenden und Zauber auslösen, wenn Key losgelassen wird
            if (Input.GetKeyUp(slotKey))
            {
                OnChannelEnd();

                if (!IsPointerOverUI())
                {
                    UseBase(PlayerManager.instance.playerStats);

                    if (maxCharges == 1 && chargeCooldownTimer <= 0)
                    {
                        chargeCooldownTimer = cooldownTime;
                        state = AbilityState.Cooldown;
                    }
                    else
                    {
                        state = AbilityState.Ready;
                    }
                }

                return;
            }

            // Abbruch bei anderem Input
            if (AnyNonMovementInput())
                AbortChanneling();
        }
        else
        {
            if (channelSlowMod != null)
                OnChannelEnd();
        }

        if (properties.HasFlag(SpellProperty.Persistent))
            OnPersistentUpdate();
    }


    // NEU: Kann von Kindklassen überschrieben werden, um spezifische Ziele zu setzen
    protected virtual void UpdateTargetPosition()
    {
        if (properties.HasFlag(SpellProperty.Targeted))
        {
            currentTargetPosition = GetTargetPosition();
        }
    }

    /// <summary>
    /// Kann von Kindklassen überschrieben werden, um eigene Update-Logik einzubauen.
    /// </summary>
    protected virtual void OnUpdateAbility() { }

    // NEU: Kann von persistenten Fähigkeiten überschrieben werden
    protected virtual void OnPersistentUpdate() { }

    /// <summary>
    /// Wird aufgerufen, wenn der Spieler die Fähigkeit benutzen will.
    /// </summary>
    public void Use()
    {
        if (IsPointerOverUI()) return;
        if (state != AbilityState.Ready) return;
        if (currentCharges <= 0) return;

        currentCharges--;

        // Cooldown sofort starten, wenn mehrere Charges vorhanden sind oder die Fähigkeit instant ist
        if (maxCharges > 1 || (!properties.HasFlag(SpellProperty.Active) && !properties.HasFlag(SpellProperty.Channeling)))
        {
            if (chargeCooldownTimer <= 0)
            {
                chargeCooldownTimer = cooldownTime;
                //state = AbilityState.Cooldown; // <-- NEU: State explizit auf Cooldown setzen
            }
        }

        if (properties.HasFlag(SpellProperty.Channeling))
        {
            state = AbilityState.Channeling;
            channelTimer = channelTime;
            tickCooldownTimer = 0;
        }
        else if (properties.HasFlag(SpellProperty.Active))
        {
            state = AbilityState.Active;
            activeTimer = activeTime;
            tickCooldownTimer = 0;
            UseBase(PlayerManager.instance.playerStats);
        }
        else // Instant
        {
            UseBase(PlayerManager.instance.playerStats);
        }
    }

    #region Timer Handling
    private void HandleChargeCooldown()
    {
        if (currentCharges < maxCharges)
        {
            chargeCooldownTimer -= Time.deltaTime;
            if (chargeCooldownTimer <= 0)
            {
                currentCharges++;
                // Wenn wir immer noch nicht voll sind, starte den Cooldown für die nächste Aufladung
                if (currentCharges < maxCharges)
                {
                    chargeCooldownTimer = cooldownTime;
                }
            }
        }
    }

    private void HandleChanneling()
    {
        if (state != AbilityState.Channeling) return;

        channelTimer -= Time.deltaTime;
        tickCooldownTimer -= Time.deltaTime;

        OnChannelUpdate();

        if (tickCooldownTimer <= 0)
        {
            OnTick(PlayerManager.instance.playerStats);
            tickCooldownTimer = tickTimer;
        }

        if (channelTimer <= 0)
        {
            OnChannelEnd();
            UseBase(PlayerManager.instance.playerStats);

            // NEU: Cooldown setzen wie bei Active
            if (maxCharges == 1 && chargeCooldownTimer <= 0)
            {
                chargeCooldownTimer = cooldownTime;
                state = AbilityState.Cooldown;
            }
            else
            {
                state = AbilityState.Ready;
            }
        }
    }

    private void HandleActiveDuration()
    {
        if (state != AbilityState.Active) return;

        activeTimer -= Time.deltaTime;
        tickCooldownTimer -= Time.deltaTime;

        if (tickCooldownTimer <= 0)
        {
            OnTick(PlayerManager.instance.playerStats);
            tickCooldownTimer = tickTimer;
        }

        if (activeTimer <= 0)
        {
            OnCooldown(PlayerManager.instance.playerStats);

            // Cooldown nur setzen, wenn es keine weiteren Charges gibt
            if (maxCharges == 1 && chargeCooldownTimer <= 0)
            {
                chargeCooldownTimer = cooldownTime;
                state = AbilityState.Cooldown; // NEU: State explizit auf Cooldown setzen
            }
            else
            {
                state = AbilityState.Ready;
            }
        }
    }

    private void HandleCooldown()
    {
        if (state != AbilityState.Cooldown) return;

        chargeCooldownTimer -= Time.deltaTime;
        if (chargeCooldownTimer <= 0)
        {
            chargeCooldownTimer = 0;
            state = AbilityState.Ready;
            // Optional: Debug.Log($"[Ability] {abilityName} ist wieder bereit!");
        }
    }

    private StatModifier channelSlowMod;

    // Channeling-Start: Bewegung drosseln & Event abonnieren
    protected virtual void OnChannelStart()
    {
        var player = PlayerManager.instance.playerStats;
        channelSlowMod = new StatModifier(-0.7f, StatModType.PercentMult, this);
        player.MovementSpeed.AddModifier(channelSlowMod);

        GameEvents.Instance.OnPlayerWasAttacked += OnPlayerWasAttacked;

        OnChannelEnter(); // NEU: Hook für Kindklassen
    }

    // Channeling-Ende: Bewegung zurücksetzen & Event abbestellen
    protected virtual void OnChannelEnd()
    {
        var player = PlayerManager.instance.playerStats;
        if (channelSlowMod != null)
            player.MovementSpeed.RemoveModifier(channelSlowMod);
        channelSlowMod = null;

        GameEvents.Instance.OnPlayerWasAttacked -= OnPlayerWasAttacked;

        OnChannelExit(); // NEU: Hook für Kindklassen
    }

    // Wird vom Event aufgerufen
    private void OnPlayerWasAttacked(float damage)
    {
        if (state != AbilityState.Channeling) return;
        if (damage <= 0) return;

        // Channeling NICHT abbrechen, sondern Timer zurücksetzen
        //channelTimer = channelTime;
        tickCooldownTimer = tickTimer;
        Debug.Log("[Ability] Channeling-Timer durch Schaden zurückgesetzt!");
    }

    // Channeling-Abbruch (durch Input oder Schaden)
    protected void AbortChanneling()
    {
        OnChannelEnd();
        state = AbilityState.Ready;
        channelTimer = 0;
        tickCooldownTimer = 0;
        Debug.Log("[Ability] Channeling abgebrochen!");
    }

    // NEU: Methoden für Reichweiten-Checks
    public virtual bool HasRange() 
    { 
        return range > 0; 
    }

    public virtual float GetRange() 
    { 
        return range; 
    }

    public virtual Vector3 GetTargetPosition() 
    {
        // Für Targeted Abilities: Nutze das TargetSystem
        if (properties.HasFlag(SpellProperty.Targeted))
        {
            // Hole das aktuelle Ziel aus dem Combat-System
            var characterCombat = PlayerManager.instance.player.GetComponent<CharacterCombat>();
            if (characterCombat != null && characterCombat.currentTarget != null)
            {
                return characterCombat.currentTarget.transform.position;
            }
        }
        
        // Für alle anderen Abilities mit Reichweite: Mausposition auf dem Boden
        return GetMouseWorldPosition();
    }

    // NEU: Kann von Kindklassen überschrieben werden, um spezifische Ziele zu setzen
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Raycast auf den Floor mit spezifischem LayerMask
        int floorLayerMask = LayerMask.GetMask("Floor");
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, floorLayerMask))
        {
            return hit.point;
        }
        
        // Alternative: Raycast auf alle Objekte mit Floor-Tag
        if (Physics.Raycast(ray, out RaycastHit floorHit, 200f))
        {
            if (floorHit.collider.CompareTag("Floor"))
            {
                return floorHit.point;
            }
        }
        
        // Fallback: Mausposition auf Y=0 Ebene projizieren
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        // Letzter Fallback: Spielerposition
        return transform.position;
    }

    // NEU: Methode für Range-Check speziell für UI-Feedback
    public virtual bool IsInRange()
    {
        if (range <= 0) return true; // Keine Reichweitenbeschränkung
    
        Vector3 playerPosition = PlayerManager.instance.player.transform.position;
    
        if (properties.HasFlag(SpellProperty.Targeted))
        {
            // Für Targeted Abilities: Prüfe aktuelles Target
            var characterCombat = PlayerManager.instance.player.GetComponent<CharacterCombat>();
            if (characterCombat?.currentTarget != null)
            {
                float distance = Vector3.Distance(playerPosition, characterCombat.currentTarget.transform.position);
                return distance <= range;
            }
            return false; // Kein Target = nicht in Reichweite
        }
        else
        {
            // Für Ground-Targeted: Prüfe Mausposition
            Vector3 targetPosition = GetMouseWorldPosition();
            float distance = Vector3.Distance(playerPosition, targetPosition);
            return distance <= range;
        }
    }

    #endregion

    #region Abstract & Interface Methods
    // --- ABSTRAKTE METHODEN (BLEIBEN GLEICH) ---
    // Wird bei Instant/Channeling-Ende/Persistent-Start ausgelöst
    public abstract void UseBase(IEntitie entitie);
    // Wird bei jedem Tick von Persistent/Channeling ausgelöst
    public abstract void OnTick(IEntitie entitie);
    // Wird am Ende von Persistent ausgelöst
    public abstract void OnCooldown(IEntitie entitie);

        // Channeling-Hooks (können von Kindklassen überschrieben werden)
    protected virtual void OnChannelEnter() { }
    protected virtual void OnChannelUpdate() { }
    protected virtual void OnChannelExit() { }

    // --- INTERFACE-METHODEN (ANGEPASST) ---
    public bool IsOnCooldown() => currentCharges == 0 && chargeCooldownTimer > 0;
    public float GetCooldown() => chargeCooldownTimer;
    public bool IsActive() => state == AbilityState.Active || state == AbilityState.Channeling;
    public float GetActiveTime() => (state == AbilityState.Active) ? activeTimer : channelTimer;
    public int GetCurrentCharges() => currentCharges;
    public int GetMaxCharges() => maxCharges;
    public string GetName() => abilityName;
    // NEU: Interface-Implementierung für Description
    public string GetDescription() 
    { 
        // Automatischer Umbruch alle 50-60 Zeichen
        return FormatDescription(description);
    }

    private string FormatDescription(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        const int maxLineLength = 50;
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";
        
        foreach (var word in words)
        {
            if ((currentLine + " " + word).Length > maxLineLength && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine.Trim());
                currentLine = word;
            }
            else
            {
                currentLine += (string.IsNullOrEmpty(currentLine) ? "" : " ") + word;
            }
        }
        
        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine.Trim());
            
        return string.Join("\n", lines);
    }
    #endregion


    // Beispiel für die zentrale Prüfung in Ability:
    protected bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    protected string myHotkey = "None"; // Default, kann beim Setzen überschrieben werden

    public void SetSlotName(string newHotkey) { myHotkey = newHotkey;}

    // Prüft, ob ein anderer Input als WASD gedrückt wurde
    protected bool AnyNonMovementInput()
    {
        KeyCode slotKey = KeyManager.MyInstance.ActionBinds.ContainsKey(myHotkey)
            ? KeyManager.MyInstance.ActionBinds[myHotkey]
            : KeyCode.None;

        // Interface/Interaktionstasten
        bool interfaceInput =
            Input.GetKeyDown(KeyCode.E) || // Menü
            Input.GetKeyDown(KeyCode.F) || // Interagieren
            Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Tab) ||
            Input.GetMouseButtonDown(0) || // Linksklick
            Input.GetMouseButtonDown(1);   // Rechtsklick

        // Bewegungstasten und Slot5
        bool movementOrAbility =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(slotKey);

        // Abbruch, wenn Interface/Interaktionstaste gedrückt wurde und NICHT Bewegung/SLOT5
        return Input.anyKeyDown && !movementOrAbility || interfaceInput;
    }
}


