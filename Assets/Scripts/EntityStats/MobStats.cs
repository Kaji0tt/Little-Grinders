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
    private int level;
    private int hp;
    private int attackPower;
    private int abilityPower;
    private float attackSpeed;
    private int movementSpeed;

    public int Hp 
    {
        get {return hp; }
        private set
        {
            if (value < 0) hp = 0;
            else hp = value;
        }
    }

    public int AttackPower
    {
        get { return attackPower; }
        private set
        {
            if (value < 0) hp = 0;
            else attackPower = value;
        }
    }

    public int AbilityPower
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
    public int MovementSpeed
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



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("test");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
