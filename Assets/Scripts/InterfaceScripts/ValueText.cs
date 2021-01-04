using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueText : MonoBehaviour
{
    public GameObject xpBar;
    private Slider xpBarValue;
    private Text valueText;
    void Start()
    {
        xpBarValue = xpBar.GetComponent<Slider>();
        valueText = GetComponent<Text>();
    }

    void LateUpdate()
    {

        valueText.text = xpBarValue.minValue + "/" + xpBarValue.maxValue;
    }
}
