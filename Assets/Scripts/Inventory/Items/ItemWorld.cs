using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEditor;

public class ItemWorld : MonoBehaviour
{

    private ItemInstance item;

    private SpriteRenderer spriteRenderer;
   
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        //Texture2D texture = item.GetSprite;
        spriteRenderer.sprite = item.icon;
 

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
