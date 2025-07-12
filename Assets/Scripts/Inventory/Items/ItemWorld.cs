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
    private Light lightSource;
    private Renderer itemWorldRend;
    [SerializeField] private TextMeshProUGUI itemWorldText;
    [SerializeField] private float m_Hue, m_Saturation, m_Value;

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

        // Template aus Settings, z. B.: "F to pick up {item.ItemName}"
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

        //Debug.Log($"[ItemWorld] SetItem: Instanz {instanceId}, Item {item.ItemName} ({item.ItemID})");
    }

    private void OnTriggerStay(Collider collider)
    {
        //Debug.Log($"[ItemWorld] OnTriggerStay: Instanz {instanceId}, Item {item?.ItemName}, Collider {collider.name}, Frame {Time.frameCount}");

        if (collider.isTrigger && !isBeingCollected)
        {
            IsometricPlayer isoPlayer = PlayerManager.instance.player.GetComponent<IsometricPlayer>();

            if (Input.GetKey(UI_Manager.instance.pickKey))
            {
                isBeingCollected = true; // <-- Flag setzen, damit die Logik nur einmal ausgeführt wird

                //Debug.Log($"[ItemWorld] PICKUP PRESSED: Instanz {instanceId}, Item {item?.ItemName}, Player {isoPlayer.gameObject.name}, Frame {Time.frameCount}");

                int itemCount = isoPlayer.inventory.itemList.Count + isoPlayer.inventory.GetConsumableDict().Count;
                //Debug.Log($"[ItemWorld] Inventory Count: {itemCount}, Instanz {instanceId}");

                if (itemCount < 15)
                {
                    //Debug.Log($"[ItemWorld] AddItem CALLED: Instanz {instanceId}, Item {item?.ItemName}, Player {isoPlayer.gameObject.name}, Frame {Time.frameCount}");
                    isoPlayer.inventory.AddItem(item);

                    //Debug.Log($"[ItemWorld] DestroySelf CALLED: Instanz {instanceId}, Item {item?.ItemName}, Frame {Time.frameCount}");
                    DestroySelf();
                }
                else
                {
                    //Debug.LogWarning($"[ItemWorld] Inventar voll: Instanz {instanceId}, Item {item?.ItemName}, Player {isoPlayer.gameObject.name}, Frame {Time.frameCount}");
                    isBeingCollected = false; // Falls Inventar voll, Flag zurücksetzen
                }
            }
        }
    }

/*
    public ItemInstance GetItem()
    {
        Debug.Log($"[ItemWorld] GetItem CALLED: Instanz {instanceId}, Item {item?.ItemName}, Frame {Time.frameCount}");
        return item;
    }
*/
    public void DestroySelf()
    {
        //Debug.Log($"[ItemWorld] DestroySelf EXECUTED: Instanz {instanceId}, Item {item?.ItemName}, Frame {Time.frameCount}");
        Destroy(gameObject);
    }
}
