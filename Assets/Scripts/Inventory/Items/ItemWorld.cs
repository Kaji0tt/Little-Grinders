using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEditor;
using TMPro;

public class ItemWorld : MonoBehaviour
{
    private ItemInstance item;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shadowSpriteRenderer; // Für Schattenwurf
    private Light lightSource;
    private Renderer itemWorldRend;
    [SerializeField] private TextMeshProUGUI itemWorldText;
    [SerializeField] private float m_Hue, m_Saturation, m_Value;

    [Header("Item Display Settings")]
    [Tooltip("Einheitliche Größe für alle gedroppten Items in World-Units")]
    [SerializeField] private float targetWorldSize = 1f; // Zielgröße in Unity-Units
    
    [Tooltip("Einheitliche Größe speziell für Gems (wenn 0, wird targetWorldSize verwendet)")]
    [SerializeField] private float targetGemSize = 0.3f; // Separate Größe für Gems
    
    [Header("Shadow Settings")]
    [Tooltip("Material für den Schatten-Sprite (wird hinter dem Item gerendert)")]
    [SerializeField] private Material shadowMaterial;
    
    [Header("Float Animation")]
    [Tooltip("Aktiviert die Auf-und-Ab-Animation")]
    [SerializeField] private bool enableFloatAnimation = true;
    
    [Tooltip("Höhe der Auf-und-Ab-Bewegung in Unity-Units")]
    [SerializeField] private float floatAmplitude = 0.1f;
    
    [Tooltip("Geschwindigkeit der Animation")]
    [SerializeField] private float floatSpeed = 1f;
    
    private Vector3 startPosition;
    private float floatTimer = 0f;

    // Debug: Eindeutige Instanz-ID für jede ItemWorld
    private static int nextId = 0;
    private int instanceId;

    private bool isBeingCollected = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lightSource = GetComponentInChildren<Light>();
        itemWorldRend = GetComponent<Renderer>();
        instanceId = nextId++;
        
        // Speichere Start-Position für Float-Animation
        startPosition = transform.position;
        
        // Erstelle Shadow-SpriteRenderer
        CreateShadowSprite();
        
        //Debug.Log($"[ItemWorld] Awake: Instanz {instanceId}, GameObject {gameObject.name}");
    }

    public static ItemWorld SpawnItemWorld(Vector3 position, ItemInstance item)
    {
        Transform transform = Instantiate(ItemAssets.Instance.pfItemWorld, position, Quaternion.identity);
        ItemWorld itemWorld = transform.GetComponent<ItemWorld>();
        itemWorld.SetItem(item);
        //Debug.Log($"[ItemWorld] SpawnItemWorld: Instanz {itemWorld.instanceId}, Item {item.ItemName} ({item.ItemID}) an {position}");
        return itemWorld;
    }

    public static ItemWorld DropItem(Vector3 dropPosition, ItemInstance item)
    {
        ItemWorld itemWorld = SpawnItemWorld(dropPosition, item);
        return itemWorld;
    }

    public void SetItem(ItemInstance item)
    {
        this.item = item;
        spriteRenderer.sprite = item.icon;
        itemWorldText = transform.GetComponentInChildren<TextMeshProUGUI>();

        // Normalisiere Sprite-Größe für einheitliche Darstellung
        NormalizeSpriteSize();
        
        // Aktualisiere Shadow-Sprite mit dem gleichen Icon
        UpdateShadowSprite();

        // Template aus Settings, z. B.: "F to pick up {item.ItemName}"
        string template = "{item.ItemName}";
        Color rarityColor = Color.white;

        switch (item.itemRarity)
        {
            case Rarity.Common:
                rarityColor = Color.white;
                itemWorldRend.material.SetColor("_Color", Color.white);
                lightSource.color = new Color(0, 1, 0, 0.2f);
                lightSource.range = 0f;
                lightSource.intensity = 0f;
                break;
            case Rarity.Uncommon:
                rarityColor = Color.green;
                itemWorldRend.material.SetColor("_Color", Color.green);
                itemWorldRend.material.SetFloat("_OffSet", 0.0010f);
                lightSource.color = new Color(0, 1, 0, 0.5f);
                lightSource.range = 1f;
                lightSource.intensity = 0.01f;
                AudioManager.instance.PlaySound("Drop_Uncommon");
                break;
            case Rarity.Rare:
                rarityColor = Color.blue;
                itemWorldRend.material.SetColor("_Color", Color.blue);
                itemWorldRend.material.SetFloat("_OffSet", 0.0015f);
                lightSource.color = Color.blue;
                lightSource.range = 1f;
                lightSource.intensity = 0.1f;
                AudioManager.instance.PlaySound("Drop_Rare");
                break;
            case Rarity.Epic:
                rarityColor = Color.magenta;
                itemWorldRend.material.SetColor("_Color", Color.magenta);
                itemWorldRend.material.SetFloat("_OffSet", 0.8f);
                lightSource.color = Color.magenta;
                lightSource.range = 1f;
                lightSource.intensity = 0.5f;
                AudioManager.instance.PlaySound("Drop_Epic");
                break;
            case Rarity.Legendary:
                rarityColor = new Color(0.54f, 0.32f, 0.13f, 1);
                itemWorldRend.material.SetColor("_Color", rarityColor);
                itemWorldRend.material.SetFloat("_OffSet", 1f);
                lightSource.color = rarityColor;
                lightSource.range = 1f;
                lightSource.intensity = 1f;
                AudioManager.instance.PlaySound("Drop_Legendary");
                break;
        }

        string hexColor = ColorUtility.ToHtmlStringRGB(rarityColor);
        string coloredName = $"<color=#{hexColor}>{item.ItemName}</color>";
        string resolvedText = template.Replace("{item.ItemName}", coloredName);
        itemWorldText.text = resolvedText;
        
        // Kompensiere Canvas-Skalierung für Items mit kleinerem Scale (z.B. Gems)
        // Der Canvas muss in Original-Größe bleiben, auch wenn das Item kleiner skaliert wurde
        Canvas canvas = itemWorldText.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (transform.localScale.x != 1f)
            {
                // Invertiere die Item-Skalierung für den Canvas
                float scaleCompensation = 1f / transform.localScale.x;
                canvas.transform.localScale = Vector3.one * scaleCompensation;
            }
            
            // NUR FÜR GEMS: Erhöhe Canvas-Position nach oben für besseren Abstand zum Item
            if (item.itemType == ItemType.Gem)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    Vector3 currentPos = canvasRect.localPosition;
                    canvasRect.localPosition = new Vector3(currentPos.x, 0.8f, currentPos.z); // 0.8 Units nach oben für Gems
                }
            }
        }
    }

    private void Update()
    {
        // Float-Animation
        if (enableFloatAnimation)
        {
            floatTimer += Time.deltaTime * floatSpeed;
            
            // Berechne Y-Offset mit Sinus-Funktion für smoothe Bewegung
            float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
            
            // Setze neue Position
            transform.position = startPosition + new Vector3(0, yOffset, 0);
        }
    }

    /// <summary>
    /// Skaliert das Sprite auf eine einheitliche World-Größe
    /// Gems können eine separate Größe haben
    /// </summary>
    private void NormalizeSpriteSize()
    {
        if (spriteRenderer.sprite == null)
            return;

        // Bestimme Zielgröße: Gems bekommen targetGemSize (falls > 0), andere Items targetWorldSize
        float targetSize = (item.itemType == ItemType.Gem && targetGemSize > 0) 
            ? targetGemSize 
            : targetWorldSize;

        // Hole die Bounds des Sprites (in World-Units)
        Bounds spriteBounds = spriteRenderer.bounds;
        
        // Berechne die größte Dimension (Breite oder Höhe)
        float maxDimension = Mathf.Max(spriteBounds.size.x, spriteBounds.size.y);

        // Berechne Skalierungsfaktor um auf targetSize zu kommen
        float scaleFactor = targetSize / maxDimension;

        // Wende Skalierung auf Transform an
        transform.localScale = Vector3.one * scaleFactor;

        //Debug.Log($"[ItemWorld] NormalizeSpriteSize: {item.ItemName} ({item.itemType}), TargetSize: {targetSize:F2}, Scale: {scaleFactor:F3}");
    }

    /// <summary>
    /// Erstellt den Shadow-SpriteRenderer als Child-Objekt
    /// </summary>
    private void CreateShadowSprite()
    {
        // Erstelle neues GameObject für den Schatten
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(transform);
        shadowObj.transform.localPosition = Vector3.zero;
        shadowObj.transform.localRotation = Quaternion.identity;
        shadowObj.transform.localScale = Vector3.one;

        // Füge SpriteRenderer hinzu
        shadowSpriteRenderer = shadowObj.AddComponent<SpriteRenderer>();
        
        // Aktiviere Cast Shadows
        shadowSpriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        
        // Setze Material (falls im Inspector zugewiesen)
        if (shadowMaterial != null)
        {
            shadowSpriteRenderer.material = shadowMaterial;
        }
    }

    /// <summary>
    /// Aktualisiert den Shadow-Sprite mit den gleichen Einstellungen wie das Haupt-Sprite
    /// </summary>
    private void UpdateShadowSprite()
    {
        if (shadowSpriteRenderer == null || spriteRenderer == null)
            return;

        // Kopiere Sprite
        shadowSpriteRenderer.sprite = spriteRenderer.sprite;
        
        // Kopiere Sorting Layer und Order (Order -1 für hinter dem Item)
        shadowSpriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        shadowSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        shadowSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        
        // Kopiere weitere relevante Einstellungen
        shadowSpriteRenderer.color = spriteRenderer.color;
        shadowSpriteRenderer.flipX = spriteRenderer.flipX;
        shadowSpriteRenderer.flipY = spriteRenderer.flipY;
        
        //Debug.Log($"[ItemWorld] UpdateShadowSprite: {item.ItemName}, SortingLayer: {shadowSpriteRenderer.sortingLayerName}, Order: {shadowSpriteRenderer.sortingOrder}");
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.isTrigger && !isBeingCollected)
        {
            // Hole das Inventory über UI_Inventory
            var inventory = UI_Inventory.instance.inventory;

            if (Input.GetKey(UI_Manager.instance.pickKey))
            {
                isBeingCollected = true;

                // Prüfe, ob ein freier Slot existiert und füge das Item hinzu
                bool added = inventory.AddItemToFirstFreeSlot(item);

                if (added)
                {
                    DestroySelf();
                }
                else
                {
                    // Inventar voll
                    isBeingCollected = false;
                }
            }
        }
    }

    private void OnMouseEnter()
    {
        // Zeige Tooltip beim Hovern über das Item
        if (item != null && UI_Manager.instance != null)
        {
            UI_Manager.instance.ShowItemTooltip(transform.position, item);
        }
    }

    private void OnMouseExit()
    {
        // Verstecke Tooltip wenn Maus das Item verlässt
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.HideTooltip();
        }
    }

    public void DestroySelf()
    {
        // Stelle sicher, dass Tooltip versteckt wird wenn Item aufgehoben wird
        if (UI_Manager.instance != null)
        {
            UI_Manager.instance.HideTooltip();
        }
        
        //Debug.Log($"[ItemWorld] DestroySelf EXECUTED: Instanz {instanceId}, Item {item?.ItemName}, Frame {Time.frameCount}");
        Destroy(gameObject);
    }
}
