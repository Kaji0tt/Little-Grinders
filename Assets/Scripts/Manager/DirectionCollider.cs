using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionCollider : MonoBehaviour
{
    #region Singleton;
    public static DirectionCollider instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject dirCollider;
    #endregion

    private float attackCD = 0;
    
    private Vector3 right, forward;

    public List<EnemyController> collidingEnemyControllers = new List<EnemyController>();


    void Start()
    {
        IsometricPlayer player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();

        //Übernehmen der Kamera Achsen
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }


    void Update()
    {

        if (Input.anyKey)
        {
            Move();

        }
        
    }

    private void Move()
    {
        Vector3 direction = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        direction = Vector3.ClampMagnitude(direction, 1);
        direction = direction.normalized;

        if (direction.magnitude > .3f)
            transform.position = new Vector3(PlayerManager.instance.player.transform.position.x, PlayerManager.instance.player.transform.position.y, PlayerManager.instance.player.transform.position.z)
                                 + direction;

    }

    //Bleibt die Frage, ob es schlauer ist im Enemy.cs oder im DirCollider die TakeDamage abfrage zu callen.
    

 
    private void OnTriggerEnter(Collider collider)
    {
        //collidingEnemys = Collider.FindObjectsOfType<Enemy>();

        if(collider.gameObject.tag == "Enemy")
        {

            if(!collidingEnemyControllers.Contains(collider.gameObject.GetComponentInParent<EnemyController>()))
            collidingEnemyControllers.Add(collider.gameObject.GetComponentInParent<EnemyController>());

        }
        
    }


    private void OnTriggerExit(Collider collider)
    {

        if (collider.gameObject.tag == "Enemy")
        {
            if(collidingEnemyControllers.Contains(collider.gameObject.GetComponentInParent<EnemyController>()))
            collidingEnemyControllers.Remove(collider.gameObject.GetComponentInParent<EnemyController>());
        }

    }
    



}
