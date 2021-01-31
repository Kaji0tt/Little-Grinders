using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPlayerPref : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        PlayerPrefs.DeleteKey("Load");
    }
}
