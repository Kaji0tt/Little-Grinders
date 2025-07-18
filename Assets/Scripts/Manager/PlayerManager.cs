﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{

    #region Singleton
    public static PlayerManager instance;


    private void Awake()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<IsometricPlayer>();
        playerStats = player.GetComponent<PlayerStats>();

    }


    #endregion


    public IsometricPlayer player { get; private set; }

    public PlayerStats playerStats { get; private set; }


    public Image xpFill, hpFill;

    public TextMeshProUGUI xp_Text;

    public GameObject hp_Text;

}
