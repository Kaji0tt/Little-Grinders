using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldTooltip : MonoBehaviour
{
    public TextMeshProUGUI tooltipWorldText { get; private set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tooltipWorldText = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
