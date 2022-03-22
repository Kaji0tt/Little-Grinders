using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Heal", menuName = "Assets/Spells/Heal")]
public class Heal : Ability
{
    public float healAmount;


    public override void Use(GameObject entitie)
    {
        base.Use(entitie);

        float entitieAP;

        if (entitie.GetComponent<PlayerStats>() != null)
            entitieAP = entitie.GetComponent<PlayerStats>().AbilityPower.Value;
        else entitieAP = entitie.GetComponent<MobStats>().AbilityPower.Value;

        Debug.Log("entitieAP: " + entitieAP);

        healAmount = healAmount + (entitieAP / 10);
        PlayerManager.instance.player.GetComponent<PlayerStats>().Heal((int)healAmount);
    }

    public override void CombatSpec()
    {
        
    }

    public override void UtilitySpec()
    {

    }

    public override void VoidSpec()
    {
        Debug.Log("pupsi, void spec funktioniert!");
    }
}
