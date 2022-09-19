using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

//Hier liegt ein Denkfehler vor.
//Generell wäre es sinnvoll, wenn der EnemyController oder eine geerbte Klasse auf bestimmte "Abilities" Zugriff hat,
//wäre auch später wichtig für Rare Mobs.
public class E_Caster : EnemyController
{
    public GameObject projectile;

    [SerializeField]
    private float projectileDamage;

    private NavMeshAgent casterNavMeshAgent;

    new private float player_distance;

    //new public float attackCD = 2;

    private float crnt_attackCD;



    [Header("Animation Settings")]

    private bool attackStarted;

    public float animationProjectileDelay = 0.5f;

    public float projectileOriginY = 0.5f;

    public float projectileOffsetXY = 0.5f;

    private IsometricRenderer isoRenderer;

    private void Start()
    {
        isoRenderer = gameObject.AddComponent<IsometricRenderer>();
    }

    public override void CheckForAggro()
    {
        casterNavMeshAgent = GetComponent<NavMeshAgent>();

        player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= aggroRange || pulled)
        {
            //Setze die Agent.StoppingDistance gleich mit der AttackRange des Mobs
            //If the StoppingDistance is smaller then the attackRange, faster mobs may attack while the player is running away
            //navMeshAgent.stoppingDistance = attackRange / 2;
            casterNavMeshAgent.stoppingDistance = attackRange;


            //Falls die Spieler-Distanz kleiner ist, als die Attack Range
            if (player_distance < attackRange && casterNavMeshAgent.velocity == Vector3.zero)
            {
                Attack();
            }
        }      
    }


    public override void Attack()
    {
        crnt_attackCD -= Time.deltaTime;

        if(crnt_attackCD <= 0)
        {

            isoRenderer.AnimateCast(CalculatePlayerDirection());

            //Der Versuch einen AttackSpeed zu integrieren - je kleiner der mobStats.AttackSpeed.Value, desto mehr Zeit zwischen den Angriffen.
            crnt_attackCD = 1f / mobStats.AttackSpeed.Value;

            attackStarted = true;

        }

        if(crnt_attackCD <= mobStats.attackCD - animationProjectileDelay && attackStarted)
        {
            InstantiateProjectile();

            attackStarted = false;
        }

    }

    private void InstantiateProjectile()
    {
        //Calculate the Direction of the Player and Offset the Origin of the Projectile by "projectileOffsetXY"

        if (isoRenderer.DirectionToIndex(CalculatePlayerDirection(), 4) < 2)
        Instantiate(projectile, new Vector3(transform.position.x + projectileOffsetXY, transform.position.y + projectileOriginY, transform.position.z), Quaternion.identity, transform).GetComponent<_projectile>()._pDamage = projectileDamage;


        else
        Instantiate(projectile, new Vector3(transform.position.x - projectileOffsetXY, transform.position.y + projectileOriginY, transform.position.z), Quaternion.identity, transform).GetComponent<_projectile>()._pDamage = projectileDamage;
    }


}
