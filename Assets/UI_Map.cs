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
        Renderer rend = this.gameObject.GetComponent<Renderer>();

        rend.material.color = Color.magenta;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(eventData.position, GetDescription());
    }

    public string GetDescription()
    {
        string desc = ("Maplevel: " + mapLevel + "\nPortal found: " + gotTeleporter);
        return desc;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(gotTeleporter)
        MapGenHandler.instance.LoadMap(GlobalMap.instance.GetMapByCords(mapCords), GlobalMap.instance.lastSpawnpoint);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
