using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Int_ToggleInv : MonoBehaviour
{

    public GameObject characterMenu, inventoryTab, skillTab;

    [Space]
    //public GameObject[] CharakterMenus; //int[] array2 = new int[] { 1, 3, 5, 7, 9 }
    private bool toggleInv;
    private Image[] InventoryItems;

    private int currentMenu;

    private void Start()
    {
        //InventoryTab = Get
        toggleInv = false;
    }

    // Update is called once per frame
    void Update()
    {
     //   print(ToggleInv);
        if (Input.GetKeyDown(KeyCode.E) && toggleInv==false)
        {
            toggleInv = true;
            characterMenu.SetActive(true);
            currentMenu = 1;
        }

        else if(Input.GetKeyDown(KeyCode.E) && toggleInv==true)
        {
            toggleInv = false;
            characterMenu.SetActive(false);
        }

        if (toggleInv == true)
        {
            if (Input.GetKeyDown(KeyCode.Tab))          //Ekliger Kladaradatsch. Eigentlich aus einem Array ausgelesen, die einzelnen Objekte durchschalten.
            {


                switch (currentMenu)
                {
                    case 1:
                        inventoryTab.SetActive(false);
                        skillTab.SetActive(true);
                        currentMenu = 2;
                        break;
                    case 2:
                        inventoryTab.SetActive(true);
                        skillTab.SetActive(false);
                        currentMenu = 1;
                        break;
                }
                //if InventoryTab.SetActive(true);

                
                
            }
        }
    }
}
