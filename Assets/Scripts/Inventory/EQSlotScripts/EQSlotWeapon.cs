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
    private CharStats charStats;

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


        //Berechnen der neuen Playerstats Values


        // Pause - Die Stats werden in einem extra Modifier berechnet, wahrscheinlich.
        // Es hängt daran, dass ich nicht die einzelnen Variabeln in der Funktion ItemStats abrufen kann.
        player = GameObject.Find("Charakter");
        charStats = player.GetComponent<CharStats>();
        print(charStats.hp);

        /*
        player = GameObject.Find("Charakter");
        charStats = player.GetComponent<CharStats>();
        print(charStats.hp);

        charStats.hp = charStats.hp + item.ItemStats(item);
        charStats.armor = charStats.armor + item.ItemStats(item);
        charStats.attackPower = charStats.attackPower + item.ItemStats(item);
        charStats.abilityPower = charStats.abilityPower + item.ItemStats(item);
        charStats.attackSpeed = charStats.attackSpeed + item.ItemStats(item);
        charStats.movementSpeed = charStats.movementSpeed + item.ItemStats(item);
        */





    }


    public void dequip()

    {
        print(itemName);
        player = GameObject.Find("Charakter");
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
