using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotSchmuck : MonoBehaviour
{
    private Item storedItem;
    private string itemType;
    private Inventory inventory;
    private GameObject player;
    private string itemName;
    private PlayerStats playerStats;

    private void Start()
    {
        GameEvents.current.equipSchmuck += equip;
    }

    public void equip(Item item)
    {
        itemName = item.itemName.ToString();
        itemType = item.ItemType(item);
        storedItem = item;
        GetComponent<Image>().sprite = item.GetSprite();

        //Berechnen der neuen Playerstats Values

        player = GameObject.Find("Charakter");
        playerStats = player.GetComponent<PlayerStats>();
        print(playerStats.hp);

        playerStats.hp = playerStats.hp + item.ItemStats(item);
        playerStats.armor = playerStats.armor + item.ItemStats(item);
        playerStats.attackPower = playerStats.attackPower + item.ItemStats(item);
        playerStats.abilityPower = playerStats.abilityPower + item.ItemStats(item);
        playerStats.attackSpeed = playerStats.attackSpeed + item.ItemStats(item);
        playerStats.movementSpeed = playerStats.movementSpeed + item.ItemStats(item);
    }

    public void dequip()

    {
        print(itemName);
        player = GameObject.Find("Charakter");
        print("Folgendes Item soll ausgezogen werden:" + storedItem.itemName + " es handelt sich um ein " + storedItem);
        inventory = player.GetComponent<IsometricPlayerMovementController>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        this.storedItem = null;


    }


    public void TaskOnClick()
    {

        dequip();

    }
}
