// 23.10.2024 AI-Tag
// This was created with assistance from Muse, a Unity Artificial Intelligence product
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Potion : ScriptableObject
{
    [Space]
    [Header("Beschreibung und Tooltip")]
    public string descr;


    [Space]
    [Header("Flat-Werte")]
    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;

    [Space]
    [Header("Prozent-Werte")]
    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;


    public virtual void Use()
    {
        //Hier sollten mit Statmodifiern gearbeitet werden.
        //Achte darauf: Potion Werte sollten stets mit Modifiern auf den Spieler angewendet werden.
        Debug.Log("Base Potion Maethod");
    }

}

[CreateAssetMenu(fileName = "MinorHealPotion", menuName = "Assets/Potions/MinorHealPotion")]
public class MinorHealPotion : Potion, IUseable
{

    new public int hp = 20;

    public override void Use()
    {
        Debug.Log("Minor Heal Potion Use reached");
        //Hier sollten mit Statmodifiern gearbeitet werden.
        //Achte darauf: Potion Werte sollten stets mit Modifiern auf den Spieler angewendet werden.
        PlayerManager.instance.player.GetComponent<PlayerStats>().Heal(hp);
    }

    public float CooldownTimer()
    {
        throw new System.NotImplementedException();
    }

    public float GetCooldown()
    {
        throw new System.NotImplementedException();
    }

    public string GetName()
    {
        throw new System.NotImplementedException();
    }

    public bool IsActive()
    {
        throw new System.NotImplementedException();
    }

    public bool IsOnCooldown()
    {
        throw new System.NotImplementedException();
    }

}