using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;


public enum InterfaceElementDeclaration { Inventar, Map, Skilltab, Tooltip, MainMenu, AB1, AB2, AB3, AB4, AB5 }
public class InterfaceElement : MonoBehaviour
{
    public InterfaceElementDeclaration interfaceElementEnum;

    public CanvasGroup myCanvasGroup { get; private set; }

    //public GameObject interfaceGameObject { get; private set; }

    public Button myButton { get; private set; }

    public void InitialisizeUIElement(InterfaceElement interfaceElement)
    {



        //interfaceElement.interfaceGameObject = interfaceElement.GetComponent<GameObject>();
        /*if (interfaceElement.interfaceGameObject.name == "Tooltip")
            Debug.Log("Yo Bruder, bin da ");
        */
        if (GetComponent<CanvasGroup>() != null)
        {
            //Debug.Log("atleast i got called by " + this.gameObject.name + ". I got assigned as: " + interfaceElementEnum.ToString());

            interfaceElement.myCanvasGroup = GetComponent<CanvasGroup>();
        }



        if (GetComponent<Button>() != null)
            interfaceElement.myButton = GetComponent<Button>();

    }


}
