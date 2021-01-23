using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave 
{
    public int level;
    public float Hp, maxHp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;
    public float[] position;

    public PlayerSave (PlayerStats player)
    {
        level = player.level;
        Hp = player.Get_currentHp();
        maxHp = player.Get_maxHp();
        Armor = player.Armor.Value;
        AttackPower = player.AttackPower.Value;
        MovementSpeed = player.MovementSpeed.Value;
        AttackSpeed = player.AttackSpeed.Value;


        position = new float[3];
        position[0] = PlayerManager.instance.player.transform.position.x;
        position[1] = PlayerManager.instance.player.transform.position.y;
        position[2] = PlayerManager.instance.player.transform.position.z;

    }

}
