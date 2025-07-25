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

    public WorldType mapTheme { get; private set; }

    private bool isLoading = false;

    public void PopulateMap(MapSave map)
    {
        mapCords = new Vector2(map.mapIndexX, map.mapIndexY);

        mapLevel = map.mapLevel;

        Text levelText = transform.GetComponentInChildren<Text>();

        levelText.text = mapLevel.ToString();

        gotTeleporter = map.gotTeleporter;

        mapTheme = map.mapTheme;

        // Set random sprite based on theme
        SetRandomThemeSprite();

        if (gotTeleporter)
            ChangeSprite();

    }

    private void SetRandomThemeSprite()
    {
        Image image = GetComponent<Image>();
        
        string spritePath = "";
        int spriteCount = 0;
        
        switch (mapTheme)
        {
            case WorldType.Forest:
                spritePath = "Sprite/Maps/Grass/Grass_";
                spriteCount = 4; // Grass_1 to Grass_4
                break;
            case WorldType.Desert:
                spritePath = "Sprite/Maps/Desert/Desert_";
                spriteCount = 5; // Desert_1 to Desert_5
                break;
            case WorldType.Jungle:
                spritePath = "Sprite/Maps/Jungle/Jungle_";
                spriteCount = 4; // Jungle_1 to Jungle_4
                break;
        }
        
        if (spriteCount > 0)
        {
            int randomSpriteIndex = UnityEngine.Random.Range(1, spriteCount + 1);
            string fullSpritePath = spritePath + randomSpriteIndex;
            
            Sprite newSprite = Resources.Load<Sprite>(fullSpritePath);
            
            if (newSprite != null)
            {
                image.sprite = newSprite;
            }
            else
            {
                Debug.LogWarning($"Could not load sprite at path: {fullSpritePath}");
            }
        }
    }

    private void ChangeSprite()
    {
        Image image = GetComponent<Image>();

        // Keep the theme sprite but change the color to indicate teleporter
        image.color = Color.magenta;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription());
    }

    public string GetDescription()
    {
        string desc = ("Maplevel: " + mapLevel + "\nTheme: " + mapTheme + "\nPortal found: " + gotTeleporter);
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
