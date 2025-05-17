using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    [SerializeField]
    private Animator weaponAnimator;
    //Füge eine Liste von Buffs hinzu, welche Auswirkungen auf die PlayerStats besitzen.
    //GGf. werden diese vom GO behandelt und nicht von der IsometricPlayer Klasse.
    //public List<Buff> playerBuffs = new List<Buff>(); 

    private CombatState currentState = CombatState.Idle;

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

        isoRenderer.SetWeaponDirection(DirectionCollider.instance.dirVector, weaponAnimator);
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
                    weaponAnimator.speed = playerStats.AttackSpeed.Value;

                    currentState = CombatState.Attacking;
                }
                break;

            case CombatState.Attacking:
                slowDuration = 0.5f / playerStats.AttackSpeed.Value; // Dynamischer Slow je nach AttackSpeed
                slowTimer = slowDuration;
                currentSlowValue = -0.5f;
                isSlowing = true;

                weaponAnimator.SetBool("isAttacking", true);
                isoRenderer.ToggleActionState(true);
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
                weaponAnimator.SetBool("isAttacking", false);
                weaponAnimator.speed = 1.0f; // Reset der Animator-Geschwindigkeit
                isoRenderer.ToggleActionState(false);
                currentState = CombatState.Idle;
                break;

            case CombatState.Casting:
                // Platzhalter für zukünftige Spell-Casting-Logik
                break;
        }
    }




    void Attack()
    {

        // Prüfe, ob eine Waffe ausgerüstet ist (Range != 0 als Platzhalter)
        if (playerStats.Range != 0)
        {
            // Trigger die Angriffsanimation
            weaponAnimator.speed = playerStats.AttackSpeed.Value;
            weaponAnimator.SetBool("isAttacking", true);

            // Berechne Verzögerung basierend auf AttackSpeed (1 / Speed) * 0.5 für „Trefferzeitpunkt“ in der Mitte
            float delay = (1f / playerStats.AttackSpeed.Value) * 0.5f;

            // Starte Coroutine, um Schaden + Sound verzögert auszuführen
            StartCoroutine(DelayedHit(delay));
        }

    }

    // Coroutine, die Schaden und Sound nach einer gewissen Verzögerung ausführt
    IEnumerator DelayedHit(float delay)
    {
        // Warte die definierte Verzögerung (halbe Angriffszeit)
        yield return new WaitForSeconds(delay);

        // *** COLLISION + SCHADEN ***
        // Prüfe alle Gegner im Angriffsbereich und wende Schaden an
        foreach (EnemyController enemy in DirectionCollider.instance.collidingEnemyControllers)
        {
            // Vermeide NullReference, falls Gegner bereits besiegt wurden
            if (enemy != null)
            {
                enemy.GetComponent<MobStats>().TakeDamage(playerStats.AttackPower.Value, playerStats.Range);
                // OPTIONAL: Zeige Hit-Animation, Partikeleffekt etc.
                // Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
            }
        }

        // *** SOUND ***
        // Spiele zufälligen Angriffssound ab, sofern AudioManager vorhanden ist
        if (AudioManager.instance != null)
        {
            string[] attackSounds = new string[] { "Attack1", "Attack2", "Attack3", "Attack4", "Attack5", "Attack6" };

            AudioManager.instance.Play(attackSounds[Random.Range(0, attackSounds.Length)]);
        }

        // *** GAME-EVENT ***
        // Informiere das System, dass der Spieler angegriffen hat (z. B. für UI, XP, etc.)
        GameEvents.Instance.PlayerHasAttacked(playerStats.AttackPower.Value);

        // Visuelles Feedback vorbereiten (z. B. Partikeleffekt bei Slow, Treffereffekt etc.)
        SpawnAttackVFX();
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

    /*
    private void OnTriggerStay(Collider collider)
    {
        
        //Versuche einen Collider mit der Item-World Klasse zu finden
        ItemWorld itemWorld = collider.GetComponent<ItemWorld>();

        //Falls ein entsprechender Collider gefunden wurde und die pickKey Taste (Default Q) gedrück wurde

        //Hier ist noch ein Fehler - beziehen wir uns auf den UI-Manager Key, wird das Item doppel aufgesammelt.
        if (itemWorld != null && Input.GetKey(UI_Manager.instance.pickKey))
        //if (itemWorld != null && Input.GetKey(KeyCode.Q)) 
        {

            //Falls noch Platz im Inventar ist
            if(inventory.itemList.Count <= 14)
            {
                //Füge Item zum Inventar hinzu
                inventory.AddItem(itemWorld.GetItem());

                //Zerstöre den Collider
                itemWorld.DestroySelf();
            }

        }

    }
    */
}
