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


    // Wenn Kopf ausgerüstet, alles erhält Kopf außer Schuhe -> Kopf Print wird nur 1x wieder gegeben. Es scheint so, als seien alle anderen Slots zum Kopf.GameEvent triggered.
    // Wenn Brust ausgerüstet, Null Error
    // Wenn Waffe ausgerüstet, alles funktioniert richtig
    // Wenn Schuhe ausgerüstet, alles funktioniert richtig

    public static Item schuhe_Item;
    private Inventory inventory;
    public IsometricPlayer isometricPlayer;


    //Das Problem hinter Int_slotBtn ist, dass es ein Objekt ist, dessen es mehrere Instanzen gibt. OnSceneLoad weiß nicht, um welche Instanz es sich handelt.
    //Deshalb darf int_slotBtn nicht gecalled werden, wenn es sich um das Laden von PlayerData handelt.
    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipSchuhe += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
    }


    public void equip(Item item)
    {
        if (schuhe_Item == null)
        {
            schuhe_Item = item;

            ItemSave.equippedItems.Add(item);

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

        
        inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;

        inventory.AddItem(schuhe_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        isometricPlayer.Dequip(schuhe_Item);

        ItemSave.equippedItems.Remove(schuhe_Item);

        schuhe_Item = null;

        int_slotBtn.storedItem = null;

    }


    public void TaskOnClick()
    {
        if (schuhe_Item != null)
            Dequip();
    }

    public void LoadItem(Item item)
    {
        schuhe_Item = item;

        ItemSave.equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

    }
}
