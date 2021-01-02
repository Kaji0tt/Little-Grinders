using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler
{
    //IUseable sollte ich später auch noch hinzufügen für die Potions... außerdem müssen Potions Stackbar sein.
    private IUseable MyUseable;

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
        if (MyMoveable != null && MyMoveable.spell.onCoolDown)
        {
            MyButton.image.color = Color.grey;

            // Das Problem ist, dass der Action-Button nicht weiß um welche Instanz des Spell-Scripts es sich handelt.. glaub ich.
            cdButton.SetActive(true);
            cdText = cdButton.GetComponent<Text>();
            cdText.text = (MyMoveable.spell.GetSpellCoolDown - MyMoveable.spell.coolDownTimer).ToString("F1");
            

        }
        else if (MyMoveable != null && !MyMoveable.spell.onCoolDown)
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

    public void SetUseable(IUseable useable, IMoveable moveable)
    {
        this.MyMoveable = moveable;
        this.MyUseable = useable;

        UpdateVisual();
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
            SetUseable(HandScript.instance.MyMoveable as IUseable, HandScript.instance.MyMoveable);
            HandScript.instance.MyMoveable = null;
        }
    }
}
