using UnityEngine;

public class Phase : Ability
{
    private Collider playerCollider;
    private bool isActive = false;

    // Buff-Intensität, wird durch rarityScaling gesetzt
    private float moveSpeedMultiplier = 1f;
    private StatModifier phaseSpeedMod;

    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Beispiel: 1.0 = kein Bonus, 1.5 = +50% Movespeed
        moveSpeedMultiplier = rarityScaling;
        Debug.Log($"[Phase] rarityScaling angewendet: {rarityScaling}, moveSpeedMultiplier: {moveSpeedMultiplier}");
    }

    public override void UseBase(IEntitie entitie)
    {
        var player = PlayerManager.instance.playerStats;
        playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null)
            playerCollider.enabled = false;

        // StatModifier für MovementSpeed hinzufügen (PercentMult)
        phaseSpeedMod = new StatModifier(moveSpeedMultiplier, StatModType.PercentMult, this);
        player.MovementSpeed.AddModifier(phaseSpeedMod);

        isActive = true;
        Debug.Log("[Phase] Phasewalk aktiviert!");
    }

    protected override void OnUpdateAbility()
    {
        // Prüfe, ob der Effekt aktiv ist und eine andere Taste als WASD gedrückt wurde
        if (isActive && AnyNonMovementInput())
        {
            Debug.Log("[Phase] Phasewalk abgebrochen durch Aktion!");
            EndPhasewalk();
            state = AbilityState.Ready; // Sofort beenden
        }
    }

    public override void OnCooldown(IEntitie entitie)
    {
        EndPhasewalk();
        Debug.Log("[Phase] Phasewalk beendet, Cooldown startet.");
    }

    private void EndPhasewalk()
    {
        var player = PlayerManager.instance.playerStats;
        if (playerCollider != null)
            playerCollider.enabled = true;

        // StatModifier entfernen
        if (phaseSpeedMod != null)
            player.MovementSpeed.RemoveModifier(phaseSpeedMod);

        isActive = false;
        Debug.Log("[Phase] EndPhasewalk aufgerufen.");
    }

    // Prüft, ob eine andere Taste als WASD gedrückt wurde
    private bool AnyNonMovementInput()
    {
        // Passe das ggf. an deine Input-Logik an!
        return Input.anyKeyDown && !(
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D)
        );
    }

    public override void OnTick(IEntitie entitie)
    {
        // Nicht benötigt für Phasewalk
    }
}
