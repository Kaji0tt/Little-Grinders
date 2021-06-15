using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class EnemyController : MonoBehaviour
{
    //Create Variabel for the Player (through PlayerManager Singleton)
    Transform character_transform;

    //Create Reference to currents GO Agent
    NavMeshAgent navMeshAgent;

    //Create Reference to currents GO isoRenderer
    IsometricRenderer isoRenderer;

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
    [Space]
    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;

    public int level;

    private float attackCD = 0f;
    private float p_attackCD = 0f;
    private float maxHp;
    [Space]
    public int Experience;
    public float aggroRange = 5f, attackRange;
    //private PlayerStats playerStats;
    public LootTable lootTable;


    ///----Position/Ziel/Direction----
    ///Controller Variables
    Vector3 forward, right;
    private float player_distance;
    Vector2 inputVector;
    public bool pulled;

    //Erstelle einen Kreis aus der Aggro-Range für den Editor Modus
    void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

    void Start()
    {
        //IK-Animated Objects currently have theire Animators connected in GO's in dependecy of their position in relation to player.position
        //the according GO (e.g. looking down, looking up) is activated. For this, we need to find the according animators of the activated GO's
        enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        //UI Display
        TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();
        maxHp = Hp.Value;



        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        character_transform = PlayerManager.instance.player.transform;

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        if (ikAnimated == false)
        isoRenderer = GetComponentInChildren<IsometricRenderer>();
    }


    void Update()
    {


        navMeshAgent = GetComponent<NavMeshAgent>();

        player_distance = Vector3.Distance(character_transform.position, transform.position);



        inputVector.x = character_transform.transform.position.x - transform.position.x;
        inputVector.y = character_transform.transform.position.z - transform.position.z;
        if (ikAnimated == true)
            enemyAnimator.AnimateMe(inputVector, player_distance, attackRange, aggroRange);


        else
        {
            if (player_distance <= aggroRange || pulled)
            {
                //Setze die Agent.StoppingDistance gleich mit der AttackRange des Mobs
                navMeshAgent.stoppingDistance = attackRange / 2;

                //Setze das Target des Mobs und starte "chasing"
                SetDestination();

                //Falls die Spieler-Distanz kleiner ist, als die Attack Range
                if (player_distance <= navMeshAgent.stoppingDistance)
                {
                    //Setze "inCombatStance" des Iso-Renderers dieser Klasse auf true
                    isoRenderer.inCombatStance = true;

                    //Attack the Target
                    Attack();


                }

                else
                    isoRenderer.inCombatStance = false;
            }

            else
            {
                navMeshAgent.SetDestination(transform.position);
                isoRenderer.SetNPCDirection(new Vector2(0, 0));
            }

        }



        #region Hp-Bar

        enemyHpSlider.value = Hp.Value / maxHp;
        if (Hp.Value < maxHp)
        {
            hpBar.SetActive(true);

            if (Input.GetKey(KeyCode.LeftShift))  //Sollte am Ende auf KeyCode.LeftAlt geändert werden.
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = Mathf.RoundToInt(Hp.Value) + "/" + Mathf.RoundToInt(maxHp);
                    statText[0].text = level.ToString();
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


        if (Hp.Value <= 0)
            Destroy(gameObject);
        #endregion

    }

    private void SetDestination()
    {
        if (character_transform != null)
        {

            navMeshAgent.SetDestination(character_transform.transform.position);

            // Hier wird die "Blickrichtung" bestimmt, welche sich an dem Charakter orientiert.
            Vector3 Direction = character_transform.transform.position - transform.position;

            Vector2 inputVector = new Vector2(Direction.x * -1, Direction.z);

            inputVector = Vector2.ClampMagnitude(inputVector, 1);

            if (ikAnimated == false)
                isoRenderer.SetNPCDirection(inputVector);

            //irgendwo hier ist noch n kleiner fehler


        }
    }

    private void Attack()
    {


        PlayerStats playerStats = character_transform.GetComponent<PlayerStats>();

        attackCD -= Time.deltaTime;

        if (playerStats != null)
        {
            if (attackCD <= 0)
            {

                //Sound-Array mit den dazugehörigen Sound-Namen
                string[] hitSounds = new string[] { "Mob_ZombieAttack1", "Mob_ZombieAttack2", "Mob_ZombieAttack3" };

                //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
                if (AudioManager.instance != null)

                    //Play a Sound at random.
                    AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);

                //Füge dem Spieler Schaden entsprechend der AttackPower hinzu.
                playerStats.TakeDamage(AttackPower.Value);

                //Der Versuch einen AttackSpeed zu integrieren
                attackCD = 1f / AttackSpeed.Value;

            }
        }
    }


    //Eigentlich müsste die p_attackCD pro Instanz unterschiedlich sein, aber da das zu funktionieren scheint. Bleibt das so.
    private void OnTriggerStay(Collider collider)
    {
        /*
        if (collider.gameObject == DirectionCollider.instance.dirCollider) 
        {

            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();


            p_attackCD -= Time.deltaTime;


                if (p_attackCD <= 0)
                {

                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {

                        TakeDamage(playerStats.AttackPower.Value, playerStats.Range);

                        p_attackCD = 1f / playerStats.AttackSpeed.Value;
                    }

                }            

        }
        */
    }
    
    //Überarbeitungswürdig. Soll schließlich eine Abfrage für Collision mit sämtlichen Projektilen ergeben.
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bullet") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {
            //Derzeit müssten die entsprechenden Bullet-Scripts hier abgefragt werden, dats stupid.
            Steinwurf_Bullet steinwurf_bullet = collider.GetComponent<Steinwurf_Bullet>();

            TakeDamage(steinwurf_bullet.steinwurf.damage, steinwurf_bullet.steinwurf.range); // <- Die Bullets werden endlos fliegen können, jedoch erst ab spell.range schaden machen.         

        }
    }




    public void TakeDamage(float damage, float range)
    {

        player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range)
        {
            damage = 10 * (damage * damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte

            damage = Mathf.Clamp(damage, 1, int.MaxValue);

            Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));

            
            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

                //Play a Sound at random.
                AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);
            

            pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }

        if (Hp.Value <= 0)
            Die();
    }


    public void Die()
    {

        PlayerStats playerStats = character_transform.GetComponent<PlayerStats>();
        playerStats.Set_xp(Experience);

        string _lootTable = lootTable.ToString();
        ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);

        Destroy(gameObject);
    }

}