using System;
using UnityEngine;

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
    public void Initialize(AbilityData data)
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
    }

    /// <summary>
    /// Die zentrale Update-Logik, die alle Timer und Zustände verwaltet.
    /// </summary>
    protected virtual void Update()
    {
        HandleChargeCooldown();
        HandleChanneling();
        HandleActiveDuration();
    }

    /// <summary>
    /// Wird aufgerufen, wenn der Spieler die Fähigkeit benutzen will.
    /// </summary>
    public void Use()
    {
        if (state != AbilityState.Ready) return; // Kann nur im Ready-State benutzt werden
        if (currentCharges <= 0) return;         // Benötigt mindestens eine Aufladung

        currentCharges--;
        if (chargeCooldownTimer <= 0) // Starte den Cooldown, wenn er nicht schon läuft
        {
            chargeCooldownTimer = cooldownTime;
        }

        // Entscheide basierend auf den Properties, was als Nächstes passiert
        if (properties.HasFlag(SpellProperty.Channeling))
        {
            state = AbilityState.Channeling;
            channelTimer = channelTime;
            tickCooldownTimer = 0; // Erster Tick sofort
        }
        else if (properties.HasFlag(SpellProperty.Persistent))
        {
            state = AbilityState.Active;
            activeTimer = activeTime;
            tickCooldownTimer = 0; // Erster Tick sofort
            UseBase(PlayerManager.instance.playerStats); // Effekt sofort auslösen
        }
        else // Für Instant-Fähigkeiten wie Enigma
        {
            UseBase(PlayerManager.instance.playerStats);
            // Instant-Fähigkeiten gehen nicht in den Active-State, sie sind sofort fertig.
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

        if (tickCooldownTimer <= 0)
        {
            OnTick(PlayerManager.instance.playerStats);
            tickCooldownTimer = tickTimer;
        }

        if (channelTimer <= 0)
        {
            // Channeling beendet, löse den finalen Effekt aus
            UseBase(PlayerManager.instance.playerStats);
            state = AbilityState.Ready;
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
            OnCooldown(PlayerManager.instance.playerStats); // Signalisiert das Ende des Effekts
            state = AbilityState.Ready;
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

    // --- INTERFACE-METHODEN (ANGEPASST) ---
    public bool IsOnCooldown() => currentCharges == 0 && chargeCooldownTimer > 0;
    public float GetCooldown() => chargeCooldownTimer;
    public bool IsActive() => state == AbilityState.Active || state == AbilityState.Channeling;
    public float GetActiveTime() => (state == AbilityState.Active) ? activeTimer : channelTimer;
    public int GetCurrentCharges() => currentCharges;
    public int GetMaxCharges() => maxCharges;
    public string GetName() => abilityName;
    #endregion
}


