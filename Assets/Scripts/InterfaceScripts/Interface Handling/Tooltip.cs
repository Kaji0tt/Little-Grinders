using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    #region Singleton
    public static Tooltip instance;
    private void Awake()
    {


        Tooltip[] sceneInstances = FindObjectsOfType<Tooltip>();
        if (sceneInstances.Length >= 2)
        {
            Destroy(sceneInstances[0]);
        }
        instance = this;

    }

    #endregion
    public Canvas canvas;

    private RectTransform rect;

    private Text tooltipText;


    private void Start()
    {
        rect = GetComponent<RectTransform>();

        tooltipText = GetComponentInChildren<Text>();

        //gameObject.SetActive(false);
    }
    public void Update()
    {
        if(IsFullyOnScreen())
        {
            rect.pivot = new Vector2(0, 1);
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + 5f, 0);
        }

        else
        {
            rect.pivot = new Vector2(1, 0);
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 5f, 0);
        }


    }

    public bool IsFullyOnScreen()
    {

        RectTransform CanvasRect = canvas.GetComponent<RectTransform>();

        if ((rect.anchoredPosition.x * -1) + rect.sizeDelta.x >= (CanvasRect.sizeDelta.x / 2))
        {

            return true;
        }

        return false;
    }

    public void SetText(string newText)
    {
        tooltipText = GetComponentInChildren<Text>();

        tooltipText.text = newText;
    }
}
