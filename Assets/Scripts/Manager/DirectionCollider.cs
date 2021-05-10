using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        //Das is noch nicht perfekt, aber OnTriggerExit() funktioniert nicht richtig, 
        //um die entsprechenden Listeneinträge zu entfernen. Deshalb dafür noch WorkAround finden.
        for(int i = 0; i < collidingEnemyControllers.Count; i++)
        {
            if(collidingEnemyControllers.ElementAt(i) == null)
            {
                EnemyController enemy = collidingEnemyControllers.ElementAt(i);
                collidingEnemyControllers.Remove(enemy);
            }

        }


    }

    private void Move()
    {
        Vector3 direction = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        direction = Vector3.ClampMagnitude(direction, 1);
        direction = direction.normalized;

        //Wenn der Charakter sich mit mehr als 0,3f bewegt,
        if (direction.magnitude > .3f)
            //Setze die Position des DirColliders auf die des Charakters, + direction
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
