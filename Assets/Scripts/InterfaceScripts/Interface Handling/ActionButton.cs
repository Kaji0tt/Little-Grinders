using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler
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

            cdText.text = (MyUseable.GetCooldown() - MyUseable.CooldownTimer()).ToString("F1");
            

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

    public void LoadUseable(IUseable useable, IMoveable moveable)
    {
        this.MyUseable = useable;

        MyButton.image.sprite = moveable.icon;

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
            HandScript.instance.MyMoveable = null;
        }
    }
}
