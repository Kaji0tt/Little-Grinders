using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steinwurf_Bullet : MonoBehaviour
{

    public Rigidbody bullet_rigidbody;

    public Steinwurf steinwurf;

    public Vector3 destination;

    public float speed;

    public Vector3 player_position;

    private float timer;

    public float damage { get; private set; }

    public float range { get; private set; }

    void Start()
    {
        bullet_rigidbody = GetComponent<Rigidbody>();

        RaycastHit[] hits;

        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            //print(hits[i].transform.name);
            if (hit.transform.tag == "Enemy")
                destination = hit.point;

            else if (hit.transform.tag == "Floor")
                destination = hit.point;
        }
    }


    private void Update()
    {

        timer += Time.deltaTime;
        if (timer >= 5)
            Destroy(gameObject);
    }
    void FixedUpdate()
    {


            Vector3 direction = destination - transform.position;
            bullet_rigidbody.velocity = (direction.normalized * speed);


    }

    public void InstantiateMe(Steinwurf steinwurf, Vector3 spell_destination)
    {
        destination = spell_destination;
        this.steinwurf = steinwurf;
    }

    private void OnTriggerEnter(Collider collider)
    {
        
        if (collider.gameObject.tag != "Player" && collider.gameObject.tag == "Enemy")
            Destroy(gameObject);

        
    }
}
