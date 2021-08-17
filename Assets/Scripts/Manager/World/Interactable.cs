using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Erster Versuch eines Interactables in der Welt
public class Interactable : MonoBehaviour
{
    //Standard Sprite
    SpriteRenderer sprite;

    //Geöffneter / Neuer Zustand des Sprites
    public Sprite newSprite;

    //Abfrage für eine geöffnete Truhe
    private bool interactableUsed = false;


    private void OnTriggerStay(Collider collider)
    {
        //Falls Q gedrückt wird, während der Collider des Spielers mit dem des Objektes auf dem dieses Skript liegt kollidiert, und die Truhe noch nicht geöffnet wurde
        if (Input.GetKeyDown(KeyCode.Q) && collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && interactableUsed == false)
        {
            //Ändere den entsprechenden Bool
            interactableUsed = true;

            //Was passiert wenn das Interactable benutzt wurde?
            Use();


            //Ändere das Sprite entsprechend
            LootBoxOpenedSprite();
        }
    }

    public virtual void Use()
    {
        //Generiere den Loot
        //ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);
    }

    private void LootBoxOpenedSprite()
    {
        if (interactableUsed && newSprite != null)
        {
            sprite = this.gameObject.GetComponentInParent<SpriteRenderer>();
            sprite.sprite = newSprite;

        }

    }
}
