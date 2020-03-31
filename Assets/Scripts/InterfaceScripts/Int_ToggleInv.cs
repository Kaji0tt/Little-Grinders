using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Int_ToggleInv : MonoBehaviour
{

    public GameObject InventoryTab;
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
        if (Input.GetKeyDown(KeyCode.E) && ToggleInv==false)
        {
            ToggleInv = true;
            InventoryTab.SetActive(true);
        }

        else if(Input.GetKeyDown(KeyCode.E) && ToggleInv==true)
        {
            ToggleInv = false;
            InventoryTab.SetActive(false);
        }
    }
}
