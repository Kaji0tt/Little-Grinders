using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    #region Singleton
    public static PlayerManager instance;
    private void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");

    }


    #endregion


    public GameObject player { get; private set; }

}
