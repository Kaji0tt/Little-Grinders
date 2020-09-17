using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotWeapon : MonoBehaviour
{

    private Item storedItem;
    private Inventory inventory;
    private GameObject player, weaponAnim;
    public IsometricPlayer isometricPlayer;

    private void Start()
    {
        GameEvents.current.equipWeapon += equip;
        storedItem = null;
    }


    public void equip(Item item)
    {
        if (storedItem == null)
        {
            storedItem = item;
            GetComponent<Image>().sprite = item.GetSprite;
            weaponAnim = GameObject.Find("WeaponAnimParent");
            weaponAnim.GetComponent<SpriteRenderer>().sprite = item.GetSprite;
            isometricPlayer.Range = item.Range;
            player = GameObject.Find("Charakter");
        }
        else
        {
            Dequip();
            storedItem = item;
            GetComponent<Image>().sprite = item.GetSprite;
            weaponAnim = GameObject.Find("WeaponAnimParent");
            weaponAnim.GetComponent<SpriteRenderer>().sprite = item.GetSprite;
            player = GameObject.Find("Charakter");
        }

    }


    public void Dequip()
    {
        player = GameObject.Find("Charakter");

        inventory = player.GetComponent<IsometricPlayer>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        isometricPlayer.Dequip(storedItem);
        weaponAnim.GetComponent<SpriteRenderer>().sprite = null;
        this.storedItem = null;
    }


    public void TaskOnClick()
    {
        Dequip();
    }

}
