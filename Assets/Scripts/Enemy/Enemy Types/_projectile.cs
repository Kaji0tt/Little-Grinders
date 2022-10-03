using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Um bestimmte Buffs hinzuzufügen, kann im Projektil GetComponent<Buff> gecalled werden, falls dies != null ist, kann der entsprechende Buff bei
//Kollision applied werden.
public class _projectile : MonoBehaviour
{
    private Vector3 _pDirection;

    private Quaternion _pRotation;

    public bool _pSpecialEffect = true;

    //Schön wäre eine Liste, bzw. ein Dict in welchem die Buffs liegen. Perfekter Weise ein Enum mit ScriptableObjects zum auswählen.
    public Buff buff;

    //Der Ursprung des Projektils um gegebenenfalls AP Values zur Bearbeitung des Schadens zu verwenden.
    //public GameObject projectileOrigin;

    [HideInInspector]
    public float _pDamage;

    public float _pSpeed;

    //public float _pYOffSet;

    public ParticleSystem _hitParticles;

    public GameObject _hitEnvParticles;

    private Rigidbody _pRbody;

    public IEntitie _origin { get; private set; }


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
        //print(_pDirection.normalized);
        _pRbody.velocity = (_pDirection.normalized * _pSpeed);

        _pRotation = Quaternion.LookRotation(_pDirection);
        transform.localRotation = Quaternion.Lerp(transform.rotation, _pRotation, 1);
        transform.Rotate(new Vector3(-90, 0, 0));
        
    }

    //Falls der Collider der Spieler ist
    private void OnTriggerEnter(Collider collider)
    {
       
        if (collider.gameObject.tag == "Player")
        {
            //Und falls das Projektil einen SpecialEffect besitzt
            if(_pSpecialEffect)
            {
                ApplySpecialEffect(PlayerManager.instance.player.transform.GetComponent<PlayerStats>());
                Instantiate(_hitParticles, PlayerManager.instance.player.transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            else
            {
                PlayerManager.instance.player.GetComponent<PlayerStats>().TakeDamage(_pDamage, 0);
                Instantiate(_hitParticles, PlayerManager.instance.player.transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
        else if (collider.gameObject.tag == "Env")
        {
            Instantiate(_hitEnvParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
//muss frei bleiben hier.
    }

    public virtual void ApplySpecialEffect(IEntitie targetEntitie)
    {
        //Erschaffe eine Kopie des angegebenen Buffs
        BuffInstance buffInstance = BuffDatabase.instance.GetInstance(buff.buffName);

        //Und füge diese der Ziel-Entitie des Projektils hinzu und vermittel ?Der Ziel-Entitie? die Informationen des Ursprungs.
        //->Die Ziel-Entitie muss nicht wissen, wo der Urpsrung des Buffs liegt, lediglich der Buff muss dies wissen.
        buffInstance.ApplyBuff(targetEntitie, _origin);

    }

    public void SetOrigin(IEntitie origin)
    {
        _origin = origin;
    }

}
