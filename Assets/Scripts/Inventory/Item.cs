using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item 
{
    public enum ItemType
    {
        Schuhe,
        Hose,
        Brust,
        Kopf,
        Weapon,
        Schmuck

    }
    public ItemType itemType;
    public int amount;
    public string ItemName;


    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.Schuhe:       return ItemAssets.Instance.SchuheSprite;
            case ItemType.Hose:         return ItemAssets.Instance.HoseSprite;
            case ItemType.Brust:        return ItemAssets.Instance.BrustSprite;
            case ItemType.Kopf:         return ItemAssets.Instance.KopfSprite;
            case ItemType.Weapon:       return ItemAssets.Instance.WeaponSprite;
            case ItemType.Schmuck:      return ItemAssets.Instance.SchmuckSprite;
        }
    }

}
