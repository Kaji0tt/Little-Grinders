using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogScript : MonoBehaviour
{
    #region Singleton
    public static LogScript instance;
    private void Awake()
    {
        instance = this;

    }
    #endregion

    private Text text;
    private bool input = false;

    private void Start()
    {
        text = transform.GetComponent<Text>();
        text.text = "";
    }

    void Update()
    {
        if(input)
        {
            text.CrossFadeAlpha(0, 4, false);
            input = false;
        }
    }

    public void ShowLog(string logtxt)
    {
        text.CrossFadeAlpha(1, 1, false);
        text.text = logtxt;
        input = true;
    }
}
