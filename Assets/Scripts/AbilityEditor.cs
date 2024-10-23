using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/TemplateAbility")]
public class AbilityEditor : Ability
{


    public GameObject possibleEffect1;

    public GameObject possibleEffect2;

    private StatModifier possibleStatModifier1;

    private StatModifier possibleStatModifier2;





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
