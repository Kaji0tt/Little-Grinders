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

    public Sprite SchuheSprite;
    public Sprite HoseSprite;
    public Sprite BrustSprite;
    public Sprite KopfSprite;
    public Sprite WeaponSprite;
    public Sprite SchmuckSprite;
}
