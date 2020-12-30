using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    private Rigidbody myRigidBody;

    //TravelTime of Spell
    [SerializeField]
    private float speed;

    //Range of the Spell
    [SerializeField]
    public float range;

    //Arc the Spell is flying in
    [SerializeField]
    private float bullet_height; //soll noch eingebaut werden, damit die projectiles in kurven fliegen.


    //Damage of the Spell. Should be scaling with Ability Power later on.
    [SerializeField]
    public float damage;

    // Scaling of AP
    //[SerilizeField]
    //private float scaling;

    //The spell's icon
    [SerializeField]
    private Sprite icon;

    //Does it need Line of Sight?
    [SerializeField]
    private bool los;

    private Transform target;
    private Vector3 spell_destination;
    private float timer;

    // Diese Datei wäre auch praktisch für z.B. Pfeile
    // Target sollte stets der erste Enemy sein, welcher sich im 3D Raum zwischen Mouse.ScreenPointToArray und Charakter befindet.
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();

        RaycastHit[] hits;

        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            //print(hits[i].transform.name);
            if (hit.transform.tag == "Enemy") 
                spell_destination = hit.point;
            
            else if (hit.transform.tag == "Floor")
                spell_destination = hit.point;
        }


    }

    private void Awake()
    {

    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 5)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        float dist = Vector3.Distance(spell_destination, PlayerManager.instance.player.transform.position);

        if ( dist <= range)
        {
            Vector3 direction = spell_destination - transform.position;
            myRigidBody.velocity = (direction.normalized * speed);
        }


        //direction = new Vector3 (direction.normalized.x, bullet_height, direction.normalized.z); 






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