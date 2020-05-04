using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotWeapon : MonoBehaviour
{
    private Item storedItem;
    private string itemType;
    private Inventory inventory;
    private GameObject player, weaponAnim;
    private string itemName;

    private void Start()
    {
        GameEvents.current.equipWeapon += equip;
    }

    public void equip(Item item)
    {
        itemName = item.itemName.ToString();
        itemType = item.ItemType(item);
        storedItem = item;
        GetComponent<Image>().sprite = item.GetSprite();
        weaponAnim = GameObject.Find("WeaponAnimParent");
        weaponAnim.GetComponent<SpriteRenderer>().sprite = item.GetSprite();

        print(item.ItemStats(item));

    }

    public void dequip()

    {
        print(itemName);
        player = GameObject.Find("Charakter");
        print("Folgendes Item soll ausgezogen werden:" + storedItem.itemName + " es handelt sich um ein " + storedItem);
        inventory = player.GetComponent<IsometricPlayerMovementController>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        weaponAnim = GameObject.Find("WeaponAnimParent");
        weaponAnim.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Blank_Icon");
        this.storedItem = null;


    }


    public void TaskOnClick()
    {

        dequip();

    }

}
