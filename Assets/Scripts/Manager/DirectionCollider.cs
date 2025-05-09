﻿using System.Collections;
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

    //private Vector3 right, forward;
    public Vector3 dirVector;

    //IsometricPlayer player;

    public List<EnemyController> collidingEnemyControllers = new List<EnemyController>();
    //public List<GameObject> collidingEnemyControllers = new List<GameObject>();

    void Start()
    {
        IsometricPlayer player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();


        #region Alter Dir-Collider
        //Übernehmen der Kamera Achsen
        /*
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
        */
        #endregion

    }


    void Update()
    {
        int floorLayerMask = LayerMask.GetMask("Floor"); // Erstelle eine Layer-Maske nur für die Ebene mit dem Tag "Floor"
        
        //Direction of Directioncollider = Input.Mouse World Position - Charakter Position 
        Ray ray = CameraManager.instance.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, floorLayerMask))
        {
            //
            Vector3 rayhitDir = raycastHit.point - PlayerManager.instance.player.transform.position;
            dirVector = rayhitDir.normalized;
        }

        Vector3 newPosition = PlayerManager.instance.player.transform.position + dirVector;
        newPosition.y = transform.position.y; // Setze die Y-Position auf die aktuelle Y-Position des DirectionColliders
        transform.position = newPosition;
    

        //transform.position = PlayerManager.instance.player.transform.position + dirVector;
        //Waffenposition anpassen

        //player.isoRenderer.SetWeaponDirection(dirVector, player.weaponGameObject.GetComponent<Animator>());

        #region Alter Dir-Collider
        /*
        if (Input.anyKey)
        {
            Move();

        }
        */
        #endregion
        //Das is noch nicht perfekt, aber OnTriggerExit() funktioniert nicht richtig, 
        //um die entsprechenden Listeneinträge zu entfernen. Deshalb dafür noch WorkAround finden.
        for (int i = 0; i < collidingEnemyControllers.Count; i++)
        {
            if(collidingEnemyControllers.ElementAt(i) == null)
            {
                EnemyController enemy = collidingEnemyControllers.ElementAt(i);
                collidingEnemyControllers.Remove(enemy);
            }

        }

        //print(collidingEnemyControllers.Count());




    }

    /*
    private void Move()
    {
        Vector3 direction = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        direction = Vector3.ClampMagnitude(direction, 1);
        direction = direction.normalized;

        //Wenn der Charakter sich mit mehr als 0,3f bewegt,
        if (direction.magnitude > .3f)
            //Setze die Position des DirColliders auf die des Charakters, + direction
            transform.position = PlayerManager.instance.player.transform.position + direction;

    }
    */
    //Bleibt die Frage, ob es schlauer ist im Enemy.cs oder im DirCollider die TakeDamage abfrage zu callen.


 /*
    private void OnTriggerEnter(Collider collider)
    {
        //collidingEnemys = Collider.FindObjectsOfType<Enemy>();

        if(collider.gameObject.tag == "Enemy")
        {

            if(!collidingEnemyControllers.Contains(collider.gameObject.GetComponentInParent<EnemyController>()))
            {

            }
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
    */



}


