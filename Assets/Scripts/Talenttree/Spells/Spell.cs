using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Spell : Talent, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IMoveable, IUseable //MonoBehaviour ,// 
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

    public void SetDescription(string newDes)
    {
        this.description = newDes;
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

    

    //Spell IMoveable.spell => this;



    //Is this an active Spell?
    [SerializeField]
    private bool active;

    public bool onCoolDown;
    public float coolDownTimer { get; private set; }



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
            image.color = Color.grey;
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

    public virtual void Use()
    {
        throw new NotImplementedException();
    }

    public virtual bool IsOnCooldown()
    {
        throw new NotImplementedException();
    }

    public virtual float GetCooldown()
    {
        throw new NotImplementedException();
    }

    public virtual float CooldownTimer()
    {
        throw new NotImplementedException();
    }
}