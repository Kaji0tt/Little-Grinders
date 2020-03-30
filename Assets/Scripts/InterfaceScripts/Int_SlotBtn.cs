using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Int_SlotBtn : MonoBehaviour
{
    public string StoredData;
    private GameObject EQSlot;
    private GameObject InvSlot;

    //in StoreData wird nach dem Collidieren des Spielers mit dem Instantierten Spielobjektes 
    //entsprechende Informationen über die Item-Art gespeichert.
    //
    //Im Testbeispiel Schuhe und ggf. Waffe


    void Start()
    {
        //sprite = GetComponent<Image>();
    }

    public void TaskOnClick()
    {
        if (transform.GetComponent<Image>().sprite == Resources.Load<Sprite>("Blank_Icon"))
        {
            print("Found an empty space!");
        }

        else if (transform.GetComponent<Image>().sprite != Resources.Load<Sprite>("Blank_Icon"))
        {

            StoredData = transform.GetComponent<Image>().sprite.name;
            print(StoredData);

            // Auslesen der Sprite und weitergeben des Sprites an richtigen Ausrüstungsslots
            if (StoredData.Contains("Schuhe"))
            {
                StoredData = "Schuhe";
                ItemEQSwap();
            }
            else if (StoredData.Contains("Hose"))
            {
                StoredData = "Hose";
                ItemEQSwap();
            }
            else if (StoredData.Contains("Brust"))
            {
                StoredData = "Brust";
                ItemEQSwap();
            }
            else if (StoredData.Contains("Kopf"))
            {
                StoredData = "Kopf";
                ItemEQSwap();
            }
            else if (StoredData.Contains("Weapon"))
            {
                StoredData = "Weapon";
                ItemEQSwap();
            }
            else if (StoredData.Contains("Schmuck"))
            {
                StoredData = "Schmuck";
                ItemEQSwap();
            }
            else
            {
                print("Dieses Item kann nicht ausgerüstet werden!");
            }


        }
    }

    public string ItemEQSwap()
    {
        string goname = name;
        if (goname == "Slot")
        {
            EQSlot = GameObject.Find("EQ" + StoredData + "Img");
            Sprite EQSprite = EQSlot.GetComponent<Image>().sprite;
            EQSlot.GetComponent<Image>().sprite = transform.GetComponent<Image>().sprite;
            transform.GetComponent<Image>().sprite = EQSprite;
            string EQSpriteName = EQSprite.name;

            if (EQSpriteName.Contains("Schuhe"))
            {
                StoredData = "Schuhe";
            }
            if (EQSpriteName.Contains("Hose"))
            {
                StoredData = "Hose";
            }
            if (EQSpriteName.Contains("Brust"))
            {
                StoredData = "Brust";
            }
            if (EQSpriteName.Contains("Kopf"))
            {
                StoredData = "Kopf";
            }
            if (EQSpriteName.Contains("Weapon"))
            {
                StoredData = "Weapon";
            }
            if (EQSpriteName.Contains("Schmuck"))
            {
                StoredData = "Schmuck";
            }


        }

        else if (goname.Contains("EQ"))
        {

            //Transform InventorySlots = transform.Find("Slot");
            //foreach(Transform slot in InventoryPanel)

            GameObject[] Slots;
            Slots = GameObject.FindGameObjectsWithTag("Inv-Slot");
            for (int i = 0; i < 15; i++)
            {

                if (Slots[i].GetComponent<Image>().sprite == Resources.Load<Sprite>("Blank_Icon"))
                {
                    StoredData = gameObject.GetComponent<Image>().sprite.name;
                    Slots[i].GetComponent<Image>().sprite = gameObject.GetComponent<Image>().sprite;
                    if (goname.Contains("EQ"))
                    {
                        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
                    }
                }

                else
                {
                    print("Inventar ist voll!");
                }
            }

            /*
            for (int i = 0; i < 15; i++)
            {
                FindObject
                if (invIcons[i].GetComponent<Image>().sprite == Resources.Load<Sprite>("Blank_Icon"))
                {
                    print("Found an empty space!");
                    break;
                }
            }
            */
        }

        return StoredData;
    }

}
