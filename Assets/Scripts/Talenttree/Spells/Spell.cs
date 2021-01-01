using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Spell : Talent, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IMoveable //MonoBehaviour,  IUseable // 
{
    [Header("Spell Eigenschaften")]
    //What is the Spells Name?
    [SerializeField]
    private string spellName;

    public string GetSpellName
    {
        get
        {
            return spellName;
        }
    }

    //Description of the Spell
    [SerializeField]
    [TextArea]
    private string description; 
    
    public string GetDescription
    {
        get
        {
            return description;
        }
    }

    //How much CoolDown does this spell have?
    [SerializeField]
    private float spellCoolDown;

    public float GetSpellCoolDown
    {
        get
        {
            return spellCoolDown;
        }
    }

    //Is this an active Spell?
    [SerializeField]
    private bool active;

    public bool onCoolDown;
    private float coolDownTimer;
    /*
    //Die Spell Klasse dient lediglich dem Geschoss selbst.. ggf. sollten Talente ebenfalls Spells sein, bzw. diese beeinflussen.
    [SerializeField]
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


    //Does it need Line of Sight?
    [SerializeField]
    public bool los;

    private Transform target;
    private Vector3 spell_destination;

    */

    //Fragwürdig. Diese Klasse sieht derzeit eher aus wie eine BulletKlasse, aber mal schauen..

    #region Interface Handling
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), GetDescription);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandScript.instance.TakeMoveable(this);
    }
    #endregion

    // Diese Datei wäre auch praktisch für z.B. Pfeile
    // Target sollte stets der erste Enemy sein, welcher sich im 3D Raum zwischen Mouse.ScreenPointToArray und Charakter befindet.
    void Start()
    {
        /*
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
        */

    }


    private void Update()
    {
        if(onCoolDown)
        {
            Image image = GetComponent<Image>();
            image.color = new Color(0, 1, 0, 1);
            coolDownTimer += Time.deltaTime;
            if (coolDownTimer >= spellCoolDown)
            {
                image.color = Color.white;

                coolDownTimer = 0;
                onCoolDown = false;
            }
        }

        /*
        timer += Time.deltaTime;
        if (timer >= 5)
            Destroy(gameObject);
            */
    }

    private void FixedUpdate()
    {
        /*
        float dist = Vector3.Distance(spell_destination, PlayerManager.instance.player.transform.position);

        if ( dist <= range)
        {
            Vector3 direction = spell_destination - transform.position;
            myRigidBody.velocity = (direction.normalized * speed);
        }
        */
    }



    private void OnTriggerEnter(Collider collider)
    {
        /*
        if (collider.gameObject.tag != "Player" && collider.gameObject.tag == "Enemy")
            Destroy(gameObject);

        */
    }

}