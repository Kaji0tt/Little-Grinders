using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotSchuhe : MonoBehaviour
{


    // Equipment hat "Ausrüstungsslots"
    // ___.equip
    // 🗸 Equipment.equip setzt Sprite für die entsprechenden Ausrüstungsslots richtig
    // 🗸 Equipment.equip entfernt InventoryItem (inventory.removeItem) -> gereglt über PlayerController.
    // 🗸 Equipment.equip setzt PlayerStat Values entsprechend der ausgerüsteten Items
    // 🗸 Equipment.equip speichert die Items im Ausrüstungsslots -> geregelt über eqList
    // Equipment.equip -> ggf. Deequip

    // 
    // ___.deequip
    // 🗸Equipment.deequip nimmt Item.Item aus Ausrüstungsslot raus
    // 🗸Equipment.deeuip packt das entsprechende Item zurück ins Inventory
    //      Equipment.deeuip setzt PlayerStat Values zurück


    //Aufgabe: Wenn wir ein Item ausrüsten wollen, welches bereits besetzt ist, wird das alte Item verschwinden.
    //Dazu folgender Link: https://youtu.be/d9oLS5hy0zU?t=531

    public Item storedItem;
    private Inventory inventory;
    private GameObject player;
    public IsometricPlayer isometricPlayer;

    private void Start()
    {
        GameEvents.current.equipSchuhe += equip;
    }


    public void equip(Item item)
    {
        storedItem = item;
        GetComponent<Image>().sprite = item.GetSprite;
        player = GameObject.Find("Charakter");
    }

    public void Dequip()
    {
        player = GameObject.Find("Charakter");
        
        inventory = player.GetComponent<IsometricPlayer>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        isometricPlayer.Dequip(storedItem);
        this.storedItem = null;
    }


    public void TaskOnClick()
    {
        Dequip();
    } 

}
