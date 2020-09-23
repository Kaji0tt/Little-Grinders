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

    public Item ID0001;
    public Item ID0002;
    public Item ID0003;
    public Item ID0004;
    public Item ID0005;
    //public Item SchmuckSprite;
}
