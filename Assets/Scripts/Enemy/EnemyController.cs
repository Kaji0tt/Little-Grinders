using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    //Create Variabel for the Player (through PlayerManager Singleton)
    //Transform character_transform;

    //Create Reference to currents GO Agent
    [Header("Welt Daten")]
    public NavMeshAgent myNavMeshAgent { get; private set; }

    //Create Reference to currents GO isoRenderer
    public IsometricRenderer myIsoRenderer { get; private set; }

    private EntitieState myEntitieState;

    public Transform Player => PlayerManager.instance.player.transform;

    //public Transform myTarget;


    //[Header("Interface Referenzen")]
    public GameObject hpBar { get; private set; }
    public Canvas combatCanvas { get; private set; }

    public Slider enemyHpSlider { get; private set; }


    /// <summary>
    /// Bullet Types - finde eine bessere Methode die entsprechenden Scripte abzurufen.
    /// </summary>
    //private Steinwurf steinwurf;


    [Header("Combat Referenzen")]
    public MobStats mobStats { get; private set; }
    public float attackRange, aggroRange;
    private float maxHpForUI;
    public bool isDead = false;
    //public int level;





    //public LootTable lootTable;


    ///----Position/Ziel/Direction----
    ///Controller Variables
    Vector3 myForward, myRight;

    /// <summary>
    /// Intervalle für den Zufälligkeitswert von zu wählenenden Richtungen
    /// </summary>
    private float directionTimer = 0f;
    private readonly float directionInterval = 0.5f;
    private Vector3 currentDirection = Vector3.zero;


    //Erstelle einen Kreis aus der Aggro-Range für den Editor Modus
    void OnDrawGizmosSelected()
    {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

    void Start()
    {
        mobStats = GetComponent<MobStats>();

        //UI Display
        TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        SetLocalDirectionVariables();

        AddEssentialComponents();

        //Recalculate BaseStats in dependency of Map-Level
        maxHpForUI = mobStats.Hp.Value;

        TransitionTo(new IdleState(this));
    }
    void Update()
    {

        myEntitieState?.Update();

        if(!mobStats.isDead)
        {
            CalculateHPCanvas();
            myIsoRenderer.SetFacingDirection(TargetDirection());
        }
        else
            hpBar.GetComponent<CanvasGroup>().alpha = 0;

    }

    public void SetLocalDirectionVariables()
    {
        //Adapt the Isometric-Camera angles
        myForward = Camera.main.transform.forward;
        myForward.y = 0;
        myForward = Vector3.Normalize(myForward);
        myRight = Quaternion.Euler(new Vector3(0, 90, 0)) * myForward;
    }

    public virtual void AddEssentialComponents()
    {
        //Get Renderer Component.
        if (myIsoRenderer == null)
            myIsoRenderer = GetComponentInChildren<IsometricRenderer>();

        //Add the MobCamScript.
        if (gameObject.GetComponent<MobsCamScript>() == null)
            gameObject.AddComponent<MobsCamScript>();

        if (myNavMeshAgent == null)
            myNavMeshAgent = GetComponent<NavMeshAgent>();

        Debug.Log(gameObject.name);

        // Automatische Referenzierung der UI-Elemente
        if (hpBar == null)
            hpBar = transform.GetChild(2).gameObject;

        if (enemyHpSlider == null && hpBar != null)
            enemyHpSlider = hpBar.GetComponentInChildren<Slider>();
    }

    #region Action StateMachine
    public void TransitionTo(EntitieState newState)
    {   
        if(!mobStats.isDead)
        {
            myEntitieState?.Exit();
            myEntitieState = newState;
            myEntitieState.Enter();
        }
    }

    public void PerformAttack()
    {


        // TODO: Schadenslogik, Cooldowns etc.
        PlayerStats playerStats = PlayerManager.instance.player.transform.GetComponent<PlayerStats>();


        if (playerStats != null)
        {

            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieAttack1", "Mob_ZombieAttack2", "Mob_ZombieAttack3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

                //Play a Sound at random.
                AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);

            //Füge dem Spieler Schaden entsprechend der AttackPower hinzu. int Range derzeit irrelevant (0)
            playerStats.TakeDamage(mobStats.AttackPower.Value, 0);

            //Calle Außerdem das GameEvent, dass der Spieler angegriffen wurde.
            //GameEvents.current.PlayerWasAttacked(mobStats, mobStats.AttackPower.Value);
            //myIsoRenderer.PlayAttack();

            //Der Versuch einen AttackSpeed zu integrieren - je kleiner der mobStats.AttackSpeed.Value, desto mehr Zeit zwischen den Angriffen.
            //mobStats.attackCD = 1f / mobStats.AttackSpeed.Value;
        }

    }

    public bool IsPlayerInAttackRange()
    {
        if (Player == null) return false;
        //myTarget
        return Vector3.Distance(transform.position, Player.position) <= attackRange;
    }

    public void StopMoving()
    {
        if (myNavMeshAgent != null && myNavMeshAgent.isActiveAndEnabled)
        {
            myNavMeshAgent.SetDestination(transform.position);
        }

    }

    //public void MoveToTarget()
    public void MoveToPlayer()
    {
        if (myNavMeshAgent == null)
            myNavMeshAgent = GetComponent<NavMeshAgent>();

        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0f)
        {
            //Die Laufrichtung, welche in die Wahrschienlichkeitsrechnung als Hauptziel vergeben wird
            Vector3 preferredDirection = (Player.position - transform.position).normalized;

            // Gewichtet anhand von Wahrscheinlichkeiten Richtung Spieler laufen
            currentDirection = MovementHelper.GetWeightedDirection(preferredDirection);

            // Setze das Intervall für die Richtungsdefinition zurück
            directionTimer = directionInterval;
        }

        // Laufziel berechnen – z. B. 2 Meter in diese Richtung
        Vector3 destination = transform.position + currentDirection * 2f;

        if(!mobStats.isDead)
        myNavMeshAgent.SetDestination(destination);
        //Vector3 toPlayer = PlayerManager.instance.player.transform.position - transform.position;


    }
    #endregion

    public float TargetDistance()
	{
		float distance = Vector3.Distance(Player.position, transform.position);
        return distance;
	}
	
    public Vector2 TargetDirection()
    {
        Vector3 Direction = PlayerManager.instance.player.transform.position - transform.position;

        Vector2 outputVector = new Vector2(Direction.x * -1, Direction.z);

        outputVector = Vector2.ClampMagnitude(outputVector, 1);

        return outputVector;
    }

    private void CalculateHPCanvas()
    {

        //Re-Calculate maxHp - this seems necessary, since inheriting Classes do not seem to run EnemyContrller Start Functions to initialize certain values.
        if (maxHpForUI == 0)
        {
                mobStats = GetComponent<MobStats>();
                maxHpForUI = mobStats.Hp.Value;
        }

        enemyHpSlider.value = mobStats.Hp.Value / maxHpForUI;

        if (mobStats.Hp.Value < maxHpForUI)
        {
            if (!mobStats.isDead)
                hpBar.GetComponent<CanvasGroup>().alpha = 1;

            if (Input.GetKey(KeyCode.LeftShift) && !mobStats.isDead)  //Sollte am Ende auf KeyCode.LeftAlt geändert werden.
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = Mathf.RoundToInt(mobStats.Hp.Value) + "/" + Mathf.RoundToInt(maxHpForUI);
                    statText[0].text = mobStats.level.ToString();
                }
            }
            else if(!mobStats.isDead)
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = " ";
                    statText[0].text = " ";
                }

            }

        }


        if (mobStats.Hp.Value <= 0)
        {


            if(!mobStats.isDead)
            {
                //StopMoving();
                TransitionTo(new DeadState(this));
            }

            //StopMoving();
        }

    }


    public void TakeDamage(float incoming_damage, int range_radius_ofDMG)
    {

        if (TargetDistance() <= range_radius_ofDMG)
        {
            incoming_damage = 10 * (incoming_damage * incoming_damage) / (mobStats.Armor.Value + (10 * incoming_damage));

            incoming_damage = Mathf.Clamp(incoming_damage, 1, int.MaxValue);

            //Schadesberechnung
            mobStats.Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));

            GameEvents.Instance.EnemyWasAttacked(incoming_damage, transform);

            // Knockback
            if(!mobStats.isDead)
            ApplyKnockback();

            //Setze den Entitie State auf "Hit"
            // Nur Hit-Animation, wenn NICHT im Angriff
            if (!(myEntitieState is HitState) && !isDead)
            {
                TransitionTo(new HitState(this));
            }

            //pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }
    }


    //Take Direct Damage ignoriert die Rüstungswerte der Entitie - besonders relevant für AP Schaden.
    public virtual void TakeDirectDamage(float incoming_damage, float range_radius_ofDMG)
    {
        float player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range_radius_ofDMG)
        {

            mobStats.Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));

            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

                //Play a Sound at random.
                AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);


            //Setze den Entitie State auf "Hit"
            //TransitionTo(new HitState(this));
            myIsoRenderer.Play(AnimationState.Hit);

            //pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.

        }
    }

    private void ApplyKnockback()
    {
        //if (DirectionCollider.instance == null) return;

        Vector3 knockbackDirection = (transform.position - PlayerManager.instance.player.transform.position).normalized;

        // Knockback-Distanz
        float knockbackDistance = 0.2f;

        // Stelle sicher, dass wir nur horizontal verschieben
        knockbackDirection.y = 0;

        // Berechne neues Ziel
        Vector3 targetPosition = transform.position + knockbackDirection * knockbackDistance;

        // Bewegung direkt setzen oder animieren
        StartCoroutine(KnockbackRoutine(targetPosition, 0.3f));
    }

    private IEnumerator KnockbackRoutine(Vector3 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        // Optionale Deaktivierung der NavMesh-Steuerung
        //myNavMeshAgent.SetDestination(transform.position);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        // NavMeshAgent reaktivieren
        //myNavMeshAgent.enabled = true;
    }

    //Überarbeitungswürdig. Soll schließlich eine Abfrage für Collision mit sämtlichen Projektilen ergeben.
    public virtual void OnTriggerEnter(Collider collider)
    {
        //18.09.22 Die Projektile erhalten derzeit ihre eigene Klasse because logical Reasons.
        if (collider.gameObject.tag == "Bullet") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {

            /*
            //Derzeit müssten die entsprechenden Bullet-Scripts hier abgefragt werden, dats stupid.
            Steinwurf_Bullet steinwurf_bullet = collider.GetComponent<Steinwurf_Bullet>();

            TakeDirectDamage(steinwurf_bullet.steinwurf.damage, steinwurf_bullet.steinwurf.range); // <- Die Bullets werden endlos fliegen können, jedoch erst ab spell.range schaden machen.  
            */

        }

        if (collider.gameObject.tag == "DirCollider")
        {
            collider.gameObject.GetComponent<DirectionCollider>().collidingEnemyControllers.Add(this);
        }
    }

    public virtual void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "DirCollider")
        {
            collider.gameObject.GetComponent<DirectionCollider>().collidingEnemyControllers.Remove(this);
        }
    }

}


//Mob Calculation. From my understanding, this is currently handled somewhere else.

/*
private void CalculateMobStats()
{
    //Alles noch nicht durchdacht, für pre alpha 0.3 solls reichen.
    if (GlobalMap.instance != null)
    {
        mobStats.level = mobStats.level + GlobalMap.instance.currentMap.mapLevel;

        if (mobStats.level > GlobalMap.instance.currentMap.mapLevel)
        {
            mobStats.Hp.AddModifier(new StatModifier(mobStats.Hp.Value + (GlobalMap.instance.currentMap.mapLevel * 10 / 2), StatModType.Flat));
            //Hp.AddModifier(new StatModifier(Hp.Value + (GlobalMap.instance.currentMap.mapLevel * 0.01f), StatModType.PercentMult));
            mobStats.Armor.AddModifier(new StatModifier((PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() / (GlobalMap.instance.currentMap.mapLevel + 1)), StatModType.Flat));
            mobStats.AttackPower.AddModifier(new StatModifier(mobStats.AttackPower.Value + (GlobalMap.instance.currentMap.mapLevel * 2), StatModType.Flat));

            //Print do double-check the values of increasing modifiers.
            //print("modifiers of mobs should have been added, resulting in:" + Hp.Value + " for Hp on " + gameObject.name + ". AttackPower: " + AttackPower.Value);
        }



        experience += mobStats.Get_xp() + GlobalMap.instance.currentMap.mapLevel * 10;

    }

}
*/