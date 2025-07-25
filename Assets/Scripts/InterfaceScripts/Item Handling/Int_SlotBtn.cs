using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Int_SlotBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IDropHandler
{
    public int slotIndex;
    public ItemType slotType; // None = Inventar, sonst Equip-Slot

    [SerializeField]
    public Image slotImage;
    private Inventory inventory;

    // Lokale ItemInstance für Equip-Slots
    private ItemInstance equippedItem;

    private void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    private void Start()
    {
        inventory = UI_Inventory.instance.inventory;
    }

    public void StoreItem(ItemInstance item)
    {
        // UI-Visualisierung
        slotImage.sprite = item != null ? item.icon : Resources.Load<Sprite>("Blank_Icon");

        // Für Equip-Slots: Item lokal speichern
        if (slotType != ItemType.None)
        {
            equippedItem = item;
        }

        // Speziallogik für Waffen-Slot
        if (slotType == ItemType.Weapon)
        {
            var isoRenderer = PlayerManager.instance.player.GetComponent<IsometricRenderer>();
            if (isoRenderer != null && isoRenderer.weaponAnimator != null)
            {
                var weaponSprite = isoRenderer.weaponAnimator.GetComponent<SpriteRenderer>();
                weaponSprite.sprite = item != null ? item.icon : Resources.Load<Sprite>("Blank_Icon");
            }
            PlayerManager.instance.player.rangedWeapon = item != null && item.RangedWeapon;
        }

        // Tooltip
        if (item != null && UI_Manager.instance.tooltip.activeSelf)
            UI_Manager.instance.ShowItemTooltip(new Vector2(transform.position.x, transform.position.y), item);
        else
            UI_Manager.instance.HideTooltip();
    }

    public void ClearSlot()
    {
        StoreItem(null);
    }

    public void PointerClick()
    {
        // Für Equip-Slots: Lokale ItemInstance verwenden
        var item = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);

        Debug.Log($"[Int_SlotBtn] PointerClick auf Slot {slotIndex} ({slotType}), Item: {(item != null ? item.GetName() : "leer")}");
        if (slotType == ItemType.None && item != null)
        {
            // Inventar → Equip
            var allSlots = FindObjectsOfType<Int_SlotBtn>();
            var equipSlot = allSlots
                    .FirstOrDefault(slot => slot.slotType == item.itemType && slot.slotType != ItemType.None && slot != this);

            Debug.Log($"[Int_SlotBtn] Suche EquipSlot für ItemType {item.itemType}: {(equipSlot != null ? equipSlot.slotIndex.ToString() : "kein Slot gefunden")}");
            if (equipSlot != null)
            {
                Debug.Log($"[Int_SlotBtn] Rüste Item '{item.GetName()}' aus Inventar-Slot {slotIndex} in Equip-Slot {equipSlot.slotIndex}");
                EquipOrUnequipItem(item, slotIndex, equipSlot.slotIndex, true);
            }
        }
        else if (slotType != ItemType.None && item != null)
        {
            // Equip → Inventar (ersten freien Slot verwenden)
            bool added = inventory.AddItemToFirstFreeSlot(item);
            if (added)
            {
                Debug.Log($"[Int_SlotBtn] Lege Item '{item.GetName()}' aus Equip-Slot zurück ins Inventar");
                EquipOrUnequipItem(item, slotIndex, -1, false); // -1 da der Slot automatisch gefunden wird
            }
            else
            {
                Debug.LogWarning($"[Int_SlotBtn] Inventar voll - kann Item '{item.GetName()}' nicht abrüsten");
            }
        }
    }

    private void EquipOrUnequipItem(ItemInstance item, int fromSlotIndex, int toSlotIndex, bool isEquip)
    {
        Debug.Log($"[Int_SlotBtn] EquipOrUnequipItem: {(isEquip ? "Equip" : "Unequip")} Item '{item?.GetName()}' von Slot {fromSlotIndex} nach Slot {toSlotIndex}");

        if (isEquip)
        {
            // Suche den passenden Equip-Slot anhand des ItemTypes
            var allSlots = FindObjectsOfType<Int_SlotBtn>();
            var equipSlot = allSlots.FirstOrDefault(slot => slot.slotType == item.itemType && slot.slotType != ItemType.None);

            if (equipSlot == null)
            {
                Debug.LogWarning($"[Int_SlotBtn] Kein Equip-Slot für ItemType {item.itemType} gefunden!");
                return;
            }

            // Prüfe, ob bereits ein Item im Equip-Slot ist (lokal gespeichert)
            ItemInstance oldEquippedItem = equipSlot.equippedItem;

            // Wenn bereits ein Item des gleichen Typs ausgerüstet ist, tausche sie
            if (oldEquippedItem != null)
            {
                Debug.Log($"[Int_SlotBtn] Austausch: Entferne altes Item '{oldEquippedItem.GetName()}' und lege es in Inventar-Slot {fromSlotIndex}");
                oldEquippedItem.Unequip(PlayerManager.instance.player.playerStats);
                
                // WICHTIG: ItemType explizit übergeben!
                UI_Manager.instance.UpdateAbilityForEquippedItem(null, oldEquippedItem.itemType);

                // Altes Item ins Inventar legen (an die Stelle des neuen Items)
                inventory.AddItemAtIndex(oldEquippedItem, fromSlotIndex);
            }
            else
            {
                // Kein Austausch - nur aus Inventar entfernen
                inventory.RemoveItemAtIndex(fromSlotIndex);
            }

            // Neues Item ausrüsten (lokal im Equip-Slot speichern)
            Debug.Log($"[Int_SlotBtn] Ausrüsten: Item '{item.GetName()}' wird in Equip-Slot {equipSlot.slotType} gelegt");
            item.Equip(PlayerManager.instance.player.playerStats);
            UI_Manager.instance.UpdateAbilityForEquippedItem(item);

            // Visuell im Equip-Slot anzeigen (speichert automatisch lokal)
            equipSlot.StoreItem(item);
        }
        else
        {
            // Unequip: Item aus Equip-Slot ins Inventar verschieben
            Debug.Log($"[Int_SlotBtn] Unequip: Item '{item.GetName()}' aus Equip-Slot wird ins Inventar verschoben");
            item.Unequip(PlayerManager.instance.player.playerStats);
            
            // WICHTIG: ItemType explizit übergeben!
            UI_Manager.instance.UpdateAbilityForEquippedItem(null, item.itemType);

            // Item ist bereits ins Inventar gelegt (via AddItemToFirstFreeSlot in PointerClick)
            // Visuell aus Equip-Slot entfernen (löscht automatisch lokale Variable)
            StoreItem(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Für Equip-Slots: Lokale ItemInstance verwenden
        var item = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);

        if (item == null)
        {
            if (slotType == ItemType.None)
                UI_Manager.instance.HideTooltip();
            else
                UI_Manager.instance.ShowTooltip("Du findest bestimmt ein Item, das hier reinpasst.");
        }
        else
        {
            UI_Manager.instance.ShowItemTooltip(new Vector2(transform.position.x, transform.position.y), item);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Für Equip-Slots: Lokale ItemInstance verwenden
        var item = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);

        if (item != null)
        {
            HandScript.instance.TakeMoveable(item);
            HandScript.instance.slotIndex = slotIndex;

            // WICHTIG: Item aus dem ursprünglichen Slot entfernen
            if (slotType == ItemType.None)
            {
                inventory.RemoveItemAtIndex(slotIndex);
            }
            else
            {
                // Für Equip-Slots: Item abrüsten und lokal entfernen
                item.Unequip(PlayerManager.instance.player.playerStats);
                
                // WICHTIG: ItemType explizit übergeben!
                UI_Manager.instance.UpdateAbilityForEquippedItem(null, item.itemType);
                
                StoreItem(null); // Entfernt auch die lokale equippedItem-Variable
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        IMoveable moveable = HandScript.instance.MyMoveable;
        ItemInstance draggedItem = moveable as ItemInstance;
        if (draggedItem == null) return;

        // Für Equip-Slots: Lokale ItemInstance verwenden
        var currentItem = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);

        if (slotType == ItemType.None)
        {
            // Inventar-Slot
            if (currentItem == null)
            {
                // Freier Slot: Item direkt hinzufügen
                inventory.AddItemAtIndex(draggedItem, slotIndex);
                HandScript.instance.Put(); // Item aus Hand entfernen
            }
            else
            {
                // Belegter Slot: Items tauschen
                inventory.AddItemAtIndex(draggedItem, slotIndex);

                // Altes Item zurück zum ursprünglichen Slot
                int originalSlot = HandScript.instance.slotIndex;
                if (originalSlot != slotIndex) // Nur wenn es ein anderer Slot ist
                {
                    inventory.AddItemAtIndex(currentItem, originalSlot);
                }
                HandScript.instance.Put(); // Item aus Hand entfernen
            }
        }
        else if (slotType == draggedItem.itemType)
        {
            // Equip-Slot mit passendem ItemType
            if (currentItem != null)
            {
                // Bereits ausgerüstetes Item: Tauschen
                int originalSlot = HandScript.instance.slotIndex;

                // Altes Item zum ursprünglichen Slot
                if (originalSlot >= 0) // Nur wenn es ein Inventar-Slot war
                {
                    inventory.AddItemAtIndex(currentItem, originalSlot);
                }
                else
                {
                    // Falls es von einem anderen Equip-Slot kam: ins Inventar
                    inventory.AddItemToFirstFreeSlot(currentItem);
                }
            }

            // Neues Item ausrüsten
            draggedItem.Equip(PlayerManager.instance.player.playerStats);
            UI_Manager.instance.UpdateAbilityForEquippedItem(draggedItem);
            StoreItem(draggedItem); // Setzt auch die lokale equippedItem-Variable

            HandScript.instance.Put(); // Item aus Hand entfernen
        }
        else
        {
            // Falscher ItemType für Equip-Slot: Item zurück zum ursprünglichen Slot
            ReturnItemToOriginalSlot(draggedItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    // Hilfsmethode: Item zurück zum ursprünglichen Slot
    private void ReturnItemToOriginalSlot(ItemInstance item)
    {
        int originalSlot = HandScript.instance.slotIndex;

        if (originalSlot >= 0)
        {
            // Zurück ins Inventar
            inventory.AddItemAtIndex(item, originalSlot);
        }
        else
        {
            // Zurück zu einem Equip-Slot oder ins Inventar
            inventory.AddItemToFirstFreeSlot(item);
        }

        HandScript.instance.Put(); // Item aus Hand entfernen
    }
    
    public ItemInstance GetEquippedItem()
    {
        return slotType != ItemType.None ? equippedItem : null;
    }
}
