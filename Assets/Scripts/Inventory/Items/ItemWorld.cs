using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEditor;

public class ItemWorld : MonoBehaviour
{

    private ItemInstance item;

    private SpriteRenderer spriteRenderer;

    private Light lightSource;
   
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        lightSource = GetComponentInChildren<Light>();

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

        if (item.itemRarity == "Unbrauchbar")
        {
            lightSource.color = new Color(1, 0, 0, 0.2f);
            lightSource.range = 1.5f;
            lightSource.intensity = 0.5f;
        }

        if (item.itemRarity == "Gewöhnlich")
        {
            lightSource.color = new Color(0, 1, 0, 0.5f);
            lightSource.range = 0f;
            lightSource.intensity = 0f;
        }

        if (item.itemRarity == "Ungewöhnlich")
        {
            lightSource.color = new Color(0,1,0,0.5f);
            lightSource.range = 1.5f;
            lightSource.intensity = 1f;

            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop_Uncommon");
        }


        if (item.itemRarity == "Selten")
        {
            lightSource.color = Color.blue;
            lightSource.range = 2f;
            lightSource.intensity = 1.5f;

            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop_Rare");
        }


        if (item.itemRarity == "Episch")
        {
            lightSource.color = Color.magenta;
            lightSource.range = 2f;
            lightSource.intensity = 2f;

            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop_Epic");
        }


        if (item.itemRarity == "Legendär")
        {
            lightSource.color = new Color(0.54f, 0.32f, 0.13f, 1);
            lightSource.range = 3f;
            lightSource.intensity = 2.5f;

            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop_Legendary");
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
