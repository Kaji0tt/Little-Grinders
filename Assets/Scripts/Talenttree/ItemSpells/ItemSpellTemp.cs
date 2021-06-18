using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSpell", menuName = "Assets/ItemSpell")]
public class ItemSpellTemp : Spell, IUseable
{
    [Header("Werte dieses Spells")]
    public float healAmount;

    public string description;

    //If its a Spell, which instantiate Prefabs like Bullets / Fireball, it should be called on Isometric Player with "player.CastSpell(this);"

    void Start()
    {

    }
    public override void Use()
    {



    }

    public override bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public override float CooldownTimer()
    {
        return coolDownTimer;
    }

    public override float GetCooldown()
    {
        return GetSpellCoolDown;
    }

}