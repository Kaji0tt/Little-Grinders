using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class TalentLines : MonoBehaviour
{
    #region Singleton
    public static TalentLines instance;
    private void Awake()
    {
        instance = this;

    }
    #endregion

    public float lineThickness = 3f;

    /*
    [SerializeField]
    private Transform parent;
    */
    public void CreateConnection(Talent talent, Talent childTalent)
    {

        GameObject gameObject = new GameObject("TalentLine", typeof(Image));
        gameObject.transform.SetParent(this.gameObject.GetComponentInParent<Transform>(), false);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        Vector2 dir = (childTalent.GetComponent<RectTransform>().anchoredPosition - talent.GetComponent<RectTransform>().anchoredPosition).normalized;
        float distance = Vector2.Distance(talent.GetComponent<RectTransform>().anchoredPosition, childTalent.GetComponent<RectTransform>().anchoredPosition);

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, lineThickness);
        rectTransform.anchoredPosition = new Vector2(talent.transform.position.x, talent.transform.position.y) + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
    }


}
