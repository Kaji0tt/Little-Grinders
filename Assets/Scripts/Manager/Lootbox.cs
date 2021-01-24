using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootbox : MonoBehaviour
{
    SpriteRenderer sprite;
    public Sprite newSprite;
    private bool lootBoxOpened = false;


    private void OnTriggerStay(Collider collider)
    {
        if (Input.GetKeyDown(KeyCode.Q) && collider.gameObject == PlayerManager.instance.player && lootBoxOpened == false)
        {
            print("got here!");
            lootBoxOpened = true;
            ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);
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
