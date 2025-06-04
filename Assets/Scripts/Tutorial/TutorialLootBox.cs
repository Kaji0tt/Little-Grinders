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

    private void Start()
    {
        StartCoroutine(ShowIntroLogs());
    }

    private IEnumerator ShowIntroLogs()
    {
        yield return new WaitUntil(() => LogScript.instance != null); // Warten bis LogScript bereit ist

        LogScript.instance.ShowLog("Hey Adventurer!");
        yield return new WaitForSeconds(2f);

        LogScript.instance.ShowLog("Welcome to the world of Little Grinders!");
        yield return new WaitForSeconds(3f);

        LogScript.instance.ShowLog("Use WASD to move around and open the Chest to prepare for Battle!");
        yield return new WaitForSeconds(5f);
    }

    void OnDisable()
    {
        PlayerStats.eventLevelUp -= ShowNextUI;
    }


    private void OnTriggerStay(Collider collider)
    {
        if (Input.GetKeyDown(KeyCode.F) && collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && lootBoxOpened == false)
        {
            lootBoxOpened = true;

            Vector3 spawnPos = new Vector3(PlayerManager.instance.player.transform.position.x + .5f, gameObject.transform.position.y, PlayerManager.instance.player.transform.position.z + .9f);

            ItemWorld.SpawnItemWorld(spawnPos, new ItemInstance(firstItem));

            LootBoxOpenedSprite();


        }
    }

    private void LootBoxOpenedSprite()
    {
        if (lootBoxOpened)
        {
            sprite = GetComponentInParent<SpriteRenderer>();
            sprite.sprite = newSprite;

            tutorialUI1.gameObject.SetActive(false);

            StartCoroutine(PostLootLog()); // NEU

            // ...
        }
    }

    private IEnumerator PostLootLog()
    {
        LogScript.instance.ShowLog("Great! Get the sword!");

        yield return new WaitForSeconds(3.5f); // oder automatisch
        
        LogScript.instance.ShowLog("Open the inventory on \"C\" and do \"Mouse1\" on the sword.");

    }

    private void ShowNextUI()
    {
        if(PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() == 2)
        {
            tutorialUI2.gameObject.SetActive(false);

            tutorialUI3.gameObject.SetActive(true);

            //tutorialUI3.GetComponent<Animator>().SetInteger("Arrow", 2);
        }

    }

}
