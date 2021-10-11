using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Siehe #Lootbox - eine gesonderte Klasse für die Lootbox im Tutorial.
public class TutorialLootBox : MonoBehaviour
{
    SpriteRenderer sprite;
    public Sprite newSprite;
    private bool lootBoxOpened = false;
    private bool tutorialShowed = false;


    [SerializeField]
    private GameObject tutorialUI1;

    [SerializeField]
    private GameObject tutorialUI2;

    [SerializeField]
    private GameObject tutorialUI3;

    public Item firstItem;



    [SerializeField]
    Tutorial tutorialBox;

    void OnEnable()
    {
        PlayerStats.eventLevelUp += ShowNextUI;


    }



    void OnDisable()
    {
        PlayerStats.eventLevelUp -= ShowNextUI;
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
            sprite = this.gameObject.GetComponentInParent<SpriteRenderer>();
            sprite.sprite = newSprite;
            /*
            if(tutorialShowed == false)
            tutorialBox.ShowTutorial(3);
            tutorialShowed = true;
            */

            tutorialUI1.gameObject.SetActive(false);

            tutorialUI2.gameObject.SetActive(true);

            tutorialUI2.GetComponent<Animator>().SetInteger("Arrow", 1);
        }

    }

    private void ShowNextUI()
    {
        if(PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() == 2)
        {
            tutorialUI2.gameObject.SetActive(false);

            tutorialUI3.gameObject.SetActive(true);

            tutorialUI3.GetComponent<Animator>().SetInteger("Arrow", 2);
        }

    }

}
