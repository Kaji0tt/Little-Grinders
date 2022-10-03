using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Scheint nicht verwendet zu werden.
public class Int_ItemPickup : MonoBehaviour
{
    private string StoredData;
    public Int_SlotBtn int_SlotBtn;
    private SpriteRenderer sprite;
    void Start()
    {
        print(gameObject.name);
        StoredData = "Nothing";
        sprite = GetComponent<SpriteRenderer>();


    }

    private void OnTriggerStay(Collider Charakter)
    {
        //print("Charakter steht in mir");

        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameObject[] Slots;
            Slots = GameObject.FindGameObjectsWithTag("Inv-Slot");
            for (int i = 0; i < 15; i++)
            {

                if (Slots[i].GetComponent<Image>().sprite == Resources.Load<Sprite>("Blank_Icon"))
                {

                    StoredData = gameObject.GetComponent<SpriteRenderer>().sprite.name;
                    Slots[i].GetComponent<Image>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
                    Destroy(gameObject);
                    break;
                }

            }


        }
    }

}
