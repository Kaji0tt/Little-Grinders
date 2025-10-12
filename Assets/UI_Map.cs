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

    public bool isCleared { get; private set; }

    public WorldType mapTheme { get; private set; }

    private bool isLoading = false;
    
    // NEU: Ob diese Map bereits besucht wurde
    private bool isExplored = false;

    public void PopulateMap(MapSave map)
    {
        mapCords = new Vector2(map.mapIndexX, map.mapIndexY);

        mapLevel = map.mapLevel;

        Text levelText = transform.GetComponentInChildren<Text>();

        levelText.text = mapLevel.ToString();

        isCleared = map.isCleared;

        mapTheme = map.mapTheme;
        
        // NEU: Prüfe ob Map bereits besucht wurde
        isExplored = map.isVisited;

        // Set random sprite based on theme
        SetRandomThemeSprite();

        if (isCleared)
            ChangeSprite();
            
        // NEU: Setze Transparenz für unbesuchte Maps
        SetTransparencyBasedOnExploration();
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

        // GEÄNDERT: Grüne Farbe für geklärte Maps
        image.color = Color.green;
    }
    
    // NEU: Setze Transparenz basierend auf Erkundungsstatus
    private void SetTransparencyBasedOnExploration()
    {
        Image image = GetComponent<Image>();
        Text levelText = transform.GetComponentInChildren<Text>();
        
        if (!isExplored)
        {
            // Unbesuchte Maps: 50% Transparenz
            Color imageColor = image.color;
            imageColor.a = 0.5f;
            image.color = imageColor;
            
            if (levelText != null)
            {
                Color textColor = levelText.color;
                textColor.a = 0.5f;
                levelText.color = textColor;
            }
        }
        else
        {
            // Besuchte Maps: Vollständig sichtbar
            Color imageColor = image.color;
            imageColor.a = 1f;
            image.color = imageColor;
            
            if (levelText != null)
            {
                Color textColor = levelText.color;
                textColor.a = 1f;
                levelText.color = textColor;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription());
    }

    public string GetDescription()
    {
        string explorationStatus = isExplored ? "Explored" : "Unexplored";
        string clearStatus = isCleared ? "Cleared!" : "Eclipsed."; // NEU: Angepasste Beschreibung
        string desc = ("Maplevel: " + mapLevel + "\nTheme: " + mapTheme + "\nStatus: " + clearStatus + "\nExploration: " + explorationStatus);
        return desc;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // NEU: Nur erkundete Maps sind anklickbar (unabhängig vom Cleared-Status)
        if(!isLoading && isExplored)
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
