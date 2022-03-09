using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Spell : Talent, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IMoveable, IUseable, IEndDragHandler
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

    /*
    public override void SetDescription(string newDes)
    {
        this.description = newDes;
    }
    */
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

    public bool onCoolDown;
    public float coolDownTimer { get; private set; }



    #region Interface Handling
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x, Input.mousePosition.y), GetDescription,this.gameObject);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandScript.instance.TakeMoveable(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HandScript.instance.Put();
    }
    #endregion



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