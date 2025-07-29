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
    private float comboWindowTime = 1.0f; // Zeit, in der ein Combo-Input akzeptiert wird



    //Füge eine Liste von Buffs hinzu, welche Auswirkungen auf die PlayerStats besitzen.
    //GGf. werden diese vom GO behandelt und nicht von der IsometricPlayer Klasse.
    //public List<Buff> playerBuffs = new List<Buff>(); 

    private CombatState currentState = CombatState.Idle;

    private int currentComboIndex = 0;
    private AttackStep currentAttackStep;

    private WeaponCombo currentCombo => equippedWeapon?.weaponCombo;

    private float comboCooldownTimer = 0f;

    private ItemInstance equippedWeapon => PlayerManager.GetEquippedWeapon();

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


    #region Input Buffering
    private Queue<float> bufferedInputs = new Queue<float>();
    private float attackStepTimer = 0f;
    private bool isInAttackStep = false;
    private float lastAutoProcessTime = 0f; // HIER als Instanzvariable
    private int MaxBufferedInputs => equippedWeapon?.weaponCombo.comboSteps.Count ?? 0;


    private void Update()
    {
        HandleAttackInputBuffering();
        HandleBufferClearOnOtherInput();
        PlayerCombatStance();
    }

    private void HandleAttackInputBuffering()
    {
        // Während Cooldown KEIN Input speichern!
        //if (currentState == CombatState.Cooldown)
        //    return;

        //if(IsMouseOverUIWithIgnores() && Input.GetKeyDown(KeyCode.Mouse0))
        //Debug.Log("[INPUT BUFFER] Input ignoriert, da über UI");

        if (Input.GetKeyDown(KeyCode.Mouse0) && !IsMouseOverUIWithIgnores())
            {
                if (bufferedInputs.Count < MaxBufferedInputs)
                {
                    bufferedInputs.Enqueue(Time.time);
                    //Debug.Log($"[INPUT BUFFER] Input #{bufferedInputs.Count} gepuffert (Zeit: {Time.time:F2}s) | Max: {MaxBufferedInputs}");
                }
                else
                {
                    //Debug.Log($"[INPUT BUFFER] Buffer voll! Input ignoriert (Max: {MaxBufferedInputs})");
                }
            }
    }

    private void HandleBufferClearOnOtherInput()
    {
        // Erlaube nur WASD und Mouse0, alles andere leert den Buffer
        if (Input.anyKeyDown)
        {
            bool isMovementKey =
                Input.GetKeyDown(KeyManager.MyInstance.Keybinds["UP"]) ||
                Input.GetKeyDown(KeyManager.MyInstance.Keybinds["DOWN"]) ||
                Input.GetKeyDown(KeyManager.MyInstance.Keybinds["LEFT"]) ||
                Input.GetKeyDown(KeyManager.MyInstance.Keybinds["RIGHT"]);
            bool isAttackKey = Input.GetKeyDown(KeyCode.Mouse0);

            if (!isMovementKey && !isAttackKey)
            {
                if (bufferedInputs.Count > 0)
                {
                    Debug.Log("[INPUT BUFFER] Buffer durch andere Taste geleert!");
                    bufferedInputs.Clear();
                }
            }
        }
    }
    #endregion

    void PlayerCombatStance()
    {
        // Debug-Ausgabe für aktuellen State und allgemeine Infos
        Debug.Log($"[STATE] Current: {currentState} | ComboIndex: {currentComboIndex} | Buffer: {bufferedInputs.Count} | isInAttackStep: {isInAttackStep} | AttackStepTimer: {attackStepTimer:F2}s | ComboWindowTime: {comboWindowTime:F2}s, comboCooldownTime: {comboCooldownTimer:F2}s ");

        switch (currentState)
        {
            case CombatState.Idle:
                isoRenderer.AnimateIdleWeapon();
                Debug.Log($"[IDLE] STATE - Buffer Count: {bufferedInputs.Count}");
                if (bufferedInputs.Count > 0)
                {
                    Debug.Log($"[IDLE] Verarbeite Input - Buffer vor Dequeue: {bufferedInputs.Count}");
                    bufferedInputs.Dequeue();
                    Debug.Log($"[IDLE] Buffer nach Dequeue: {bufferedInputs.Count}");
                    
                    // Setze AttackStep HIER
                    if (currentCombo != null && currentCombo.comboSteps.Count > currentComboIndex)
                    {
                        currentAttackStep = currentCombo.comboSteps[currentComboIndex];
                        Debug.Log($"[IDLE → ATTACKING] ComboIndex: {currentComboIndex} | AttackStep gesetzt | Buffer: {bufferedInputs.Count}");
                        currentState = CombatState.Attacking;
                    }
                    else
                    {
                        Debug.LogError($"[IDLE] ERROR - Combo/ComboSteps null oder Index außerhalb! Combo: {currentCombo} | Index: {currentComboIndex}");
                    }
                }
                else
                {
                    Debug.Log($"[IDLE] Kein Input im Buffer - warte...");
                }
                break;

            case CombatState.Attacking:
            
                Debug.Log($"[ATTACKING] STATE ENTRY - ComboIndex: {currentComboIndex}");

                //Setze isInAttackStep zu Beginn des CombatState.Attacking
                isInAttackStep = true;
                isoRenderer.weaponAnimator.SetBool("isAttacking", true);
                Debug.Log($"[ATTACKING] isInAttackStep gesetzt auf: {isInAttackStep}");

                // Setze Slow für den aktuellen AttackStep
                slowDuration = 0.5f / playerStats.AttackSpeed.Value;
                slowTimer = slowDuration;
                currentSlowValue = -0.5f;
                isSlowing = true;
                Debug.Log($"[ATTACKING] Slow gesetzt - Duration: {slowDuration:F2}s | Timer: {slowTimer:F2}s");

                // Führe den Angriff aus
                attackStepTimer = currentAttackStep.timeForAttackStep;
                Debug.Log($"[ATTACKING] AttackStepTimer gesetzt auf: {attackStepTimer:F2}s für Step: {currentAttackStep.animationClip.name}");
                
                PerformAttack();
                Debug.Log($"[ATTACKING] PerformAttack() ausgeführt");

                Debug.Log($"[ATTACKING → COMBOWINDOW] Reset ComboWindowsTime");
                comboWindowTime = 1.0f; // Setze ComboWindowTime zurück

                Debug.Log($"[ATTACKING → COMBOWINDOW] State-Wechsel");
                currentState = CombatState.ComboWindow;
                break;

            case CombatState.ComboWindow:
                Debug.Log($"[COMBOWINDOW] STATE - isInAttackStep: {isInAttackStep} | AttackStepTimer: {attackStepTimer:F2}s | ComboWindowTime: {comboWindowTime:F2}s | Buffer: {bufferedInputs.Count}");
                
                if (isInAttackStep)
                {
                    Debug.Log($"[COMBOWINDOW] IN ATTACKSTEP - Timer läuft ab...");
                    attackStepTimer -= Time.deltaTime;

                    if (attackStepTimer <= 0f)
                    {
                        Debug.Log($"[COMBOWINDOW] ATTACKSTEP BEENDET - Timer abgelaufen");
                        comboWindowTime = 1.0f;
                        Debug.Log($"[COMBOWINDOW] ComboWindowTime zurückgesetzt auf: {comboWindowTime:F2}s");

                        if (bufferedInputs.Count > 0)
                        {
                            Debug.Log($"[COMBOWINDOW] Input im Buffer gefunden - Buffer vor Dequeue: {bufferedInputs.Count}");
                            bufferedInputs.Dequeue();
                            currentComboIndex++;
                            Debug.Log($"[COMBOWINDOW] ComboIndex erhöht: {currentComboIndex - 1} → {currentComboIndex} | Buffer nach Dequeue: {bufferedInputs.Count}");

                            if (currentCombo != null && currentCombo.comboSteps.Count > currentComboIndex)
                            {
                                currentAttackStep = currentCombo.comboSteps[currentComboIndex];
                                Debug.Log($"[COMBOWINDOW → ATTACKING] Nächster AttackStep gesetzt: {currentAttackStep.animationClip.name} | ComboIndex: {currentComboIndex}");
                                currentState = CombatState.Attacking;
                            }
                            else
                            {
                                Debug.Log($"[COMBOWINDOW → COOLDOWN] Combo beendet - keine weiteren Steps | ComboIndex: {currentComboIndex} | Max Steps: {currentCombo?.comboSteps?.Count}");
                                currentState = CombatState.Cooldown;
                            }
                        }
                        else
                        {
                            Debug.Log($"[COMBOWINDOW] Kein Input im Buffer - AttackStep beendet ohne Folge-Input");
                        }
                        
                        isInAttackStep = false;
                        Debug.Log($"[COMBOWINDOW] isInAttackStep gesetzt auf: {isInAttackStep}");
                    }
                }
                else
                {
                    Debug.Log($"[COMBOWINDOW] NICHT IN ATTACKSTEP - Combo-Fenster läuft...");
                    comboWindowTime -= Time.deltaTime;

                    if (bufferedInputs.Count > 0)
                    {
                        Debug.Log($"[COMBOWINDOW] Input im Buffer während Combo-Fenster - LastAutoProcessTime: {lastAutoProcessTime:F2}s | Current Time: {Time.time:F2}s");
                        
                        if (Time.time - lastAutoProcessTime >= 0.1f)
                        {
                            Debug.Log($"[COMBOWINDOW] Auto-Process Delay abgelaufen - verarbeite Input");
                            Debug.Log($"[COMBOWINDOW] Buffer vor Dequeue: {bufferedInputs.Count}");
                            bufferedInputs.Dequeue();
                            lastAutoProcessTime = Time.time;
                            currentComboIndex++;
                            Debug.Log($"[COMBOWINDOW] ComboIndex erhöht: {currentComboIndex - 1} → {currentComboIndex} | Buffer nach Dequeue: {bufferedInputs.Count} | LastAutoProcessTime: {lastAutoProcessTime:F2}s");

                            if (currentCombo != null && currentCombo.comboSteps.Count > currentComboIndex)
                            {
                                currentAttackStep = currentCombo.comboSteps[currentComboIndex];
                                Debug.Log($"[COMBOWINDOW → ATTACKING] Nächster AttackStep gesetzt: {currentAttackStep.animationClip.name} | ComboIndex: {currentComboIndex}");
                                currentState = CombatState.Attacking;
                                break;
                            }
                            else
                            {
                                Debug.Log($"[COMBOWINDOW → COOLDOWN] Combo beendet - keine weiteren Steps | ComboIndex: {currentComboIndex} | Max Steps: {currentCombo?.comboSteps?.Count}");
                                currentState = CombatState.Cooldown;
                            }
                        }
                        else
                        {
                            Debug.Log($"[COMBOWINDOW] Auto-Process Delay noch aktiv - warte... (${(0.1f - (Time.time - lastAutoProcessTime)):F2}s verbleibend)");
                        }
                    }
                    else
                    {
                        Debug.Log($"[COMBOWINDOW] Kein Input im Buffer - Combo-Fenster läuft weiter...{comboWindowTime:F2}s verbleibend");
                    }
                }
                
                // Movement-Slow-Logic (unverändert)
                if (isSlowing)
                {
                    slowTimer -= Time.deltaTime;
                    float t = Mathf.Clamp01(slowTimer / slowDuration);
                    float newValue = Mathf.Lerp(0f, currentSlowValue, t);

                    playerStats.MovementSpeed.RemoveModifier(attackSlow);
                    attackSlow = new StatModifier(newValue, StatModType.PercentMult);
                    playerStats.MovementSpeed.AddModifier(attackSlow);

                    if (t <= 0)
                    {
                        isSlowing = false;
                        Debug.Log($"[COMBOWINDOW] Slowing beendet");
                    }
                }

                // Combo-Fenster abgelaufen
                if (!isInAttackStep && comboWindowTime <= 0)
                {
                    Debug.Log($"[COMBOWINDOW → COOLDOWN] Combo-Fenster abgelaufen ({comboWindowTime:F2}s) - lösche {bufferedInputs.Count} gepufferte Inputs");
                    bufferedInputs.Clear(); // Buffer komplett löschen
                    currentState = CombatState.Cooldown;
                }
                break;

            case CombatState.Cooldown:
                //isoRenderer.AnimateIdleWeapon();
                isoRenderer.weaponAnimator.SetBool("isAttacking", false);
                Debug.Log($"[COOLDOWN] STATE - Timer: {comboCooldownTimer:F2}s | Buffer: {bufferedInputs.Count}");
                
                playerStats.MovementSpeed.RemoveModifier(attackSlow);

                // Starte Cooldown-Timer, falls noch nicht gestartet
                if (comboCooldownTimer <= 0f)
                {
                    comboCooldownTimer = currentCombo?.comboCooldown ?? 1f;
                    Debug.Log($"[COOLDOWN] Timer gestartet für {comboCooldownTimer:F2}s");
                }
                else
                {
                    comboCooldownTimer -= Time.deltaTime;
                    Debug.Log($"[COOLDOWN] Timer läuft... {comboCooldownTimer:F2}s verbleibend");
                    
                    if (comboCooldownTimer <= 0f)
                    {
                        bufferedInputs.Clear(); // Buffer löschen
                        Debug.Log($"[COOLDOWN → IDLE] Cooldown beendet - ComboIndex zurückgesetzt: {currentComboIndex} → 0 | Buffer geleert");
                        currentComboIndex = 0; // ComboIndex zurücksetzen!
                        currentState = CombatState.Idle;
                        comboCooldownTimer = 0f;
                    }
                }
                break;

            case CombatState.Casting:
                Debug.Log($"[CASTING] STATE - Noch nicht implementiert");
                break;

            default:
                Debug.LogError($"[STATE] UNKNOWN STATE: {currentState}");
                break;
        }
    }

    void PerformAttack()
    {
        // NEU: LogScript-Ausgabe für AttackStep
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog($"AttackStep {currentComboIndex + 1}/{currentCombo.comboSteps.Count} gestartet!", 1.2f);
        }

        StartCoroutine(DashTowardsTarget(currentAttackStep));
        DealDamage(currentAttackStep);
        isoRenderer.PlayWeaponAttackWithDuration(currentAttackStep.animationClip, currentAttackStep.timeForAttackStep);
        PlaySound("Attack");
        //StartCoroutine(DelayedHit(delay, currentStep));
    }

    IEnumerator DashTowardsTarget(AttackStep currentAttack)
    {
        // Nur dashen wenn Target NICHT in Reichweite ist
        if (IsCurrentTargetInRange())
        {
            Debug.Log("[DASH] Target bereits in Reichweite - kein Dash nötig");
            yield break;
        }

        float dashDistance = currentAttack.dashDistance;
        
        // Reduziere Distanz, wenn kein Target gesetzt ist
        if (currentTarget == null)
        {
            dashDistance /= 3f;

        }

        // Nur horizontale Richtung verwenden
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Y-Komponente entfernen
        direction = direction.normalized;

        if (currentTarget == null)
        {
            Debug.LogWarning("[DASH] Kein aktuelles Ziel gesetzt - Dash abgebrochen");
            yield break; // Kein Ziel gesetzt, also abbrechen
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction * dashDistance;

        // Dash VFX
        SpawnDashTrail(startPos, endPos);
        
        // Dash-Parameter
        float dashDuration = 0.3f; // Dauer des Dashs in Sekunden
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            
            // Exponentieller Ease-Out Curve für natürlichen Dash-Effekt
            float t = elapsed / dashDuration;
            
            // Exponentieller Ease-Out: schnell starten, dann verlangsamen
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out
            
            // Alternative: Ease-In-Out für sanfteren Start und Ende
            // float easedT = t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

            // Berechne aktuelle Position
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, easedT);
            currentPos.y = startPos.y; // Y-Position konstant halten

            transform.position = currentPos;
            
            yield return null; // Warte einen Frame
        }

        // Stelle sicher, dass wir exakt an der Endposition sind
        Vector3 finalPos = endPos;
        finalPos.y = startPos.y;
        transform.position = finalPos;
    }

    void SpawnDashTrail(Vector3 start, Vector3 end)
    {
        // TODO: Spawne Dash-Partikel oder Trail-Renderer
        // Beispiel: Instantiate(dashTrailPrefab, start, Quaternion.LookRotation(end - start));
    }

    void DealDamage(AttackStep currentAttack)
    {
        float baseDamage = playerStats.AttackPower.Value;

        foreach (EnemyController enemy in DirectionCollider.instance.collidingEnemyControllers)
        {
            if (enemy != null && !enemy.mobStats.isDead)
            {
                bool isCrit = Random.value < playerStats.CriticalChance.Value;
                float finalDamage = CalculateFinalDamage(baseDamage, currentAttack.damageMultiplier, isCrit, enemy.mobStats);
                enemy.TakeDamage(finalDamage, playerStats.Range, isCrit);
            }
        }

        GameEvents.Instance.PlayerHasAttacked(baseDamage * currentAttack.damageMultiplier);
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

    #region Helper Functions
    
    private void PlaySound(string soundName)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound(soundName);
        }
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

    Vector3 targetPosition
    {
        get
        {
            if (currentTarget != null)
                return currentTarget.transform.position;
            else
                return DirectionCollider.instance.targetWorldPosition;
        }
    }

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
    #endregion

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
    
    #region Target Utilities

    /// <summary>
    /// Gibt alle gültigen Feinde in Reichweite zurück (lebend und nicht null)
    /// </summary>
    private List<EnemyController> GetValidEnemiesInRange()
    {
        List<EnemyController> validEnemies = new List<EnemyController>();

        foreach (EnemyController enemy in DirectionCollider.instance.collidingEnemyControllers)
        {
            if (enemy != null && !enemy.mobStats.isDead)
            {
                validEnemies.Add(enemy);
            }
        }

        return validEnemies;
    }

    /// <summary>
    /// Prüft ob das aktuelle Target in Reichweite ist
    /// </summary>
    private bool IsCurrentTargetInRange()
    {
        if (currentTarget == null) return false;
        
        return DirectionCollider.instance.collidingEnemyControllers.Contains(currentTarget) 
            && !currentTarget.mobStats.isDead;
    }

    /// <summary>
    /// Führt eine Aktion für jeden gültigen Feind in Reichweite aus
    /// </summary>
    private void ForEachEnemyInRange(System.Action<EnemyController> action)
    {
        foreach (EnemyController enemy in GetValidEnemiesInRange())
        {
            action(enemy);
        }
    }

    #endregion

}
