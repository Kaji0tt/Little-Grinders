using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEditor;

public class ItemWorld : MonoBehaviour
{
    
    public static ItemWorld SpawnItemWorld(Vector3 position, Item item)
    {
        Transform transform = Instantiate(ItemAssets.Instance.pfItemWorld, position, Quaternion.identity);

        ItemWorld itemWorld = transform.GetComponent<ItemWorld>();
        itemWorld.SetItem(item);

        return itemWorld;
    }

    public static ItemWorld DropItem(Vector3 dropPosition, Item item)
    {

        ItemWorld itemWorld = SpawnItemWorld(dropPosition, item);
            return itemWorld;
    }
    private Item item;

    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetItem(Item item)
    {
        this.item = item;
        //Texture2D texture = item.GetSprite;
        spriteRenderer.sprite = item.GetSprite;
 

    }
    public Item GetItem ()
    {
        return item;
    }
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
   
}
