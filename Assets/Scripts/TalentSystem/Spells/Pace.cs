using UnityEngine;

public class Pace : Ability
{
    private int chargeCount = 3;  // Startet mit 3 Aufladungen
    private const int maxCharges = 3;  // Maximal 3 Aufladungen
    private float defaultSpeedModifier = 0.2f;  // 20% Geschwindigkeitsbonus pro Aufladung
    private float chargeDuration = 3f;  // Standarddauer pro Aufladung
    private float maxDuration = 5f;
    //new private float cooldownTime = 5f;  // Cooldown für eine Aufladung
    private float cooldownDuration;
    //new private float activeTime = 0f;

    private StatModifier speedModifier;

    public Pace_UI paceUI;  // UI-Komponente für die Ladungsanzeige

    private void Start()
    {
        paceUI.UpdateChargeUI(chargeCount);  // Initialisiere die UI-Anzeige basierend auf den aktuellen Ladungen
    }

    #region UseRegion

    public override void UseBase(IEntitie entitie)
    {
        // Falls keine Aufladung verfügbar ist, breche die Verwendung ab
        if (chargeCount <= 0)
        {
            Debug.Log("Keine Aufladungen verfügbar.");
            return;
        }

        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        // Reduziere die Aufladungen und aktualisiere die UI
        chargeCount--;
        paceUI.UpdateChargeUI(chargeCount);

        // Füge den StatModifier hinzu, der die Bewegungsgeschwindigkeit erhöht
        speedModifier = new StatModifier(defaultSpeedModifier, StatModType.PercentAdd, this);
        playerStats.MovementSpeed.AddModifier(speedModifier);

        // Setze die aktive Zeit für die aktuelle Aufladung auf die Standarddauer, ohne die maximale Dauer zu überschreiten
        activeTime = Mathf.Min(activeTime + chargeDuration, maxDuration);


        // Auswahl der Spezialisierung und deren zusätzliche Effekte
        switch (abilitySpec)
        {
            case AbilitySpecialization.Spec1:
                OnUseSpec1(entitie);
                break;

            case AbilitySpecialization.Spec2:
                OnUseSpec2(entitie);
                break;

            case AbilitySpecialization.Spec3:
                OnUseSpec3(entitie);
                break;
        }
    }

    private void EndActiveEffect()
    {
        // Entferne den StatModifier nach Ablauf der aktiven Zeit
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        playerStats.MovementSpeed.RemoveModifier(speedModifier);
    }

    private void RecoverCharge()
    {
        // Füge eine Aufladung hinzu, falls unter dem Maximum
        if (chargeCount < maxCharges)
        {
            chargeCount++;
            paceUI.UpdateChargeUI(chargeCount);  // Aktualisiere die UI-Anzeige
        }

        // Beende die Wiederherstellung, wenn das Maximum erreicht ist
        if (chargeCount >= maxCharges)
        {
            CancelInvoke(nameof(RecoverCharge));
        }
    }

    public override void OnUseSpec1(IEntitie entitie)
    {
        // Die Dauer pro Aufladung beträgt 6 Sekunden, maximale Dauer kann 8 Sekunden erreichen
        chargeDuration = Mathf.Min(6f + activeTime, 8f);
    }

    public override void OnUseSpec2(IEntitie entitie)
    {
        // Teleportiert den Spieler 2f in Richtung Mauszeiger bei jeder Aktivierung
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 teleportDirection = (mousePos - PlayerManager.instance.player.transform.position).normalized * 2f;
        PlayerManager.instance.player.transform.position += teleportDirection;
    }

    public override void OnUseSpec3(IEntitie entitie)
    {
        // Geschwindigkeit um 200% erhöhen für 0.5 Sekunden und Schaden an berührte Gegner
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        StatModifier burstSpeedModifier = new StatModifier(2f, StatModType.PercentAdd, this);
        playerStats.MovementSpeed.AddModifier(burstSpeedModifier);

        // Entferne den Burst-Modifier nach 0.5 Sekunden
        Invoke("RemoveBurstSpeed", 0.5f);
    }

    private void RemoveBurstSpeed()
    {
        PlayerManager.instance.player.GetComponent<PlayerStats>().MovementSpeed.RemoveModifier(speedModifier);
    }

    /// <summary>
    /// Beim Tick: 
    /// 
    /// Veringere die Aktive Zeit (activeTimeLeft) um 1;
    ///   ->Wenn die Aktive Zeit (activeTimeLeft) kleiner gleich 0 ist, entferne 1 SpeedModifier und setze die activeTimeLeft auf activeDuration
    /// 
    /// Wenn der Cooldown größer als 0 ist, verringere ihn um 1
    ///     -> Wenn der Cooldown kleiner gleich 0 ist, erhöhe chargeCount, falls chargeCount unter maxCharges ist.
    ///
    /// Falls chargeCount <= maxCharges, setze cooldownDuration auf cooldownTime
    /// </summary>
    /// <param name="entitie"></param>
    public void OnTick(IEntitie entitie)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        Debug.Log("TICK PACE!");
        // Reduziere die aktive Zeit und entferne ggf. den SpeedModifier
        if (activeTime <= 0)
        {
            // Entferne den aktuellen SpeedModifier, falls die aktive Zeit abgelaufen ist
            playerStats.MovementSpeed.RemoveModifier(speedModifier);

            // Setze die aktive Zeit auf activeDuration
            activeTime = chargeDuration;
        }

        // Reduziere die cooldownDuration und füge ggf. eine Aufladung hinzu
        cooldownDuration -= 1f;
        if (cooldownDuration <= 0)
        {
            if (chargeCount < maxCharges)
            {
                chargeCount++;
                paceUI.UpdateChargeUI(chargeCount);
            }

            // Setze die cooldownDuration auf cooldownTime, falls die maximale Anzahl an Aufladungen noch nicht erreicht wurde
            if (chargeCount < maxCharges)
            {
                cooldownDuration = cooldownTime;
            }
        }
    }

    public override void OnTickSpec1(IEntitie entitie)
    {
        OnTick(entitie);
    }

    public override void OnTickSpec2(IEntitie entitie)
    {
        OnTick(entitie);
    }

    public override void OnTickSpec3(IEntitie entitie)
    {
        OnTick(entitie);
    }

    public override void OnCooldownSpec1(IEntitie entitie)
    {
        EndActiveEffect();
    }

    public override void OnCooldownSpec2(IEntitie entitie)
    {
        throw new System.NotImplementedException();
    }

    public override void OnCooldownSpec3(IEntitie entitie)
    {
        throw new System.NotImplementedException();
    }

    public override void ApplySpec1Bonus(Talent t)
    {

    }

    public override void ApplySpec2Bonus(Talent t)
    {
        throw new System.NotImplementedException();
    }

    public override void ApplySpec3Bonus(Talent t)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}