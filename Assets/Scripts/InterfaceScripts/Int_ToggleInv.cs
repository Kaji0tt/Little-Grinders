using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Int_ToggleInv : MonoBehaviour
{

    public GameObject CharacterMenu, InventoryTab, SkillTab;

    [Space]
    //public GameObject[] CharakterMenus; //int[] array2 = new int[] { 1, 3, 5, 7, 9 }
    private bool ToggleInv;
    private Image[] InventoryItems;

    private void Start()
    {
        //InventoryTab = Get
        ToggleInv = false;
    }

    // Update is called once per frame
    void Update()
    {
        print(ToggleInv);
        if (Input.GetKeyDown(KeyCode.E) && ToggleInv==false)
        {
            ToggleInv = true;
            CharacterMenu.SetActive(true);
        }

        else if(Input.GetKeyDown(KeyCode.E) && ToggleInv==true)
        {
            ToggleInv = false;
            CharacterMenu.SetActive(false);
        }

        if (ToggleInv == true)
        {
            if (Input.GetKey(KeyCode.Tab))          //Ekliger Kladaradatsch. Eigentlich aus einem Array ausgelesen, die einzelnen Objekte durchschalten.
            {

                //if InventoryTab.SetActive(true);
                InventoryTab.SetActive(false);
                SkillTab.SetActive(true);
                
                
            }
        }
    }
}
