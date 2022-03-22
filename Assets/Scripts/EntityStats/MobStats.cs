using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
      Viele Variable/Attribute werden sowohl von Player als auch Gegnern(Enemies) verwendet. Daher hier die Parent-Klasse Mob-Stats von der sowohl PlayerStats als auch Enemy Stats erben.
      Die Attribute für sauberen Code bitte nur über die Setter und Getter, sowie mitgelieferte Methoden wie .damage() oder .heal() im Falle von hp modifizieren.

      Beispiel der Implementierung in einer anderen Klasse am Player-Objekt player. Der Player wird hier mit 100 hp angenommen.

    Code:

                player.heal(40);
                Debug.Log("player health is " + player.Hp)
    
    output auf Konsole:

                player health is 140

      Auch innerhalb dieser oder der erbenden Klassen den Wert der kleingeschriebenen Variablen nur per Getter und Setter manipulieren, damit fehlerhafte Inputs nicht weiterverarbeitet
      werden (z.B. negativer Level oder movementSpeed wären unsinnig).

      richtig: 
                AttackPower = 10;

      falsch:   attackPower = 10;

      also zum merken: immer die Variablen mit Großbuchstaben manipulieren.
*/


public class MobStats : MonoBehaviour
{
    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;



    /*
      [SerializeField]
      private int level;
      [SerializeField]
      private float hp;
      [SerializeField]
      private float armor;
      [SerializeField]
      private float attackPower;
      [SerializeField]
      private float abilityPower;
      [SerializeField]
      private float attackSpeed;


      private float movementSpeed;
      */

    public int level;

    public int xp;
    public int Get_xp()
    {
        return xp;
    }
    
    [Space]
    /// <summary>
    /// Combat Values
    /// </summary>
    public float castCD = 0f;

    [HideInInspector]
    //Referenzwert für den Attack-Cooldown. Sollte 1 bleiben, alles andere wird über Attacksped kalkuliert.
    public float attackCD = 1f;



    /// <summary>
    /// HP-Stuff
    /// </summary>
    private float currentHp;
    public float Get_currentHp()
    {
        return currentHp;
    }
    public void Set_currentHp(float mod)
    {
        currentHp = currentHp + mod;
    }

    public void Load_currentHp(float value)
    {
        currentHp = value;
    }

    private float maxHp;
    public float Get_maxHp()
    {
        return maxHp = Hp.Value;
    }
    public void Set_maxHp()
    {
        maxHp = Hp.Value;
    }





}
