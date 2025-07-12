using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



//Int_SlotBtn sollte mit IMoveable und IUseable ebenfalls arbeiten.
public class Int_SlotBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    public ItemInstance storedItem;
    public ItemType slotType; // z.B. "Kopf", "Brust", "Beine", "Schuhe", "Weapon"

    public Image slotImage; // Optional: für das UI-Icon
    private bool isEquipped = false;

    private void Awake()
    {
        // Falls du ein Image für das Slot-Icon hast
        slotImage = GetComponent<Image>();
    }



    public void StoreItem(ItemInstance item)
    {
        storedItem = item;
        UpdateSlotVisual();

        // Speziallogik für Waffen
        if (slotType == ItemType.Weapon && item != null)
        {
            // Setze das Waffen-Sprite im IsometricRenderer
            var isoRenderer = PlayerManager.instance.player.GetComponent<IsometricRenderer>();
            if (isoRenderer != null && isoRenderer.weaponAnimator != null)
            {
                var weaponSprite = isoRenderer.weaponAnimator.GetComponent<SpriteRenderer>();
                if (weaponSprite != null)
                    weaponSprite.sprite = item.icon;
            }
            // Fernkampf-Flag setzen
            PlayerManager.instance.player.rangedWeapon = item.RangedWeapon;
        }

    }

    public void ClearSlot()
    {
        storedItem = null;
        UpdateSlotVisual();
        isEquipped = false;

        // Speziallogik für Waffen: Sprite entfernen
        if (slotType == ItemType.Weapon)
        {
            var isoRenderer = PlayerManager.instance.player.GetComponent<IsometricRenderer>();
            if (isoRenderer != null && isoRenderer.weaponAnimator != null)
            {
                var weaponSprite = isoRenderer.weaponAnimator.GetComponent<SpriteRenderer>();
                if (weaponSprite != null)
                    weaponSprite.sprite = Resources.Load<Sprite>("Blank_Icon");
            }

            PlayerManager.instance.player.rangedWeapon = false;
        }
    }

    private void UpdateSlotVisual()
    {
        if (slotImage != null)
        {
            slotImage.sprite = storedItem != null ? storedItem.icon : Resources.Load<Sprite>("Blank_Icon");
        }
    }

    public void HideItem()
    {
        UI_Manager.instance.HideTooltip();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Falls der Inventar-Slot ein Item trägt,
        if (storedItem != null)

        UI_Manager.instance.ShowItemTooltip(new Vector2(transform.position.x, transform.position.y), storedItem);

        else
        {
            UI_Manager.instance.ShowTooltip("Du findest sicher noch einen Gegenstand für diese Stelle!");
        }


    }

    public void OnPointerExit(PointerEventData eventData)
    {

        HideItem();
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        HandScript.instance.Put();
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandScript.instance.TakeMoveable(storedItem);
    }

    public void SetItem(ItemInstance item, Inventory inventory)
    {
        // Wenn bereits ein Item im Slot liegt, lege es ins Inventar zurück
        if (storedItem != null && slotType != ItemType.None && inventory != null)
        {
            inventory.AddItem(storedItem, 1);
        }
        StoreItem(item);
        isEquipped = true;
    }
}
