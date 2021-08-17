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

            exploredMapsTxt.text = GlobalMap.instance.exploredMaps.Count.ToString();

            GlobalMap.instance.OnMapListChanged += WorldMap_OnMapListChanged;
        }


    }


    private void WorldMap_OnMapListChanged(object sender, EventArgs e)
    {
        exploredMapsTxt.text = GlobalMap.instance.exploredMaps.Count.ToString();

        CalculateExploredMaps();

        ScrollRect scrollR = GetComponent<ScrollRect>();

        //scrollR.Rebuild(CanvasUpdate prelayout);
        //scrollR.SetLayoutVertical();
    }

    public void CalculateExploredMaps()
    {

        foreach (Transform child in mapCenter)
        {
            if(child.GetComponent<UI_Map>() && child.gameObject.activeSelf)
            Destroy(child.gameObject);
        }

        if(GlobalMap.instance.exploredMaps.Count != 0)
        foreach (MapSave map in GlobalMap.instance.exploredMaps)
        {
            UI_Map uiMap = Instantiate(mapGO, position: new Vector2(transform.position.x + (map.mapIndexX * borderScaling), transform.position.y + (map.mapIndexY*borderScaling)), 
                Quaternion.identity, parent: mapCenter).AddComponent(typeof(UI_Map)) as UI_Map;

            uiMap.gameObject.SetActive(true);

            uiMap.PopulateMap(map);

            
        }

        exploredMapsTxt.text = GlobalMap.instance.exploredMaps.Count.ToString();
    }
}
