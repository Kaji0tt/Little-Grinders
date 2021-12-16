using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class E_Caster : EnemyController
{
    public GameObject projectile;

    [SerializeField]
    private float projectileDamage;

    private NavMeshAgent _EnavMeshAgent;

    private float _Eplayer_distance;

    private float _EattackCD = 2;
    private float _Ecrnt_attackCD;

    //private IsometricRenderer isoRenderer;

    private void Start()
    {
        isoRenderer = GetComponent<IsometricRenderer>();
    }

    public override void CheckForAggro()
    {
        _EnavMeshAgent = GetComponent<NavMeshAgent>();

        _Eplayer_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);


        if (_Eplayer_distance <= aggroRange || pulled)
        {
            //Setze die Agent.StoppingDistance gleich mit der AttackRange des Mobs
            //If the StoppingDistance is smaller then the attackRange, faster mobs may attack while the player is running away
            //navMeshAgent.stoppingDistance = attackRange / 2;
            _EnavMeshAgent.stoppingDistance = attackRange;

            //Setze das Target des Mobs und starte "chasing"
            //SetDestination(); -
            if (attackRange < aggroRange)
                //isoRenderer.SetNPCDirection(SetDestination());
                SetDestination();
            else if (attackRange >= aggroRange)
                isoRenderer.SetNPCDirection(new Vector2(0, 0));

            //Falls die Spieler-Distanz kleiner ist, als die Attack Range
            if (_Eplayer_distance < attackRange && _EnavMeshAgent.velocity == Vector3.zero)
            {
                //Setze "inCombatStance" des Iso-Renderers dieser Klasse auf true
                isoRenderer.inCombatStance = true;

                //Attack the Target
                Attack();


            }

            else
                //Setze Combat-Stance zurück
                isoRenderer.inCombatStance = false;
        }

        else
        {
            _EnavMeshAgent.SetDestination(transform.position);
            isoRenderer.SetNPCDirection(new Vector2(0, 0));
        }

        
    }


    public override void Attack()
    {
        //base.Attack();
        _Ecrnt_attackCD -= Time.deltaTime;
        //Falls Attack-CD <= 0, Instantiate Projectile(transform.position)
        if(_Ecrnt_attackCD <= 0)
        {
            _Ecrnt_attackCD = _EattackCD;

            if (MapGenHandler.instance != null)
            Instantiate(projectile, new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), Quaternion.identity, MapGenHandler.instance.envParentObj.transform).GetComponent<_projectile>()._pDamage = projectileDamage;
            else
                Instantiate(projectile, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity, GameObject.Find("IntroTruhe Variant").transform).GetComponent<_projectile>()._pDamage = projectileDamage; ;
        }


        //Das Projektil sollte in void Start haben: direction = (PlayerManager.instance.player.transform.position - transform.position)

            //Falls Projektil berührt Collider mit Tag Player, GameObject.Destroy, Player.TakeDamage(projectile DMG);
            //Else, falls Projektil berührt Collider mit Tag ENV, GameObject.Destroy


    }
}
