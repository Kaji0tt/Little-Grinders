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

    public Transform myTarget;


    [Header("Interface Referenzen")]
    public GameObject hpBar;
    public Slider enemyHpSlider;

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





    public LootTable lootTable;


    ///----Position/Ziel/Direction----
    ///Controller Variables
    Vector3 myForward, myRight;


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

        CalculateHPCanvas();
        myEntitieState?.Update();

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
        if(myIsoRenderer == null)
        myIsoRenderer = gameObject.GetComponent<IsometricRenderer>();

        //Add the MobCamScript.
        gameObject.AddComponent<MobsCamScript>();
    }

    #region Action StateMachine
    public void TransitionTo(EntitieState newState)
    {
        myEntitieState?.Exit();
        myEntitieState = newState;
        myEntitieState.Enter();
    }

    public void PerformAttack()
    {
        if (myIsoRenderer != null)
        {
            myIsoRenderer.Play(AnimationState.Attack);
        }

        // TODO: Schadenslogik, Cooldowns etc.
    }

    public bool IsInAttackRange()
    {
        if (myTarget == null) return false;
        return Vector3.Distance(transform.position, myTarget.position) <= attackRange;
    }

    public void StopMoving()
    {
        myNavMeshAgent.SetDestination(transform.position);
    }

    public void MoveToTarget()
    {
        if (myTarget != null)
            myNavMeshAgent.SetDestination(myTarget.position);
    }
    #endregion

    public Vector2 CalculatePlayerDirection()
    {
        Vector3 Direction = PlayerManager.instance.player.transform.position - transform.position;

        Vector2 inputVector = new Vector2(Direction.x * -1, Direction.z);

        inputVector = Vector2.ClampMagnitude(inputVector, 1);

        return inputVector;
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

            hpBar.SetActive(true);

            if (Input.GetKey(KeyCode.LeftShift))  //Sollte am Ende auf KeyCode.LeftAlt geändert werden.
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = Mathf.RoundToInt(mobStats.Hp.Value) + "/" + Mathf.RoundToInt(maxHpForUI);
                    statText[0].text = mobStats.level.ToString();
                }
            }
            else
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = " ";
                    statText[0].text = " ";
                }

            }

        }


        if (mobStats.Hp.Value <= 0)
            Destroy(gameObject);
    }
    /*
    public virtual void Attack()
    {

        //Fetch the Playerstats of the player
        PlayerStats playerStats = PlayerManager.instance.player.transform.GetComponent<PlayerStats>();

        //Countdown for the Auto-Attack Cooldown
        mobStats.attackCD -= Time.deltaTime;
        if (playerStats != null)
        {
            if (mobStats.attackCD <= 0)
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
                myIsoRenderer.PlayAttack();

                //Der Versuch einen AttackSpeed zu integrieren - je kleiner der mobStats.AttackSpeed.Value, desto mehr Zeit zwischen den Angriffen.
                mobStats.attackCD = 1f / mobStats.AttackSpeed.Value;


            }
        }
    }
    */

    
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