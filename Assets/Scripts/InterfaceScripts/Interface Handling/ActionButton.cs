using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler //IEndDragHandler
{
    //IUseable sollte ich später auch noch hinzufügen für die Potions... außerdem müssen Potions Stackbar sein.
    public IUseable MyUseable { get; private set; }

    private IMoveable MyMoveable;

    public Button MyButton { get; private set; }

    public Sprite icon { get; private set; }

    public Image image { get; private set; }

    public GameObject cdButton;
    private Text cdText;



    void Start()
    {
        MyButton = GetComponent<Button>();
        MyButton.image = GetComponent<Image>();
        MyButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {

        if (MyUseable != null)
        {
            MyUseable.Use();
        }

    }

    void Update()
    {
        if (MyUseable != null && MyUseable.IsOnCooldown())
        {

            MyButton.image.color = Color.grey;

            cdButton.SetActive(true);

            cdText = cdButton.GetComponent<Text>();

            cdText.text = (MyUseable.CooldownTimer()).ToString("F1");


        }
        else if (MyUseable != null && !MyUseable.IsOnCooldown())
        {

            MyButton.image.color = Color.white;

            cdButton.SetActive(false);

        }


    }

    public void UpdateVisual()
    {

        MyButton.image.sprite = HandScript.instance.Put().icon;

        MyButton.image.color = Color.white;
    }

    //Wurde eingefügt um ggf. ActionBars zu speichern.
    /*
    public void SetUseable(IMoveable moveable)
    {
        this.MyMoveable = moveable;

        UpdateVisual();
    }
    */
    public void SetUseable(IUseable useable)
    {
        //this.MyMoveable = moveable;

        this.MyUseable = useable;

        UpdateVisual();
    }

    public void LoadSpellUseable(Spell spell)
    {
        //this.MyUseable = useable;

        MyUseable = spell;

        MyMoveable = spell;

        MyButton = GetComponent<Button>();

        MyButton.image.sprite = spell.icon;

        MyButton.image.color = Color.white;

    }

    internal void LoadItemUseable(Item item)
    {
        Inventory inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;

        ItemInstance interfaceItem = inventory.itemList.FirstOrDefault(x => x.ItemID == item.ItemID);

        MyUseable = interfaceItem;

        MyMoveable = interfaceItem;

        MyButton = GetComponent<Button>();

        MyButton.image.sprite = interfaceItem.icon;

        MyButton.image.color = Color.white;
    }

    //On POinter ClickHandler ist wohl lediglich für das Spellbook wichtig, welches wir über den TalentTree umgangen haben.
    /*public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is IUseable)
            {
                UI_Manager.instance.SetUseable(HandScript.instance.MyMoveable as IUseable);
            }
        }
    }*/

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HandScript.instance.MyMoveable != null)
        {
            SetUseable(HandScript.instance.MyMoveable as IUseable);
            MyMoveable = HandScript.instance.Put();
            //HandScript.instance.MyMoveable = null;
        }

        /*
        if(Input.GetKey(KeyCode.Mouse1))
        {
            MyMoveable = null;
            MyUseable = null;
            image = null;
        }
        */

    }

}
