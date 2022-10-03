using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_BuffHolder : MonoBehaviour, IDescribable
{
    public BuffInstance buff;

    public Text durationText;

    public Text stackText;
    private void Start()
    {

    }
    private void Update()
    {
        int i = (int)buff.MyDuration;
        durationText.text = i.ToString();

        if(buff.MyDuration <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void DestroyGameObject()
    {
        Destroy(gameObject);
        Destroy(this);
    }

    public string GetDescription()
    {
        return buff.GetDescription();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
