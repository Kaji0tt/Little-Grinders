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
    private bool dirCollider = false;
    private float attackCD = 0f;
    public Sprite Dead;
    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;

    //Vektoren zur Berechnung von Position/Ziel/Direction
    Vector3 forward, right;
    private float xInputVector, zInputVector;

    public float aggroRange = 5f, attackRange;
    private float distance;
    private Vector3 targetVector;


    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        //enemyStats = GetComponent<EnemyStats>();


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

    //Klappt so halb. Sobald einmal true, geht er nicht mehr auf false.
    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.name == "DirectionCollider")
            dirCollider = true;
        else
            dirCollider = false;
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
        if (dirCollider == true)
        {
            distance = Vector3.Distance(destination.position, transform.position);
            //print(distance);
            //print(range);
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
    }
    

    public void Die()
    {
        print("Na supi! Erster Gegener wurde getötet.");
        
        //gequirrlte scheiße. animationen sind mal wieder n ganz neue chapter, der mob soll halt einfach verschwinden plsssss
        this.gameObject.GetComponent<SpriteRenderer>().sprite = Dead;
        GameObject.Destroy(this);
    }
}
