using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    Transform destination;

    NavMeshAgent navMeshAgent;
    IsometricCharacterRenderer isoRenderer;


    ///----Combat Variables-----
    ///
    CharacterCombat combat;
    IsometricPlayer isometricPlayer;
    private GameObject scriptController;
    ItemDatabase itemDatabase;
    public GameObject hpBar;
    public Slider slider;


    ///-----Stat Stuff-----
    ///
    [Header("Stats")]
    private float attackCD = 0f;
    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;
    private float maxHp;
    [Space]
    public int Experience;
    public float aggroRange = 5f, attackRange;


    ///----Position/Ziel/Direction----
    ///
    Vector3 forward, right;
    private float xInputVector, zInputVector;
    private float distance;
    private Vector3 targetVector;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        scriptController = GameObject.FindWithTag("GameController");
        itemDatabase = scriptController.GetComponent<ItemDatabase>();
        maxHp = Hp.Value;


        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        destination = PlayerManager.instance.player.transform;

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }


    void Update()
    {

        // Die Direction berechnet sich noch aus den Welt
        navMeshAgent = GetComponent<NavMeshAgent>();
        distance = Vector3.Distance(destination.position, transform.position);

        if (navMeshAgent == null)
        {
            Debug.LogError("The Nav Mesh agent Component is not attached to " + gameObject.name);
        }
        else if (distance <= aggroRange)
        {
            SetDestination();

            if (distance <= attackRange)
            {
                //Hier müssten entsprechende Animationen geladen werden, welche sich aus dem inputVector ergibt.
                //isoRenderer.SetNPCDirection(inputVector); <-- Im isoRender entsprechende Animationen hinzufügen.

                IsometricPlayer isometricPlayer = destination.GetComponent<IsometricPlayer>();
                attackCD -= Time.deltaTime;
                if (isometricPlayer != null)
                {
                    if (attackCD <= 0)
                    {
                        isometricPlayer.TakeDamage(AttackPower.Value);
                        attackCD = 1f / AttackSpeed.Value;
                    }
                }


            }

        }

        #region Hp-Bar

        slider.value = Hp.Value / maxHp;
        if(Hp.Value < maxHp)
            hpBar.SetActive(true);

        if (Hp.Value <= 0)
            Destroy(gameObject);
        #endregion

    }


    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.name == "DirectionCollider")
        {
            IsometricPlayer isometricPlayer = destination.GetComponent<IsometricPlayer>();
            isometricPlayer.attackCD -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (isometricPlayer.attackCD <= 0)
                {
                    TakeDamage(isometricPlayer.AttackPower.Value, isometricPlayer.Range);
                    isometricPlayer.attackCD = 1f / isometricPlayer.AttackSpeed.Value;
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
        distance = Vector3.Distance(destination.position, transform.position);       
            if (distance <= range)
            {
              damage = 10 * (damage*damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte
              damage = Mathf.Clamp(damage, 1, int.MaxValue);
              Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
              print(gameObject + " hat " + damage + " Schaden erhalten");
              if (Hp.Value <= 0)
              Die();
            }
    }


    public void Die()
    {
        //print("enemy died");
        itemDatabase.GetComponent<ItemDatabase>().GetDrop(gameObject.transform.position);   
        Destroy(gameObject);
    }
}
