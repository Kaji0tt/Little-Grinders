using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class _projectile : MonoBehaviour
{
    private Vector3 _pDirection;

    public bool _pSpecialEffect;

    [HideInInspector]
    public float _pDamage;

    public float _pSpeed;

    //public float _pYOffSet;

    public ParticleSystem _hitParticles;

    private Rigidbody _pRbody;

    private enum Trajectory { FollowTarget, Direction, Falling, Curve}

    [SerializeField]
    private Trajectory trajectory;

    void Start()
    {
        _pRbody = GetComponent<Rigidbody>();

        if(trajectory == Trajectory.Direction)
        {
            //_pDirection = new Vector2((PlayerManager.instance.player.transform.position.x - transform.position.x), (PlayerManager.instance.player.transform.position.z - transform.position.z));
            _pDirection = new Vector3(PlayerManager.instance.player.transform.position.x - transform.position.x, 0, PlayerManager.instance.player.transform.position.z - transform.position.z);
        }
        if (trajectory == Trajectory.FollowTarget)
        {
            print("Not implented yet");
        }
        if (trajectory == Trajectory.Falling)
        {
            print("Not implented yet");
        }
        if (trajectory == Trajectory.Curve)
        {
            print("Not implented yet");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (trajectory == Trajectory.Direction)
        {
            ProjectileFlightByDirection();
        }
    }

    private void ProjectileFlightByDirection()
    {
        print(_pDirection.normalized);
        _pRbody.velocity = (_pDirection.normalized * _pSpeed);
    }

    private void OnTriggerEnter(Collider collider)
    {

        if (collider.gameObject.tag == "Player")
        {
            if(_pSpecialEffect)
            {
                ApplySpecialEffect();
            }
            else
            {
                PlayerManager.instance.player.GetComponent<PlayerStats>().TakeDamage(_pDamage);
                Instantiate(_hitParticles, PlayerManager.instance.player.transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
        else if (collider.gameObject.tag == "Env")
        {
            Destroy(gameObject);
        }



    }

    public virtual void ApplySpecialEffect()
    {

    }
}
