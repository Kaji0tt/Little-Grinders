/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTemplate : Ability
{
    /// <summary> --> AI SUMMARY <---
    /// This Summary serves as Information for AI:
    /// Player Data:
    /// PlayerManager.instance.player as Reference for the Player GameObject for e.g. Transform.
    /// PlayerManager.instance.player.GetComponent<PlayerStats>(); is reference for the different Stats of the player.
    /// These include floats as of: 
    /// AbilityPower, Armor, AttackPower, AttackSpeed, Hp (Health), MovementSpeed
    /// theire Reference is fetched via: float aSpeed = pStats.AttackSpeed.Value;
    /// </summary>
    /// <param name="entitie"></param>

    #region UseRegion
    public override void UseBase(IEntitie entitie)
    {

        base.UseBase(entitie);


    }


    public override void ApplySpec1Bonus(Talent t)
    {


    }


    public override void OnUseSpec1(IEntitie entitie)
    {


    }

    public override void ApplySpec2Bonus(Talent t)
    {

    }
    public override void OnUseSpec2(IEntitie entitie)
    {

    }


    public override void ApplySpec3Bonus(Talent t)
    {

    }
    public override void OnUseSpec3(IEntitie entitie)
    {

    }

    #endregion


    #region TickRegion

    //Ein TickTimer, welcher alle x Sekunden während der Aktiven Zeit ausgeführt wird. Standard tickTimer ist auf 1 - also alle 1 Sekunden.
    public override void OnTickSpec1(IEntitie entitie)
    {
 

    }

    public override void OnTickSpec2(IEntitie entitie)
    {
 

    }

    public override void OnTickSpec3(IEntitie entitie)
    {
        //throw new NotImplementedException();
    }

    #endregion


    #region CooldownRegion
    public override void OnCooldown(IEntitie entitie)
    {

    }

    public override void OnCooldownSpec1(IEntitie entitie)
    {

    }

    public override void OnCooldownSpec2(IEntitie entitie)
    {

    }

    public override void OnCooldownSpec3(IEntitie entitie)
    {

    }
    #endregion
}
*/