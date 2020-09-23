using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    Transform destination;


    NavMeshAgent navMeshAgent;
    IsometricCharacterRenderer isoRenderer;

    //Combat Stuff
    CharacterCombat combat;
    IsometricPlayer isometricPlayer;
    private GameObject scriptController;
    ItemDatabase itemDatabase;

    //Stat Stuff
    private float attackCD = 0f;
    //public Sprite Dead;

    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;
    [Space]
    [Header("Combat Stats")]
    public int Experience;
    public float aggroRange = 5f, attackRange;

    //Vektoren zur Berechnung von Position/Ziel/Direction
    Vector3 forward, right;
    private float xInputVector, zInputVector;

    //Kampf Variabeln

    private float distance;
    private Vector3 targetVector;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        scriptController = GameObject.FindWithTag("GameController");
        itemDatabase = scriptController.GetComponent<ItemDatabase>();


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
              damage -= Armor.Value;
              damage = Mathf.Clamp(damage, 0, int.MaxValue);
              Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
              print(gameObject + " hat " + damage + " Schaden erhalten");
              if (Hp.Value <= 0)
              Die();
            }
    }


    public void Die()
    {
        print("enemy died");
        itemDatabase.GetComponent<ItemDatabase>().GetDrop(gameObject.transform.position);   
        Destroy(gameObject);
    }
}
