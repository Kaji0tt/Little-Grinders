using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

//Diese Klasse sollte eine Parent-Klasse für alle Mob-Typen werden. Das heißt, alle Variabeln, welche hier angegeben werden,
//müssen allübergreifend sein.

//Tochter Klassen könnten sein: E_Ranged, E_Caster, E_Sprinter, E_Summoner etc.
//Diese Differenzieren sich in:
//Override Attack
//Override Pulled
//Override TakeDamage

public class EnemyController : MonoBehaviour
{
    //Create Variabel for the Player (through PlayerManager Singleton)
    //Transform character_transform;

    //Create Reference to currents GO Agent
    NavMeshAgent navMeshAgent;

    //Create Reference to currents GO isoRenderer
    public IsometricRenderer isoRenderer;

    //Create Reference to Animator, for the case its not Animated by the IsometricRenderer, but by IK
    private EnemyAnimator enemyAnimator;

    //For the if Statement of above condition, wether its isometricRendered or rendered by IK
    public bool ikAnimated;


    ///----Combat Variables-----
    /// Gameobject's to depict the enemys status'
    public GameObject hpBar;
    public Slider enemyHpSlider;

    /// <summary>
    /// Bullet Types - finde eine bessere Methode die entsprechenden Scripte abzurufen.
    /// </summary>
    //private Steinwurf steinwurf;


    ///-----Stat Stuff-----
    ///Reference to Stats used for Combat
    //[Space]
    //public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;
    public MobStats mobStats { get; private set; }

    //public int level;


    private float maxHp;
    public int experience { get; private set; }
    public float attackRange, aggroRange;
    //private PlayerStats playerStats;
    public LootTable lootTable;


    ///----Position/Ziel/Direction----
    ///Controller Variables
    Vector3 forward, right;
    public virtual float player_distance { get; private set; }
    Vector2 inputVector;
    public bool pulled;

    private bool stun;
    private float stunTime;

    //Erstelle einen Kreis aus der Aggro-Range für den Editor Modus
    void OnDrawGizmosSelected()
    {

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);


    }

    void Start()
    {
        mobStats = GetComponent<MobStats>();
        experience = mobStats.Get_xp();
        //IK-Animated Objects currently have theire Animators connected in GO's in dependecy of their position in relation to player.position
        //the according GO (e.g. looking down, looking up) is activated. For this, we need to find the according animators of the activated GO's
        enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        //UI Display
        TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();
        maxHp = mobStats.Hp.Value;



        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.


        //Adapt the Isometric-Camera angles
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        //Ask, if this is an IK-Animated enemy
        if (ikAnimated == false)
            isoRenderer = GetComponentInChildren<IsometricRenderer>();

        //Recalculate BaseStats in dependency of Map-Level
        CalculateMobStats();
    }

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

            maxHp = mobStats.Hp.Value;

            experience += mobStats.Get_xp() + GlobalMap.instance.currentMap.mapLevel * 10;

        }

    }

    void Update()
    {

        if(!stun)
        {            
            CheckForAggro();      
        }
        else if (stun)
        {
            navMeshAgent.SetDestination(transform.position);
            isoRenderer.inCombatStance = false;
            stunTime -= Time.deltaTime;
            if (stunTime <= 0)
                stun = false;
        }

        CalculateHPCanvas();
    }

    public void StunEnemy(float duration)
    {
        stun = true;
        stunTime = duration;

    }

    public virtual void CheckForAggro()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        //Verarbeitung für ikAnimated Enemys
        //Presumably the ikAnimated Enemys are not going to be supported on the long run.
        inputVector.x = PlayerManager.instance.player.transform.position.x - transform.position.x;
        inputVector.y = PlayerManager.instance.player.transform.position.z - transform.position.z;
        if (ikAnimated == true)
            enemyAnimator.AnimateMe(inputVector, player_distance, attackRange, aggroRange);


        else
        {
            if (player_distance <= aggroRange || pulled)
            {
                //Setze die Agent.StoppingDistance gleich mit der AttackRange des Mobs
                //If the StoppingDistance is smaller then the attackRange, faster mobs may attack while the player is running away
                navMeshAgent.stoppingDistance = attackRange / 2;

                //Setze das Target des Mobs und starte "chasing"
                SetDestination();

                //Falls die Spieler-Distanz kleiner ist, als die Attack Range
                if (player_distance < attackRange && navMeshAgent.velocity == Vector3.zero)
                {
                    //Setze "inCombatStance" des Iso-Renderers dieser Klasse auf true
                    isoRenderer.inCombatStance = true;

                    //Attack the Target
                    Attack();


                }

                else
                    //Setze Combat-Stance zurück
                    isoRenderer.inCombatStance = false;
            }

            else
            {
                navMeshAgent.SetDestination(transform.position);
                isoRenderer.SetNPCDirection(new Vector2(0, 0));
            }

        }
    }



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
        if (maxHp == 0)
        {
                mobStats = GetComponent<MobStats>();
                maxHp = mobStats.Hp.Value;
        }

        enemyHpSlider.value = mobStats.Hp.Value / maxHp;

        if (mobStats.Hp.Value < maxHp)
        {

            hpBar.SetActive(true);

            if (Input.GetKey(KeyCode.LeftShift))  //Sollte am Ende auf KeyCode.LeftAlt geändert werden.
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = Mathf.RoundToInt(mobStats.Hp.Value) + "/" + Mathf.RoundToInt(maxHp);
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

    public void SetDestination()
    {
        if (PlayerManager.instance.player.transform != null)
        {

            navMeshAgent.SetDestination(PlayerManager.instance.player.transform.position);

            // Hier wird die "Blickrichtung" bestimmt, welche sich an dem Charakter orientiert.
            Vector3 Direction = PlayerManager.instance.player.transform.position - transform.position;

            Vector2 inputVector = new Vector2(Direction.x * -1, Direction.z);

            inputVector = Vector2.ClampMagnitude(inputVector, 1);

            if (ikAnimated == false)
                isoRenderer.SetNPCDirection(inputVector);

            //irgendwo hier ist noch n kleiner fehler


        }
    }

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

                //Füge dem Spieler Schaden entsprechend der AttackPower hinzu.
                playerStats.TakeDamage(mobStats.AttackPower.Value);

                //Der Versuch einen AttackSpeed zu integrieren - je kleiner der mobStats.AttackSpeed.Value, desto mehr Zeit zwischen den Angriffen.
                mobStats.attackCD = 1f / mobStats.AttackSpeed.Value;

            }
        }
    }


    
    //Überarbeitungswürdig. Soll schließlich eine Abfrage für Collision mit sämtlichen Projektilen ergeben.
    public virtual void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bullet") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {
            //Derzeit müssten die entsprechenden Bullet-Scripts hier abgefragt werden, dats stupid.
            Steinwurf_Bullet steinwurf_bullet = collider.GetComponent<Steinwurf_Bullet>();

            TakeDirectDamage(steinwurf_bullet.steinwurf.damage, steinwurf_bullet.steinwurf.range); // <- Die Bullets werden endlos fliegen können, jedoch erst ab spell.range schaden machen.         

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




    public void TakeDamage(float incoming_damage, float range_radius_ofDMG)
    {

        player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range_radius_ofDMG)
        {
            incoming_damage = 10 * (incoming_damage * incoming_damage) / (mobStats.Armor.Value + (10 * incoming_damage));            // DMG & Armor als werte

            incoming_damage = Mathf.Clamp(incoming_damage, 1, int.MaxValue);

            mobStats.Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));

            
            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

            //Play a Sound at random.
            AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);
            

            pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }

        if (mobStats.Hp.Value <= 0)
            Die();
    }

    public virtual void TakeDirectDamage(float incoming_damage, float range_radius_ofDMG)
    {
        player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range_radius_ofDMG)
        {

            mobStats.Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));

            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

            //Play a Sound at random.
            AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);

            pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }

        if (mobStats.Hp.Value <= 0)
            Die();
    }


    public void Die()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.Set_xp(experience);

        if(mobStats.level >= 1)
            //Eine Funktion die in ABhängigkeit vom Moblevel, die ANzahl der Drops erhöhen soll. Hier wäre ein Lerp-Funktion sinnvoll.
            for(int i = 0; i <= mobStats.level/2; i++)
            {
                ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);
            }

        Destroy(this);

        Destroy(gameObject);
    }

}