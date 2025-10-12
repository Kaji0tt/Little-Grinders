using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UI_GlobalMap : MonoBehaviour
{
    #region Singleton
    public static UI_GlobalMap instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    //Wenn 

    public GameObject mapGO;

    //For Maps, to get aligned in center and get eventually moved
    [SerializeField]
    private Transform mapCenter;

    [SerializeField]
    private Text exploredMapsTxt;

    [SerializeField]
    private float borderScaling;

    private void Start()
    {
        if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            CalculateExploredMaps();

            exploredMapsTxt.text = GlobalMap.instance.GetVisitedMaps().Count.ToString();

            GlobalMap.instance.OnMapListChanged += WorldMap_OnMapListChanged;
        }


    }



    private void WorldMap_OnMapListChanged(object sender, EventArgs e)
    {
        exploredMapsTxt.text = GlobalMap.instance.GetVisitedMaps().Count.ToString();

        CalculateExploredMaps();

        ScrollRect scrollR = GetComponent<ScrollRect>();

        //scrollR.Rebuild(CanvasUpdate prelayout);
        //scrollR.SetLayoutVertical();
    }

    public void CalculateExploredMaps()
    {
        // Zerstöre ALLE UI_Map Kinder, unabhängig vom aktiven Zustand
        for (int i = mapCenter.childCount - 1; i >= 0; i--)
        {
            Transform child = mapCenter.GetChild(i);
            if (child.GetComponent<UI_Map>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // NEU: Zeige alle Maps (explored + generated)
        List<MapSave> allMaps = GlobalMap.instance.GetAllMaps();
        
        if(allMaps.Count != 0)
        foreach (MapSave map in allMaps)
        {
            UI_Map uiMap = Instantiate(mapGO, position: new Vector2(transform.position.x + (map.mapIndexX * borderScaling), transform.position.y + (map.mapIndexY*borderScaling)), 
                Quaternion.identity, parent: mapCenter).AddComponent(typeof(UI_Map)) as UI_Map;

            uiMap.gameObject.SetActive(true);

            uiMap.PopulateMap(map);
        }

        // Zeige nur die Anzahl der tatsächlich besuchten Maps im Text
        exploredMapsTxt.text = GlobalMap.instance.GetVisitedMaps().Count.ToString();
    }
}
