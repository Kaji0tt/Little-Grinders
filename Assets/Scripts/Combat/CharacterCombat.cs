using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

//Finite-State-Machine
public enum CombatState
{
    Idle,
    Attacking,
    ComboWindow,
    Casting,
    Cooldown
}

public class CharacterCombat : MonoBehaviour
{
    #region Implementation - Combat Stance & Buffs

    //Ziel-Vector für Raycast der Maus
    private Vector3 targetDirection;

    //Combat-Stance Timer, um Attack entsprechend der Attacksped abzurufen
    private float combatStanceTime;



    //Füge eine Liste von Buffs hinzu, welche Auswirkungen auf die PlayerStats besitzen.
    //GGf. werden diese vom GO behandelt und nicht von der IsometricPlayer Klasse.
    //public List<Buff> playerBuffs = new List<Buff>(); 

    private CombatState currentState = CombatState.Idle;


    /// <summary>
    /// Combo System: 
    /// current index = an welcher Stelle (AttackStep) sich der spieler innerhalb der WeaponCombo befindet.
    /// </summary>
    private int currentComboIndex = 0;

    /// <summary>
    /// Combo System: 
    /// lastAttackTime = Variabel, die beim Angriff den Zeitpunkt des ComboStarts speichert
    /// </summary>
    private float lastAttackTime;

    /// <summary>
    /// Combo System:
    /// Zeit die vergehen darf, bevor die Combo abgebrochen wird.
    /// </summary>
    private float comboResetTime = 1.2f;

    private WeaponCombo currentCombo => equippedWeapon?.weaponCombo;

    private ItemInstance equippedWeapon =>
        PlayerManager.instance.player.equippedItems
            .FirstOrDefault(i => i.itemType == ItemType.Weapon);

    #endregion


    #region Slow Modifiers
    private StatModifier attackSlow = new StatModifier(-0.5f, StatModType.PercentMult);

    private StatModifier castSlow = new StatModifier(0.2f, StatModType.PercentMult);

    private float slowDuration = 0.5f;
    private float slowTimer = 0f;
    private float currentSlowValue = 0f;
    private bool isSlowing = false;

    #endregion

    private PlayerStats playerStats;

    private IsometricRenderer isoRenderer;

    //EnemyStats enemyStats;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        isoRenderer = GetComponent<IsometricRenderer>();

        //isoRenderer.SetWeaponDirection(DirectionCollider.instance.dirVector, weaponRootAnimator);
    }

    private void FixedUpdate()
    {
        PlayerCombatStance();
    }

    void PlayerCombatStance()
    {
        switch (currentState)
        {
            case CombatState.Idle:
                if (Input.GetKey(KeyCode.Mouse0) && !IsMouseOverUIWithIgnores())
                {
                    combatStanceTime = 1f / playerStats.AttackSpeed.Value;
                    Attack();

                    // Setze Animationsgeschwindigkeit auf AttackSpeed
                    isoRenderer.weaponAnimator.speed = playerStats.AttackSpeed.Value;

                    currentState = CombatState.Attacking;
                }
                break;

            case CombatState.Attacking:
                slowDuration = 0.5f / playerStats.AttackSpeed.Value; // Dynamischer Slow je nach AttackSpeed
                slowTimer = slowDuration;
                currentSlowValue = -0.5f;
                isSlowing = true;

                //weaponAnimator.SetBool("isAttacking", true);
                //isoRenderer.ToggleActionState(true);
                currentState = CombatState.ComboWindow;
                break;

            case CombatState.ComboWindow:
                combatStanceTime -= Time.deltaTime;

                if (isSlowing)
                {
                    // Timer skaliert mit AttackSpeed (langsamer Timer bei niedrigem AS)
                    slowTimer -= Time.deltaTime;
                    float t = Mathf.Clamp01(slowTimer / slowDuration); // 1 → 0
                    float newValue = Mathf.Lerp(0f, currentSlowValue, t);

                    // Modifier neu setzen
                    playerStats.MovementSpeed.RemoveModifier(attackSlow);
                    attackSlow = new StatModifier(newValue, StatModType.PercentMult);
                    playerStats.MovementSpeed.AddModifier(attackSlow);

                    if (t <= 0)
                        isSlowing = false;
                }

                if (combatStanceTime <= 0)
                {
                    currentState = CombatState.Cooldown;
                }
                break;

            case CombatState.Cooldown:
                // Alles zurücksetzen
                playerStats.MovementSpeed.RemoveModifier(attackSlow);
                //weaponAnimator.SetBool("isAttacking", false);
                isoRenderer.weaponAnimator.speed = 1.0f; // Reset der Animator-Geschwindigkeit
                //isoRenderer.ToggleActionState(false);
                currentState = CombatState.Idle;
                break;

            case CombatState.Casting:
                // Platzhalter für zukünftige Spell-Casting-Logik
                break;
        }
    }




    void Attack()
    {
        if (currentCombo == null || currentCombo.comboSteps.Count == 0)
        {
            Debug.LogWarning("Keine Combo definiert oder keine Schritte enthalten.");
            return;
        }

        if (Time.time - lastAttackTime > comboResetTime)
        {
            currentComboIndex = 0;
        }

        if (currentComboIndex >= currentCombo.comboSteps.Count)
        {
            currentComboIndex = 0;
        }

        if (currentTarget != null)
        {
            StartCoroutine(MoveTowardsTarget(currentTarget.transform.position, 0.1f, 0.3f));
        }

        AttackStep currentStep = currentCombo.comboSteps[currentComboIndex];

        isoRenderer.PlayWeaponAttack(currentStep.animationClip, isoRenderer.weaponAnimator);
        isoRenderer.weaponAnimator.speed = playerStats.AttackSpeed.Value;

        float delay = 1f / playerStats.AttackSpeed.Value * currentStep.timeToNextAttack;

        lastAttackTime = Time.time;
        currentComboIndex++;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound("Attack");
        }

        StartCoroutine(DelayedHit(delay, currentStep));
    }

    IEnumerator MoveTowardsTarget(Vector3 targetPos, float moveDistance, float duration)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction * moveDistance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }

    // Coroutine, die Schaden und Sound nach einer gewissen Verzögerung ausführt
    IEnumerator DelayedHit(float delay, AttackStep currentStep)
    {
        yield return new WaitForSeconds(delay);

        float baseDamage = playerStats.AttackPower.Value;

        foreach (EnemyController enemy in DirectionCollider.instance.collidingEnemyControllers)
        {
            if (enemy != null && !enemy.mobStats.isDead)
            {
                bool isCrit = Random.value < playerStats.CriticalChance.Value;
                float finalDamage = CalculateFinalDamage(baseDamage, currentStep.damageMultiplier, isCrit, enemy.mobStats);
                enemy.TakeDamage(finalDamage, playerStats.Range, isCrit);
            }
        }

        GameEvents.Instance.PlayerHasAttacked(baseDamage * currentStep.damageMultiplier);
        SpawnAttackVFX();
    }

    /// <summary>
    /// Calculates the final damage dealt to a target, factoring in base damage, multipliers, critical hits, and target armor.
    /// </summary>
    /// <param name="baseDamage">The base damage value before modifiers.</param>
    /// <param name="damageMultiplier">The multiplier for the current attack step.</param>
    /// <param name="isCrit">Whether the attack is a critical hit.</param>
    /// <param name="mobStats">The target's stats, used for armor reduction.</param>
    float CalculateFinalDamage(float damage, float damageMultiplier, bool isCrit, MobStats mobStats)
    {   
        // Berechne den Schaden basierend auf dem Multiplikator des AttackSteps
        damage *= damageMultiplier;

        //Prüfe ob der resultierende Schaden als kritischer Treffer zählt
        if (isCrit)
        {
            damage *= playerStats.CriticalDamage.Value; // Critical Damage Multiplier
        }

        // Wenn MobStats nicht null ist, wende Rüstung an
        // und berechne den Schaden entsprechend der Rüstungsformel.
        if (mobStats != null)
        {
            // Apply armor as a percentage reduction: damage * 100 / (100 + armor)
            float armor = Mathf.Max(0f, mobStats.Armor.Value);
            damage *= 100f / (100f + armor);
            damage = Mathf.Max(damage, 1f); // Minimum 1 Schaden
        }
        // Debug-Ausgabe für den Schaden
        //Debug.Log($"Calculated Damage: {damage} (Base: {damage}, Multiplier: {damageMultiplier}, Crit: {isCrit}, Armor: {mobStats?.Armor.Value})");
        return damage;
    }

    void SpawnAttackVFX()
    {
        // TODO: Ersetze durch echten Partikeleffekt oder Slow-FX
        // Beispiel: Instantiate(slowEffectPrefab, transform.position, Quaternion.identity);
    }

    private bool IsMouseOverUIWithIgnores() //C @CodeMonkey
    {
        //Erstelle eine lokale Variabel von der Position der Maus
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        //Erstelle eine Liste aus Raycast-Daten
        List<RaycastResult> raycastResultList = new List<RaycastResult>();

        //Erstelle Raycasts von der Position der Maus und speichere ihre Ergebnisse in der Liste
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);

        //Filter die Liste auf vom Raycast getroffene Objekte und entferne jene, welche das ClickThrough Skript besitzen.
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject.GetComponent<ClickThrough>() != null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }

        return raycastResultList.Count > 0;
    }

    /*
    public void AttackPlayer(IsometricPlayer playerStats)
    {
        playerStats.TakeDamage(isometricPlayer.AttackPower.Value);
    }
    */
    //Abfrage, ob Spieler in LOS zum Geschoss / Spell ist.
    public bool InLineOfSight()
    {

        //Generiere einen Raycast Array, der all Collider abspeichert, durch die er geht - der Rayc wird von der Camera zur Mouse-Position im 3D Raum gecasted.
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        //Scanne die getroffenen Collider, nach dem entsprechenden Punkt im Boden
        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            if (hit.transform.tag == "Floor")
            {
                //Definiere den Ziel-Vector im Verhältnis zur Spieler Position
                Vector3 targetDirection = (hit.point - transform.position);
            }
        }

        //Falls sich der Direction-Collider vom Spieler, zwischen dem Spieler und dem Ziel-Vector befinden, return true, else false.
        RaycastHit dirCollider;
        int layerMask = 1 << 8;
        if (Physics.Raycast(transform.position, targetDirection, out dirCollider, Mathf.Infinity, layerMask))
            return true;
        else
            return false;

    }


    public EnemyController currentTarget {get; private set;}

    public void SetCurrentTarget(EnemyController target)
    {
        currentTarget = target;
    }

    // ---- RANGED ATTACK ----- Not implemented yet.
    void RangedAttack(Vector3 worldPos)
    {
        //Derzeit benutzen wir das für Fernkampf. Schau hier: https://youtu.be/wntKVHVwXnc?t=642 für Infos bzgl. Spell Index
        /* --Wird wieder enabled, sobald die Skills hinzugefügt wurden--
        float dist = Vector3.Distance(worldPos, transform.position);
        if (dist <= playerStats.Range)
        {
            Instantiate(skillPrefab[0], transform.position, Quaternion.identity);
        }
        */

    }


}
