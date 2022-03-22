using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_Map : MonoBehaviour, IPointerEnterHandler, IDescribable, IPointerClickHandler, IPointerExitHandler
{

    public Vector2 mapCords { get; private set; }

    public int mapLevel { get; private set; }

    public bool gotTeleporter { get; private set; }

    private bool isLoading = false;

    public void PopulateMap(MapSave map)
    {
        mapCords = new Vector2(map.mapIndexX, map.mapIndexY);

        mapLevel = map.mapLevel;

        Text levelText = transform.GetComponentInChildren<Text>();

        levelText.text = mapLevel.ToString();

        gotTeleporter = map.gotTeleporter;

        if (gotTeleporter)
            ChangeSprite();

    }

    private void ChangeSprite()
    {
        Image image = GetComponent<Image>();

        image.color =  Color.magenta;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription());
    }

    public string GetDescription()
    {
        string desc = ("Maplevel: " + mapLevel + "\nPortal found: " + gotTeleporter);
        return desc;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(gotTeleporter && !isLoading)
        {
            MapGenHandler.instance.ResetThisMap();
            MapGenHandler.instance.LoadMap(GlobalMap.instance.GetMapByCords(mapCords), GlobalMap.instance.lastSpawnpoint);
            isLoading = true;
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
