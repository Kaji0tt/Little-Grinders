using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave 
{
    public int level;

    public float Hp, maxHp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public float[] position;

    public int xp;

    //Items
    
    public string brust, hose, kopf, schmuck, schuhe, weapon;

    //public static List<Item> equippedItems = new List<Item>();
    

    public PlayerSave (PlayerStats player)
    {
        level = player.level;

        Hp = player.Get_currentHp();


        /*  Die Base-Values müssen eigentlich eh nicht übernommen werden. Diese sind Fix oder maximal über den Skilltree veränderbar
        Armor = player.Armor.Value;

        AttackPower = player.AttackPower.Value;

        MovementSpeed = player.MovementSpeed.Value;

        AttackSpeed = player.AttackSpeed.Value;
        */

        xp = player.xp;


        //Items Speichern.
        ItemSave.SaveEquippedItems();
        if (ItemSave.brust != null)brust = ItemSave.brust;
        if (ItemSave.hose != null) hose = ItemSave.hose;
        if (ItemSave.kopf != null) kopf = ItemSave.kopf;
        if (ItemSave.schmuck != null) schmuck = ItemSave.schmuck;
        if (ItemSave.schuhe != null) schuhe = ItemSave.schuhe;
        if (ItemSave.weapon != null) weapon = ItemSave.weapon;


        //Skill-Points speichern.

        //inc.

        //Inventar speichern.

        //inc.

    }



}
