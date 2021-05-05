using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Siehe #Lootbox - eine gesonderte Klasse für die Lootbox im Tutorial.
public class TutorialLootBox : MonoBehaviour
{
    SpriteRenderer sprite;
    public Sprite newSprite;
    private bool lootBoxOpened = false;
    private bool tutorialShowed = false;

    public Item firstItem;



    [SerializeField]
    Tutorial tutorialBox;

    private void Update()
    {
        Debug.Log(PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>());
    }

    private void OnTriggerStay(Collider collider)
    {
        if (Input.GetKeyDown(KeyCode.Q) && collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && lootBoxOpened == false)
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
