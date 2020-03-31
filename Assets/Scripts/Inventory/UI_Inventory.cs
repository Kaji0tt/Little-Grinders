using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{
    private Inventory inventory;
    private Transform Int_Inventory;
    private Transform Int_Slot;

    private void Awake()
    {

        Int_Inventory = transform.Find("Inventory");
        Int_Slot = Int_Inventory.Find("Slot");
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnItemListChanged;
        RefreshInventoryItems();
    }

    private void Inventory_OnItemListChanged(object sender, System.EventArgs e)
    {
        RefreshInventoryItems();
    }

    private void RefreshInventoryItems()
    {
        foreach (Transform child in Int_Inventory)
        {
            if (child == Int_Slot) continue;
            Destroy(child.gameObject);
        }
        int x = 0;
        int y = 0;
        float SlotCellSize = 50f;
        foreach (Item item in inventory.GetItemList())
        {

            RectTransform SlotRectTransform = Instantiate(Int_Slot, Int_Inventory).GetComponent<RectTransform>();
            SlotRectTransform.gameObject.SetActive(true);
            SlotRectTransform.anchoredPosition = new Vector2(x * SlotCellSize, y * SlotCellSize);

            Image image = SlotRectTransform.Find("image").GetComponent<Image>();
            image.sprite = item.GetSprite();
            print(image.sprite);
            x++;
            if (x > 5)
            {
                x = 0;
                y++;
            }

        }
    }
}
