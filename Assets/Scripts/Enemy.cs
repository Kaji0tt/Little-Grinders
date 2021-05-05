using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum LootTable { tier1, tier2, tier3, tier4, tier5 }

public class Enemy : MonoBehaviour
{
    //Ich konnte auf anhieb jetzt kein Objekt finden, welches mit Enemy.cs läuft. Sicherheitshalber erstmal da lassen.
    //05.05.21
    /*
    [SerializeField]
    Transform destination;

    NavMeshAgent navMeshAgent;
    IsometricRenderer isoRenderer;


    ///----Combat Variables-----
    ///
    CharacterCombat combat;
    IsometricPlayer isometricPlayer;
    private GameObject scriptController;
    ItemDatabase itemDatabase;
    public GameObject hpBar;
    public Slider enemyHpSlider;


    ///-----Stat Stuff-----
    ///
    [Space]

    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;
    public int level;

    private float attackCD = 0f;
    private float maxHp;
    [Space]
    public int Experience;
    public float aggroRange = 5f, attackRange;
    //private PlayerStats playerStats;
    public LootTable lootTable;


    ///----Position/Ziel/Direction----
    ///
    Vector3 forward, right;
    private float xInputVector, zInputVector;
    private float player_distance;
    private Vector3 targetVector;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        scriptController = GameObject.FindWithTag("GameController");
        itemDatabase = scriptController.GetComponent<ItemDatabase>();
        

        maxHp = Hp.Value;



        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        destination = PlayerManager.instance.player.transform;
        //PlayerStats playerStats = destination.GetComponent<PlayerStats>();

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        isoRenderer = GetComponentInChildren<IsometricRenderer>();
    }


    void Update()
    {

        // Die Direction berechnet sich noch aus den Welt
        navMeshAgent = GetComponent<NavMeshAgent>();
        player_distance = Vector3.Distance(destination.position, transform.position);

        if (navMeshAgent == null)
        {
            Debug.LogError("The Nav Mesh agent Component is not attached to " + gameObject.name);
        }
        else if (player_distance <= aggroRange)
        {
            SetDestination();

            if (player_distance <= attackRange)
            {
                //Hier müssten entsprechende Animationen geladen werden, welche sich aus dem inputVector ergibt.
                //isoRenderer.SetNPCDirection(inputVector); <-- Im isoRender entsprechende Animationen hinzufügen.

                PlayerStats playerStats = destination.GetComponent<PlayerStats>();
                attackCD -= Time.deltaTime;
                if (playerStats != null)
                {
                    if (attackCD <= 0)
                    {
                        playerStats.TakeDamage(AttackPower.Value);
                        attackCD = 1f / AttackSpeed.Value;
                    }
                }


            }

        }

        #region Hp-Bar

        enemyHpSlider.value = Hp.Value / maxHp;
        if(Hp.Value < maxHp)
            hpBar.SetActive(true);

        if (Hp.Value <= 0)
            Destroy(gameObject);
        #endregion

    }


    private void OnTriggerStay(Collider collider)
    {
        /*
        if (collider.gameObject.name == "DirectionCollider") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
            float attackCD = playerStats.attackCD;
            attackCD -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (attackCD <= 0)
                {
                    TakeDamage(playerStats.AttackPower.Value, playerStats.Range);
                    attackCD = 1f / playerStats.AttackSpeed.Value;
                }
            }
        }
        
    }

    private void SetDestination()
    {
        if (destination != null)
        {
            //distance = Vector3.Distance(destination.position, transform.position);
            Vector3 targetVector = destination.transform.position;
            navMeshAgent.SetDestination(targetVector);

            // Hier wird die "Blickrichtung" bestimmt, welche sich an dem Charakter orientiert.
            Vector3 Direction = destination.transform.position - transform.position;
            Vector2 inputVector = new Vector2(Direction.x * -1, Direction.z);
            inputVector = Vector2.ClampMagnitude(inputVector, 1);
            isoRenderer.SetNPCDirection(inputVector);

        }
    }
    
    public void TakeDamage(float damage, int range)
    {
        player_distance = Vector3.Distance(destination.position, transform.position);       
            if (player_distance <= range)
            {
              damage = 10 * (damage*damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte
              damage = Mathf.Clamp(damage, 1, int.MaxValue);
              Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
              

            }

        if (Hp.Value <= 0)
            Die();
    }


    public void Die()
    {
        //print("enemy died");
        PlayerStats playerStats = destination.GetComponent<PlayerStats>();
        playerStats.Set_xp(Experience);

        string _lootTable = lootTable.ToString();
        itemDatabase.GetComponent<ItemDatabase>().GetWeightDrop(gameObject.transform.position);   
        Destroy(gameObject);
    }
    */
}
