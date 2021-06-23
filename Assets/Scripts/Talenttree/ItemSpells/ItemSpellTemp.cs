using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ItemSpell", menuName = "Assets/ItemSpell")]
public class ItemSpell
{
    [SerializeField]
    [TextArea]
    private string description;

    private bool onCoolDown;

    public float coolDown;

    private Image image;
}
/*
public class ItemSpellTemp : IMoveable, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IUseable
{
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

    Image image (Item item)
    {
        return item.icon;
    }

    public Sprite icon
    {
        get 
        {
            return item.icon;
        }

    }

    public float

    //If its a Spell, which instantiate Prefabs like Bullets / Fireball, it should be called on Isometric Player with "player.CastSpell(this);"

    void Start()
    {

    }
    public void Use()
    {



    }

    public bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public float CooldownTimer()
    {
        return coolDown;
    }

    public float GetCooldown()
    {
        return CooldownTimer;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

}
*/