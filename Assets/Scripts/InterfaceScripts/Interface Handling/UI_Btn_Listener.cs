using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Btn_Listener : MonoBehaviour
{
    Button thisButton;
    // Start is called before the first frame update
    void Start()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        KeyManager.MyInstance.KeyBindOnClick(gameObject.name);
    }

}
