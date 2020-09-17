using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{

    [SerializeField]
    Transform destination;


    NavMeshAgent navMeshAgent;
    IsometricCharacterRenderer isoRenderer;

    //Combat Stuff
    CharacterCombat combat;
    IsometricPlayer isometricPlayer;
    EnemyStats enemyStats;
    private float attackCD = 0f;
    public Sprite Dead;

    //Vektoren zur Berechnung von Position/Ziel/Direction
    Vector3 forward, right;
    private float xInputVector, zInputVector;

    public float aggroRange = 5f, attackRange;
    private float distance;
    private Vector3 targetVector;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        enemyStats = GetComponent<EnemyStats>();


        // Ein entsprechender Verweis wird spätestens wenn Enemys Spawnen sollen wichtig sein. (EnemyManager.instance.enemy.transform
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
                        isometricPlayer.TakeDamage(enemyStats.AttackPower.Value);
                        attackCD = 1f / enemyStats.AttackSpeed.Value;
                    }
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
        print(distance);
        print(range);
        if (distance <= range)
            print("Der call in NPC Move kam an");
        damage -= enemyStats.Armor.Value;
        damage = Mathf.Clamp(damage, 0, int.MaxValue);
        enemyStats.Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
        if (enemyStats.Hp.Value <= 0)
            Die();

    }
    

    public void Die()
    {
        print("Na supi! Erster Gegener wurde getötet.");
        
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Dead;
        GameObject.Destroy(this);
    }
}
