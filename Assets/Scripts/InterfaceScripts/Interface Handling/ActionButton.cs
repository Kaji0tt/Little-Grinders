using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * Moini! Ich bin aktuell dabei meine AbilityTalent Klasse aufzuräumen, bzw. redundant zu machen. Dabei ist mir etwas aufgefallen:
Mein Handyscript, bzw. die EventSystems von Unity machen quatsch bzgl. meiner PointerEvent daten. Ich habe gefühlt in jede Klasse alles reingehauen, was sich auf PointerDragEvent kram bezieht. Aber: Am schlausten scheint es mir, wenn das HandScript einzig und allein das IEndDragHandler Event verarbeitet und alle Sachen, die gezogen werden sollen sich ausschließlich mit IDragHandler zum aufnehmen des Drags beschäftigen.

Kannst du das bitte so formulieren, dass das Handscript schaut, ob die Maus aktuell über einem SpielObjekt im Interface hovert, dass ein IUseable bestitzt, und wenn ja, dann soll bitte 
*/
public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    public IUseable MyUseable { get; private set; }
    public IMoveable MyMoveable { get; private set; }

    public Button MyButton { get; private set; }
    public Sprite icon { get; private set; }
    public Image image { get; private set; }

    private string itemName;

    public GameObject cdButton;
    private Text cdText;
    private Inventory playerInventory;

    void Start()
    {
        MyButton = GetComponent<Button>();
        MyButton.image = GetComponent<Image>();
        MyButton.onClick.AddListener(OnClick);
        cdText = GetComponentInChildren<Text>();
        

    }

    public void OnClick()
    {

        // Überprüfe, ob es sich um ein Item handelt
        if (MyUseable != null && MyUseable is ItemInstance)
        {
            // Hole den Namen des IUseable und überprüfe die Verfügbarkeit im Inventar
            string itemName = MyUseable.GetName();

            // Finde das Item im Inventar basierend auf dem Namen
            ItemInstance itemInInventory = FindItemInInventory(itemName);

            if (itemInInventory != null)
            {
                int itemAmount = PlayerManager.instance.player.inventory.GetItemAmount(itemInInventory);

                if (itemAmount > 0)
                {
                    MyUseable.Use(); // Nur verwenden, wenn noch Aufladungen vorhanden sind
                }
                else
                {
                    // Optional: Deaktiviere den Button, wenn das Item leer ist
                    Debug.Log("Item " + itemName + " is out of stock. 2");
                    MyButton.image.color = Color.gray; // Visuelles Feedback
                }
            }
            else
            {
                MyButton.image.color = Color.black; // Visuelles Feedback
                Debug.Log("Item " + itemName + " is out of stock. 1");
            }


        }
        else if (MyUseable != null)
        {

            MyUseable.Use(); // Nur verwenden, wenn es sich nicht um ein aktives Item handelt.
        }

        else
        {
            Debug.Log("No Spell on current Action Button! ");
        }

    }

    private ItemInstance FindItemInInventory(string itemName)
    {
        // Durchlaufe alle Items im Inventar und suche nach dem Item anhand des Namens
        foreach (ItemInstance item in PlayerManager.instance.player.inventory.GetItemList())
        {
            if (item.ItemName == itemName)
            {
                return item;
            }
        }

        // Auch Consumables durchsuchen (falls sie im Dictionary sind)
        foreach (var kvp in PlayerManager.instance.player.inventory.GetConsumableDict())
        {
            string consumableItemID = kvp.Key;
            ItemInstance consumableItem = new ItemInstance(ItemDatabase.GetItemByID(consumableItemID));

            if (consumableItem.ItemName == itemName)
            {
                return consumableItem;
            }
        }

        // Wenn das Item nicht gefunden wurde, gib null zurück
        return null;
    }

    void Update()
    {
        // Überprüfe, ob es sich um ein Item handelt
        if (MyUseable != null && MyUseable is ItemInstance)
        {
            // Hole den Namen des Items und prüfe die Menge im Inventar
            string itemName = MyUseable.GetName();
            ItemInstance itemInInventory = FindItemInInventory(itemName);

            if(itemInInventory != null)
            {
                int itemAmount = PlayerManager.instance.player.inventory.GetItemAmount(itemInInventory);
                if (itemAmount > 0)
                {

                    // Wenn keine Aufladungen mehr vorhanden sind, Button ausgrauen
                    MyButton.image.color = Color.white;
                    cdText.CrossFadeAlpha(1, 1, true);
                    cdText.text = itemAmount.ToString();
                    cdText.color = Color.white;

                }
                else
                {
                    cdText.CrossFadeAlpha(0, 1, true);
                    MyButton.image.color = Color.gray;
                                               //MyUseable = null; // Entferne die Verwendung
                }
            }

            else
            {
                cdText.CrossFadeAlpha(0, 1, true);
                MyButton.image.color = Color.gray; // Item ist aufgebraucht
                                                   //MyUseable = null; // Entferne die Verwendung
            }

        }
        else if (MyUseable != null) // Cooldown- und Aktivierungslogik für Fähigkeiten
        {
            // Prüfe, ob die Fähigkeit auf Cooldown ist
            if (MyUseable.IsOnCooldown())
            {
                cdText.CrossFadeAlpha(1, 1, true);
                cdText.color = Color.white;
                MyButton.image.color = Color.grey;
                cdButton.SetActive(true);
                cdText.text = MyUseable.CooldownTimer().ToString("F1");
            }
            else
            {
                cdText.CrossFadeAlpha(0, 1, true);
                MyButton.image.color = Color.white;
                cdButton.SetActive(false);
            }

            // Zeige, ob die Fähigkeit aktiv ist
            if (MyUseable.IsActive())
            {
                MyButton.image.color = Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time, 1));
            }
        }
        else
        {
            // Wenn MyUseable null ist (kein Item oder keine Fähigkeit)
            cdText.CrossFadeAlpha(0, 1, true);
        }
    }
    public void UpdateVisual()
    {
        if (HandScript.instance.MyMoveable != null)
        {
            MyButton.image.sprite = HandScript.instance.MyMoveable.icon;
            MyButton.image.color = Color.white;
        }
    }

    public void SetUseable(IUseable useable)
    {
        this.MyUseable = useable;
        cdButton.SetActive(true);
        cdButton.GetComponent<Text>().text = useable.GetName();
        playerInventory = PlayerManager.instance.player.inventory;
        UpdateVisual();


    }

    
    public void OnPointerEnter(PointerEventData eventData)
    {
        /*
        if (HandScript.instance.MyMoveable != null && HandScript.instance.MyMoveable is IUseable)
        {
            SetUseable(HandScript.instance.MyMoveable as IUseable);
            MyMoveable = HandScript.instance.Put();
        }
        */
    }
    

    public void OnPointerExit(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (MyMoveable != null)
        {
            HandScript.instance.TakeMoveable(MyMoveable);
        }
    }



    /*
    public void OnEndDrag(PointerEventData eventData)
    {
        
        if (HandScript.instance.MyMoveable != null)
        {
            SetUseable(HandScript.instance.MyMoveable as IUseable);
            HandScript.instance.Put();
        }
        
    }
    */

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("ActionButton OnEndDrag triggered.");
        if (HandScript.instance.MyMoveable != null)
        {
            SetUseable(HandScript.instance.MyUseable);

            HandScript.instance.Put();

            icon = MyMoveable.icon;
        }
    }

    /// Laden von Spielerdaten:

    public void LoadAbilityUseable(Ability ability)
    {
        MyUseable = ability;
        MyMoveable = ability;
        MyButton = GetComponent<Button>();
        playerInventory = PlayerManager.instance.player.inventory;

        MyButton.image.sprite = ability.icon; // Setze das Icon des Spells
        MyButton.image.color = Color.white;
    }

    public void LoadItemUseable(ItemInstance item)
    {
        MyUseable = item; // Stelle sicher, dass ItemInstance IUseable implementiert
        MyMoveable = item; // Stelle sicher, dass ItemInstance IMoveable implementiert
        MyButton = GetComponent<Button>();
        playerInventory = PlayerManager.instance.player.inventory;

        MyButton.image.sprite = item.icon; // Setze das Icon des Items
        MyButton.image.color = Color.white;
    }


}
