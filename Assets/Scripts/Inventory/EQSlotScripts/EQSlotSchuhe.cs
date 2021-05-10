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
    // 🗸 Equipment.equip -> ggf. Deequip

    // 
    // ___.deequip
    // 🗸Equipment.deequip nimmt Item.Item aus Ausrüstungsslot raus
    // 🗸Equipment.deeuip packt das entsprechende Item zurück ins Inventory
    // 🗸Equipment.deeuip setzt PlayerStat Values zurück

    public static ItemInstance schuhe_Item;


    //Das Problem hinter Int_slotBtn ist, dass es ein Objekt ist, dessen es mehrere Instanzen gibt. OnSceneLoad weiß nicht, um welche Instanz es sich handelt.
    //Deshalb darf int_slotBtn nicht gecalled werden, wenn es sich um das Laden von PlayerData handelt.
    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipSchuhe += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
    }


    public void equip(ItemInstance item)
    {
        if (schuhe_Item == null)
        {
            schuhe_Item = item;

            //ItemSave.equippedItems.Add(item);

            int_slotBtn.StoreItem(item);

            GetComponent<Image>().sprite = item.icon;

        }
        else
        {
            Dequip();

            schuhe_Item = item;

            int_slotBtn.storedItem = item;

            GetComponent<Image>().sprite = item.icon;

        }
    }

    public void Dequip()
    {

        
        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(schuhe_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Dequip(schuhe_Item);

        //ItemSave.equippedItems.Remove(schuhe_Item);

        schuhe_Item = null;

        int_slotBtn.storedItem = null;

    }


    public void TaskOnClick()
    {
        if (schuhe_Item != null)
            Dequip();
    }

    public void LoadItem(ItemInstance item)
    {
        schuhe_Item = item;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

    }
}
