using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class EnemyController : MonoBehaviour
{

    [SerializeField]
    Transform character_transform;

    NavMeshAgent navMeshAgent;
    IsometricRenderer isoRenderer;
    //private Animator animator;
    private EnemyAnimator enemyAnimator;
    public bool animated;


    ///----Combat Variables-----
    ///
    CharacterCombat combat;
    IsometricPlayer isometricPlayer;
    private GameObject scriptController;
    public GameObject hpBar;
    public Slider enemyHpSlider;

    /// <summary>
    /// Bullet Types - finde eine bessere Methode die entsprechenden Scripte abzurufen.
    /// </summary>
    //private Steinwurf steinwurf;


    ///-----Stat Stuff-----
    [Space]
    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;

    public int level;

    TextMeshProUGUI[] statText;

    private float attackCD = 0f;
    private float p_attackCD = 0f;
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
    public bool pulled;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        scriptController = GameObject.FindWithTag("GameController");
        enemyAnimator = GetComponentInChildren<EnemyAnimator>();
        //animator = GetComponent<Animator>();

        //UI Display
        TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();
        maxHp = Hp.Value;



        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        character_transform = PlayerManager.instance.player.transform;

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        if (animated == false)
        isoRenderer = GetComponentInChildren<IsometricRenderer>();
    }


    void Update()
    {


        navMeshAgent = GetComponent<NavMeshAgent>();

        player_distance = Vector3.Distance(character_transform.position, transform.position);



        inputVector.x = character_transform.transform.position.x - transform.position.x;
        inputVector.y = character_transform.transform.position.z - transform.position.z;
        if (animated == true)
            enemyAnimator.AnimateMe(inputVector, player_distance, attackRange, aggroRange);

        if (navMeshAgent == null)
        {
            Debug.LogError("The Nav Mesh agent Component is not attached to " + gameObject.name);
        }

        else
        {
            if (player_distance <= aggroRange || pulled)
            {
                SetDestination();


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
                navMeshAgent = null;
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

    void ShowStatText()
    {

    }

    //Eigentlich müsste die p_attackCD pro Instanz unterschiedlich sein, aber da das zu funktionieren scheint. Bleibt das so.
    private void OnTriggerStay(Collider collider)
    {

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
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Bullet") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {

            Steinwurf_Bullet steinwurf_bullet = collider.GetComponent<Steinwurf_Bullet>();

            TakeDamage(steinwurf_bullet.steinwurf.damage, steinwurf_bullet.steinwurf.range); // <- Die Bullets werden endlos fliegen können, jedoch erst ab spell.range schaden machen.         

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

    public void TakeDamage(float damage, float range)
    {

        player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range)
        {
            damage = 10 * (damage * damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte

            damage = Mathf.Clamp(damage, 1, int.MaxValue);

            Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));

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