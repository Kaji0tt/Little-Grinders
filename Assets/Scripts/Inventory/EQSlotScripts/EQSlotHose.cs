using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotHose : MonoBehaviour
{

    public Item storedItem;
    private Inventory inventory;
    private GameObject player;
    public IsometricPlayer isometricPlayer;

    private void Start()
    {
        GameEvents.current.equipHose += equip;
    }

    public void equip(Item item)
    {
        storedItem = item;
        GetComponent<Image>().sprite = item.GetSprite;
        player = GameObject.Find("Charakter");
    }

    public void Dequip()
    {
        player = GameObject.Find("Charakter");

        inventory = player.GetComponent<IsometricPlayer>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        isometricPlayer.Dequip(storedItem);
        this.storedItem = null;
    }


    public void TaskOnClick()
    {
        Dequip();
    }

}
