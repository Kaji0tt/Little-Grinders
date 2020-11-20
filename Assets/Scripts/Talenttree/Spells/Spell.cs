using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    private Rigidbody myRigidBody;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float bullet_height;

    public float damage;


    private Transform target;
    private Vector3 spell_destination;

    // Diese Datei wäre auch praktisch für z.B. Pfeile
    // Target sollte stets der erste Enemy sein, welcher sich im 3D Raum zwischen Mouse.ScreenPointToArray und Charakter befindet.
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();

        RaycastHit[] hits;
        //var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            //print(hits[i].transform.name);
            if (hit.transform.tag == "Floor")
            {

                spell_destination = hit.point;
            }
        }


    }

    // Target sollte der Punkt

    private void Awake()
    {

    }
    private void FixedUpdate()
    {

        Vector3 direction = spell_destination - transform.position;

        //direction = new Vector3 (direction.normalized.x, bullet_height, direction.normalized.z); 
        myRigidBody.velocity = (direction.normalized * speed);

        if (transform.position == spell_destination)
            Destroy(gameObject);


        //Hey Christoph, na. Ich hab da mal was vorbereitet. Hast du Lust einzustellen, dass die Flugbahn eine Parabel bildet? Also das die "Bullet" sozusagen in einer Bogenlampe fliegt?
        // Idealerweise passiert das über myRigidBody.AddForce(x,y,z), weil damit die Physic des spiels mit einberechnet wird. 
        //Masse, Speed und Bullet_Height lassen sich manuell einstellen im Inspektor.


    }

    private void OnTriggerEnter(Collider collider)
    {

        if (collider.gameObject.tag != "Player" && collider.gameObject.tag == "Enemy")
            Destroy(gameObject);
    }



}