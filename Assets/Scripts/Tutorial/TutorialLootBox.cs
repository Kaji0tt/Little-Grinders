using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLootBox : MonoBehaviour
{
    SpriteRenderer sprite;
    public Sprite newSprite;
    private bool lootBoxOpened = false;
    private bool tutorialShowed = false;

    public Item firstItem;



    [SerializeField]
    Tutorial tutorialBox;

    private void OnTriggerStay(Collider collider)
    {
        if (Input.GetKeyDown(KeyCode.Q) && collider.gameObject == PlayerColliderManager.instance.player_collider && lootBoxOpened == false)
        {
            lootBoxOpened = true;
            Vector3 spawnPos = new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y, gameObject.transform.position.z);
            ItemWorld.SpawnItemWorld(spawnPos, new ItemInstance(firstItem));

            LootBoxOpenedSprite();
        }
    }

    private void LootBoxOpenedSprite()
    {
        if (lootBoxOpened)
        {
            sprite = this.gameObject.GetComponent<SpriteRenderer>();
            sprite.sprite = newSprite;
            if(tutorialShowed == false)
            tutorialBox.ShowTutorial(3);
            tutorialShowed = true;
        }

    }
}
