using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Int_SlotBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IDropHandler, IPointerClickHandler
{
    public int slotIndex;
    public ItemType slotType; // None = Inventar, sonst Equip-Slot

    [SerializeField]
    public Image slotImage;
    private Inventory inventory;

    // Lokale ItemInstance für Equip-Slots
    private ItemInstance equippedItem;
    
    // Flag um doppelte Click-Events zu verhindern (pro Slot-Instanz)
    private bool isProcessingClickOnThisSlot = false;
    
    // Zeitstempel für temporäres Click-Blocking (verhindert asynchrone Event-Queue Probleme)
    private float blockClicksUntilTime = 0f;

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

        // Für Equip-Slots: Item lokal speichern und GameEvents triggern
        if (slotType != ItemType.None)
        {
            equippedItem = item;
            
            // Triggere GameEvents für Equipment-Änderungen
            if (item != null && GameEvents.Instance != null)
            {
                GameEvents.Instance.EquipChanged(item);
            }
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

        // KEIN automatisches Tooltip-Anzeigen hier!
        // Tooltip wird nur bei OnPointerEnter angezeigt
    }

    public void ClearSlot()
    {
        StoreItem(null);
    }
    
    // Blockiere Clicks für kurze Zeit (verhindert Event-Queue Probleme)
    public void BlockClicksTemporarily()
    {
        blockClicksUntilTime = Time.time + 0.2f; // 200ms Blockade
        Debug.Log($"[Int_SlotBtn] Slot {slotIndex} - Clicks blockiert bis {blockClicksUntilTime:F3}");
    }

    // Neuer Event-Handler für Mausklicks (Links + Rechts)
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[Int_SlotBtn] OnPointerClick AUFGERUFEN! Slot {slotIndex}, Button: {eventData.button}, SlotType: {slotType}");
        
        // Rechtsklick = Item auf den Boden werfen
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var item = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);
            
            if (item != null)
            {
                // Item aus Slot entfernen
                if (slotType == ItemType.None)
                {
                    // Aus Inventar entfernen
                    inventory.RemoveItemAtIndex(slotIndex);
                    StoreItem(null);
                    Debug.Log($"[Int_SlotBtn] Item '{item.GetName()}' aus Inventar-Slot {slotIndex} entfernt");
                }
                else
                {
                    // Aus Equip-Slot entfernen (Unequip)
                    item.Unequip(PlayerManager.instance.player.playerStats);
                    StoreItem(null);
                    Debug.Log($"[Int_SlotBtn] Item '{item.GetName()}' aus Equip-Slot abgerüstet");
                }

                // Item vor dem Spieler auf den Boden spawnen
                // In isometrischer Perspektive: Verwende die Blickrichtung in der XZ-Ebene
                Vector3 playerPos = PlayerManager.instance.player.transform.position;
                Vector3 forward = PlayerManager.instance.player.transform.forward;
                
                // Projiziere forward auf XZ-Ebene (ignoriere Y) für echte Bodenrichtung
                Vector3 dropDirection = new Vector3(forward.x, 0, forward.z).normalized;
                
                // Kürzere Distanz für isometrische Perspektive (0.8 statt 1.5)
                Vector3 dropPosition = playerPos + dropDirection * 0.8f;
                dropPosition.y = 0.3f; // Leicht über dem Boden
                
                ItemWorld.SpawnItemWorld(dropPosition, item);
                
                Debug.Log($"[Int_SlotBtn] Item '{item.GetName()}' auf den Boden geworfen bei {dropPosition}");
            }
        }
        // Linksklick = Equip/Unequip (alte Logik)
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            PointerClick();
        }
    }

    private void PointerClick()
    {
        // Prüfe zeitbasierte Blockade (für asynchrone Events aus Event-Queue)
        if (Time.time < blockClicksUntilTime)
        {
            Debug.LogWarning($"[Int_SlotBtn] Click auf Slot {slotIndex} ZEITBASIERT blockiert! (Time: {Time.time:F3} < {blockClicksUntilTime:F3})");
            return;
        }
        
        // Verhindere doppelte Click-Events auf diesem spezifischen Slot
        if (isProcessingClickOnThisSlot)
        {
            Debug.LogWarning($"[Int_SlotBtn] Click auf Slot {slotIndex} blockiert - bereits ein Click in Verarbeitung!");
            return;
        }
        
        isProcessingClickOnThisSlot = true;
        
        try
        {
            // Für Equip-Slots: Lokale ItemInstance verwenden
            var item = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);

            Debug.Log($"[Int_SlotBtn] PointerClick auf Slot {slotIndex} ({slotType}), Item: {(item != null ? item.GetName() : "leer")}");
            
            // WICHTIG: Nur aus INVENTAR-Slots können Items ausgerüstet werden
            // Equip-Slots sollten Items nur abrüsten (ins Inventar zurücklegen)
            if (slotType == ItemType.None && item != null)
            {
                // Inventar → Equip
                var allSlots = FindObjectsOfType<Int_SlotBtn>();
                var equipSlot = allSlots
                        .FirstOrDefault(slot => slot.slotType == item.itemType && slot.slotType != ItemType.None && slot != this);

                Debug.Log($"[Int_SlotBtn] Suche EquipSlot für ItemType {item.itemType}: {(equipSlot != null ? equipSlot.slotIndex.ToString() : "kein Slot gefunden")}");
                
                if (equipSlot != null)
                {
                    // Prüfe, ob bereits ein Item im Equip-Slot ist
                    ItemInstance oldEquippedItem = equipSlot.equippedItem;

                    if (oldEquippedItem != null)
                    {
                        // Tausche Items: Altes Item abrüsten und ins Inventar legen
                        Debug.Log($"[Int_SlotBtn] Tausche: '{item.GetName()}' aus Slot {slotIndex} mit '{oldEquippedItem.GetName()}' aus Equip-Slot");
                        oldEquippedItem.Unequip(PlayerManager.instance.player.playerStats);
                        UI_Manager.instance.UpdateAbilityForEquippedItem(null, oldEquippedItem.itemType);
                        
                        // Altes Item ins Inventar legen (an die Stelle des neuen Items)
                        inventory.AddItemAtIndex(oldEquippedItem, slotIndex);
                    }
                    else
                    {
                        // Kein Item im Equip-Slot: Nur aus Inventar entfernen
                        inventory.RemoveItemAtIndex(slotIndex);
                    }

                    // Neues Item ausrüsten
                    Debug.Log($"[Int_SlotBtn] Rüste Item '{item.GetName()}' aus Inventar-Slot {slotIndex} in Equip-Slot aus");
                    item.Equip(PlayerManager.instance.player.playerStats);
                    UI_Manager.instance.UpdateAbilityForEquippedItem(item);
                    
                    // Aktualisiere den Equip-Slot visuell
                    equipSlot.StoreItem(item);
                    
                    // Aktualisiere den Inventar-Slot visuell (zeige altes Item oder leeren Slot)
                    StoreItem(oldEquippedItem);
                    
                    // WICHTIG: Aktualisiere Tooltip mit dem neuen Item (getauschtes Item liegt jetzt im Inventar-Slot)
                    if (oldEquippedItem != null)
                    {
                        UI_Manager.instance.ShowItemTooltip(new Vector2(transform.position.x, transform.position.y), oldEquippedItem);
                    }
                    else
                    {
                        UI_Manager.instance.HideTooltip();
                    }
                }
            }
            else if (slotType != ItemType.None && item != null)
            {
                // Equip → Inventar (ersten freien Slot verwenden)
                bool added = inventory.AddItemToFirstFreeSlot(item);
                if (added)
                {
                    Debug.Log($"[Int_SlotBtn] Lege Item '{item.GetName()}' aus Equip-Slot zurück ins Inventar");
                    item.Unequip(PlayerManager.instance.player.playerStats);
                    UI_Manager.instance.UpdateAbilityForEquippedItem(null, item.itemType);
                    StoreItem(null); // Visuell aus Equip-Slot entfernen
                    
                    // WICHTIG: Verstecke Tooltip nach dem Abrüsten
                    UI_Manager.instance.HideTooltip();
                }
                else
                {
                    Debug.LogWarning($"[Int_SlotBtn] Inventar voll - kann Item '{item.GetName()}' nicht abrüsten");
                }
            }
        }
        finally
        {
            // WICHTIG: Flag zurücksetzen, auch wenn ein Fehler auftritt
            isProcessingClickOnThisSlot = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Für Equip-Slots: Lokale ItemInstance verwenden
        var item = slotType != ItemType.None ? equippedItem : inventory.GetItemAtIndex(slotIndex);

        Debug.Log($"[Int_SlotBtn] OnPointerEnter auf Slot {slotIndex} ({slotType}), Item: {(item != null ? item.GetName() : "leer")}");

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
