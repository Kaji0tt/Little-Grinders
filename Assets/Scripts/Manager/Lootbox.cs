using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Erster Versuch eines Interactables in der Welt
public class Lootbox : MonoBehaviour
{
    //Standard Sprite
    SpriteRenderer sprite;

    //Geöffneter / Neuer Zustand des Sprites
    public Sprite newSprite;

    //Abfrage für eine geöffnete Truhe
    private bool lootBoxOpened = false;


    private void OnTriggerStay(Collider collider)
    {
        //Falls Q gedrückt wird, während der Collider des Spielers mit dem des Objektes auf dem dieses Skript liegt kollidiert, und die Truhe noch nicht geöffnet wurde
        if (Input.GetKeyDown(KeyCode.Q) && collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && lootBoxOpened == false)
        {
            //Ändere den entsprechenden Bool
            lootBoxOpened = true;

            //Generiere Loot
            ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);

            //Ändere das Sprite entsprechend
            LootBoxOpenedSprite();
        }
    }

    private void LootBoxOpenedSprite()
    {
        if (lootBoxOpened)
        {
            sprite = this.gameObject.GetComponent<SpriteRenderer>();
            sprite.sprite = newSprite;
        }

    }
}
