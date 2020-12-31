using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler
{
    //IUseable sollte ich später auch noch hinzufügen für die Potions... außerdem müssen Potions Stackbar sein.
    private IUseable MyUseable;

    public Button MyButton { get; private set; }

    public Sprite icon { get; private set; }

    //public Image image { get; private set; }
    

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
    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateVisual()
    {

        MyButton.image.sprite = HandScript.instance.Put().icon;
        MyButton.image.color = Color.white; 
    }

    public void SetUseable(IUseable useable)
    {
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

            SetUseable(HandScript.instance.MyMoveable as IUseable);
            HandScript.instance.MyMoveable = null;
        }
    }
}
