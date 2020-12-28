using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class EnemyController : MonoBehaviour
{

    [SerializeField]
    Transform character_transform;

    NavMeshAgent navMeshAgent;
    IsometricCharacterRenderer isoRenderer;
    //private Animator animator;
    private EnemyAnimator enemyAnimator;
    public bool animated;


    ///----Combat Variables-----
    ///
    CharacterCombat combat;
    IsometricPlayer isometricPlayer;
    private GameObject scriptController;
    ItemDatabase itemDatabase;
    public GameObject hpBar;
    public Slider enemyHpSlider;
    private Spell spell;


    ///-----Stat Stuff-----
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
    Vector3 forward, right;
    private float player_distance;
    private Vector3 targetVector;
    Vector2 inputVector;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        scriptController = GameObject.FindWithTag("GameController");
        itemDatabase = scriptController.GetComponent<ItemDatabase>();
        enemyAnimator = GetComponentInChildren<EnemyAnimator>();
        //animator = GetComponent<Animator>();


        maxHp = Hp.Value;



        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        character_transform = PlayerManager.instance.player.transform;
        //PlayerStats playerStats = destination.GetComponent<PlayerStats>();

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        if (animated == false)
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }


    void Update()
    {

        navMeshAgent = GetComponent<NavMeshAgent>();

        player_distance = Vector3.Distance(character_transform.position, transform.position);

        if (animated == true)
            enemyAnimator.AnimateMe(inputVector, player_distance, attackRange, aggroRange);

        if (navMeshAgent == null)
        {
            Debug.LogError("The Nav Mesh agent Component is not attached to " + gameObject.name);
        }

        else
        {

            if (player_distance <= aggroRange)
            {
                SetDestination();
                inputVector = transform.position - navMeshAgent.transform.position;

                if (player_distance <= attackRange)
                {

                    PlayerStats playerStats = character_transform.GetComponent<PlayerStats>();
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

            else
                navMeshAgent.SetDestination(transform.position);
        }



        #region Hp-Bar

        enemyHpSlider.value = Hp.Value / maxHp;
        if (Hp.Value < maxHp)
            hpBar.SetActive(true);

        if (Hp.Value <= 0)
            Destroy(gameObject);
        #endregion

    }


    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.name == "DirectionCollider") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {
            PlayerStats playerStats = character_transform.GetComponent<PlayerStats>();
            playerStats.attackCD -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (playerStats.attackCD <= 0)
                {
                    TakeDamage(playerStats.AttackPower.Value);
                    playerStats.attackCD = 1f / playerStats.AttackSpeed.Value;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bullet") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {

            spell = collider.GetComponent<Spell>();
            TakeDamage(spell.damage);


        }
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

            if (animated == false) 
            isoRenderer.SetNPCDirection(inputVector);


            //irgendwo hier ist noch n kleiner fehler


        }
    }

    public void TakeDamage(float damage)
    {
        PlayerStats playerStats = character_transform.GetComponent<PlayerStats>();
        player_distance = Vector3.Distance(character_transform.position, transform.position);
        if (player_distance <= playerStats.Range)
        {
            damage = 10 * (damage * damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte
            damage = Mathf.Clamp(damage, 1, int.MaxValue);
            Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
        }

        //jetzt müsste eine Abfrage 


        if (Hp.Value <= 0)
            Die();
    }


    public void Die()
    {
        //print("enemy died");
        PlayerStats playerStats = character_transform.GetComponent<PlayerStats>();
        playerStats.Set_xp(Experience);

        string _lootTable = lootTable.ToString();
        itemDatabase.GetComponent<ItemDatabase>().GetWeightDrop(gameObject.transform.position);
        Destroy(gameObject);
    }

    /*
    private void AnimateMe()
    {
        animator.SetFloat("AnimDistance", player_distance);
        

        if(animator.GetFloat("AnimDistance") <= attackRange)
        {
            animator.Play("F_Attacking");
        }
        else if (animator.GetFloat("AnimDistance") <= aggroRange)
        {
            animator.Play("F_Chasing");
        }
        else if (animator.GetFloat("AnimDistance") >= aggroRange)
        {
            animator.Play("F_Idle");
        }
    }
    */

}