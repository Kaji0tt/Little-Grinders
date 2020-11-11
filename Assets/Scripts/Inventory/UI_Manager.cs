using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    #region Singleton
    public static UI_Manager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    [SerializeField]
    private GameObject tooltip;

    private Text tooltipText;

    private void Start()
    {
        tooltipText = tooltip.GetComponentInChildren<Text>();
    }

    public void ShowItemTooltip(Vector3 position, Item item)
    {
 
        tooltip.SetActive(true);
        //tooltip.GetComponentInChildren<GameObject>().SetActive(true);
        tooltip.transform.position = position;
        //print("you got here");
        if (item != null)
        {
            //print("and for some suspicious reason you got here aswell. Hi there" + item.ItemName);
            //tooltipText = tooltip.GetComponentInChildren<Text>();

            tooltipText.text = "<b>"+item.ItemName + "</b>\n"+ item.ItemDescription + "\n\n" + item.GetValueDescription();
        }
    }

    public void ShowTooltip(Vector3 position, string description)
    {

        tooltip.SetActive(true);
        tooltip.transform.position = position;

        if (description != null)
        {
            tooltipText.text = description;
        }
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }
}
