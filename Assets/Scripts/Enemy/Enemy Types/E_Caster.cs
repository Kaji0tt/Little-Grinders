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

    private GameObject projClone;

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



    public override void AddEssentialComponents()
    {
        //For some reason Unity spits out an error if we are Instantiating the IsoRenderer and MobCamScript like this.
        base.AddEssentialComponents();

    }



    /*
    public override void Attack()
    {
        crnt_attackCD -= Time.deltaTime;

        if(crnt_attackCD <= 0)
        {

            //gameObject.GetComponent<IsometricRenderer>().AnimateCast();

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
    */
    private void InstantiateProjectile()
    {
        //Calculate the Direction of the Player and Offset the Origin of the Projectile by "projectileOffsetXY"

        if (gameObject.GetComponent<IsometricRenderer>().DirectionToIndex(CalculatePlayerDirection(), 4) < 2)
        Instantiate(projectile, new Vector3(transform.position.x + projectileOffsetXY, transform.position.y + projectileOriginY, transform.position.z), Quaternion.identity, transform).GetComponent<_projectile>()._pDamage = projectileDamage;


        else
        {
            //Initialisiere ein Projektil
            projClone = Instantiate(projectile, new Vector3(transform.position.x - projectileOffsetXY, transform.position.y + projectileOriginY, transform.position.z), Quaternion.identity, transform);
            //Setze den Schaden des Projektils
            projClone.GetComponent<_projectile>()._pDamage = projectileDamage;
            //Bestimme die Ursprungs-Entitie für das Projektil.
            projClone.GetComponent<_projectile>().SetOrigin(this.gameObject.GetComponent<MobStats>());

            //print("E_Caster else called.");
        }

    }


}
