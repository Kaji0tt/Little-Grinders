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


    public float Hp
    {
        get { return hp; }
        private set
        {
            if (value < 0) hp = 0;
            else hp = value;
        }
    }

    public float Armor
    {
        get { return armor; }
        private set
        {
            if (value < 0) hp = 0;
            else armor = value;
        }
    }

    public float AttackPower
    {
        get { return attackPower; }
        private set
        {
            if (value < 0) hp = 0;
            else attackPower = value;
        }
    }

    public float AbilityPower
    {
        get { return abilityPower; }
        private set
        {
            if (value < 0) hp = 0;
            else abilityPower = value;
        }
    }

    public float AttackSpeed
    {
        get { return attackSpeed; }
        private set
        {
            if (value < 0) hp = 0;
            else attackSpeed = value;
        }
    }
    public float MovementSpeed
    {
        get { return movementSpeed; }
        private set
        {
            if (value < 0) hp = 0;
            else movementSpeed = value;
        }
    }

    public int Level
    {
        get { return level; }
        private set
        {
            if (value < 0) hp = 0;
            else level = value;
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
