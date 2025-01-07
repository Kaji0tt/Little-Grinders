using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEditor;

public class ItemWorld : MonoBehaviour
{

    private ItemInstance item;

    private SpriteRenderer spriteRenderer;

    private Light lightSource;

    private Renderer itemWorldRend;

    [SerializeField]
    private float m_Hue, m_Saturation, m_Value;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        lightSource = GetComponentInChildren<Light>();

        itemWorldRend = GetComponent<Renderer>();

    }

    public static ItemWorld SpawnItemWorld(Vector3 position, ItemInstance item)
    {
        Transform transform = Instantiate(ItemAssets.Instance.pfItemWorld, position, Quaternion.identity);

        ItemWorld itemWorld = transform.GetComponent<ItemWorld>();

        itemWorld.SetItem(item);

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


        #region GFX & Audio Settings for Rarities
        if (item.itemRarity == "Unbrauchbar")
        {
            itemWorldRend.material.SetColor("_Color", Color.red);
            
            lightSource.color = new Color(1, 0, 0, 0.5f);
            lightSource.range = 0.2f;
            lightSource.intensity = 0.005f;
            
        }

        if (item.itemRarity == "Gewöhnlich")
        {

            itemWorldRend.material.SetColor("_Color", Color.white);

            lightSource.color = new Color(0, 1, 0, 0.2f);
            lightSource.range = 0f;
            lightSource.intensity = 0f;
        }

        if (item.itemRarity == "Ungewöhnlich")
        {

            //DynamicGI.SetEmissive(itemWorldRend, new Color(1f, 0.1f, 0.5f, 1.0f) * 1);
            itemWorldRend.material.SetColor("_Color", Color.green);
            itemWorldRend.material.SetFloat("_OffSet", 0.0010f);

            lightSource.color = new Color(0,1,0,0.5f);
            lightSource.range = 1f;
            lightSource.intensity = 0.01f;

                AudioManager.instance.Play("Drop_Uncommon");
        }


        if (item.itemRarity == "Selten")
        {
            itemWorldRend.material.SetColor("_Color", Color.blue);
            itemWorldRend.material.SetFloat("_OffSet", 0.0015f);

            lightSource.color = Color.blue;
            lightSource.range = 1f;
            lightSource.intensity = 0.1f;

                AudioManager.instance.Play("Drop_Rare");
        }


        if (item.itemRarity == "Episch")
        {
            itemWorldRend.material.SetColor("_Color", Color.magenta);
            itemWorldRend.material.SetFloat("_OffSet", 0.8f);

            lightSource.color = Color.magenta;
            lightSource.range = 1f;
            lightSource.intensity = 0.5f;

                AudioManager.instance.Play("Drop_Epic");
        }


        if (item.itemRarity == "Legendär")
        {
            itemWorldRend.material.SetColor("_Color", new Color(0.54f, 0.32f, 0.13f, 1));
            itemWorldRend.material.SetFloat("_OffSet", 1f);

            lightSource.color = new Color(0.54f, 0.32f, 0.13f, 1);
            lightSource.range = 1f;
            lightSource.intensity = 1f;

                AudioManager.instance.Play("Drop_Legendary");
        }
        #endregion



    }

    // 23.10.2024 AI-Tag
    // This was created with assistance from Muse, a Unity Artificial Intelligence product

    private void OnTriggerStay(Collider collider)
    {
        if (collider.isTrigger)
        {
            IsometricPlayer isoPlayer = PlayerManager.instance.player.GetComponent<IsometricPlayer>();

            if (Input.GetKey(UI_Manager.instance.pickKey))
            {
                // Überprüfen, ob Platz im Inventar ist
                if (isoPlayer.inventory.itemList.Count + isoPlayer.inventory.GetConsumableDict().Count < 15)
                {
                    // Füge Item zum Inventar hinzu
                    isoPlayer.inventory.AddItem(GetItem());

                    // Zerstöre den Collider
                    DestroySelf();
                }
                else
                {
                    // Inventar ist voll, keine Aktion ausführen
                    Debug.LogWarning("Inventar ist voll! Gegenstand kann nicht aufgenommen werden.");
                }
            }
        }
    }

    public ItemInstance GetItem ()
    {
        return item;
    }
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
   
}
