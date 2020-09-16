using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAssets : MonoBehaviour
{
    public static ItemAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Transform pfItemWorld;

    public Item Einfache_Schuhe;
    public Item Einfaches_Schwert;
    public Item BrustSprite;
    public Item Einfacher_Kopf;
    public Item WeaponSprite;
    public Item SchmuckSprite;
}
