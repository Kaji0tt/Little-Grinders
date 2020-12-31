using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Heilung : Talent, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IUseable
                                                                                                              //Skill
                                                                                                              // -> In Abhängigkeit davon, ob es sich bei dem 
                                                                                                              //Talent um eine Aktive Fähigkeit handelt, 
                                                                                                              //sollt es ebenfalls von Spells geerbt werden.
{
    [SerializeField]
    string description;


    [SerializeField]
    private string spellName;






    /*
    public override bool Click()
    {
        if (base.Click())
        {
            // Give the ability

            return true;
        }

        return false;
    }
    */
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), description);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandScript.instance.TakeMoveable(this);
    }

    public void Use()
    {

        //Am Besten bastelst du eine neue Spell Klasse als Scriptable Object?
        print("spell benutzt.");
    }


}
