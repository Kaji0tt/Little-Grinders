using UnityEngine;
using UnityEngine.UI;

public abstract class EQSlotBase : MonoBehaviour, IEquipableSlot
{
    private ItemInstance equippedItem;
    public EquipmentSlotType slotType;

    protected Int_SlotBtn int_slotBtn;

    // Eventbindung beim Aktivieren des Slots
    private void OnEnable()
    {
        if (GameEvents.Instance != null)
        {
            BindToGameEvent();
        }
    }

    // Evententbindung beim Deaktivieren des Slots
    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            UnbindFromGameEvent();
        }
    }

    protected abstract void BindToGameEvent();
    protected abstract void UnbindFromGameEvent();

    public virtual void Equip(ItemInstance item)
    {
        if (equippedItem != null)
        {
            Dequip(); // Altes Item zuerst ablegen
        }

        equippedItem = item;
        int_slotBtn = GetComponent<Int_SlotBtn>();

        int_slotBtn.StoreItem(item);
        GetComponent<Image>().sprite = item.icon;

        // Weitere Logik bei Ausrüsten, z.B. Statuswerte erhöhen
    }

    public virtual void Dequip()
    {
        if (equippedItem == null) return;

        //Debug.Log(equippedItem.ItemName);
        //Debug.Log(PlayerManager.instance.player.gameObject.name);
        PlayerManager.instance.player.Inventory.AddItem(equippedItem);
        PlayerManager.instance.player.Dequip(equippedItem);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        equippedItem = null;
        int_slotBtn.storedItem = null;
    }

    public void TaskOnClick()
    {
        if (equippedItem != null)
        {
            Dequip();
        }
    }

    public virtual void LoadItem(ItemInstance item)
    {
        equippedItem = item;
        PlayerManager.instance.player.equippedItems.Add(item);
        int_slotBtn.storedItem = item;
        GetComponent<Image>().sprite = item.icon;
    }
}