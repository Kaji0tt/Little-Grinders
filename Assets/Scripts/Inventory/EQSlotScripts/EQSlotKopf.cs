using UnityEngine;
using UnityEngine.UI;

public class EQSlotKopf : MonoBehaviour
{
    public static ItemInstance kopf_Item;

    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.instance.equipKopf += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
    }


    public void equip(ItemInstance item)
    {
        if (kopf_Item == null)
        {
            kopf_Item = item;

            //ItemSave.equippedItems.Add(item);

            int_slotBtn.StoreItem(item);

            GetComponent<Image>().sprite = item.icon;

        }
        else
        {
            Dequip();

            kopf_Item = item;

            int_slotBtn.storedItem = item;

            GetComponent<Image>().sprite = item.icon;

        }
    }

    public void Dequip()
    {


        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(kopf_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Dequip(kopf_Item);

        //ItemSave.equippedItems.Remove(kopf_Item);

        kopf_Item = null;

        int_slotBtn.storedItem = null;

    }


    public void TaskOnClick()
    {
        if (kopf_Item != null)
            Dequip();
    }

    public void LoadItem(ItemInstance item)
    {
        kopf_Item = item;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

    }

}
