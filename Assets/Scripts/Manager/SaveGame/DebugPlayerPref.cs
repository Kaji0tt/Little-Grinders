﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPlayerPref : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        print("Reset Debug got called.");

        PlayerPrefs.DeleteKey("Load");

        if(PlayerPrefs.GetFloat("musicVol") == 0)
            PlayerPrefs.SetFloat("musicVol", 0.05f);

        if (PlayerPrefs.GetFloat("atmosphereVol") == 0)
            PlayerPrefs.SetFloat("atmosphereVol", 0.5f);

        if (PlayerPrefs.GetFloat("effectVol") == 0)
            PlayerPrefs.SetFloat("effectVol", 0.5f);

        if (PlayerPrefs.GetFloat("interfaceVol") == 0)
            PlayerPrefs.SetFloat("interfaceVol", 0.5f);

        if (PlayerPrefs.GetFloat("musicVol") == 0)
            PlayerPrefs.SetFloat("musicVol", 0.5f);

        PlayerPrefs.DeleteKey("MapX"); PlayerPrefs.DeleteKey("MapY");

        //UI_Manager.instance.BindKeysInManager();

        //Reset the Static Values of GlobalMap
        /*
        GlobalMap.currentMap = null;
        GlobalMap.lastSpawnpoint = null;
        GlobalMap.exploredMaps = null;
        */


    }

    private void Start()
    {
        //
    }

    private void OnDisable()
    {
        //Destroy(FindObjectOfType<KeyManager>());
        Destroy(this.gameObject);
    }
}
