using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Auch innerhalb dieser oder der erbenden Klassen den Wert der kleingeschriebenen Variablen nur per Getter und Setter manipulieren, damit fehlerhafte Inputs nicht weiterverarbeitet
      werden(z.B.negativer Level oder movementSpeed wären unsinnig).

      richtig: 
                Xp = 10;

      falsch:   xp = 10;

      also zum merken: immer die Variablen mit Großbuchstaben manipulieren.
*/

public class EnemyStats : MonoBehaviour //MonoBehaviour //MobStats
{
    public CharStats Hp, Armor, AttackPower, AbilityPower, AttackSpeed;
    public IsometricPlayer isometricPlayer;
    //private MobStats Hp;

    //Geerbte Values von CharStats
    //private float e_hp, e_armor, e_attackPower, e_abilityPower, e_attackSpeed;
    void Start()
    {
        //e_hp = Hp.Value;
        //e_attackPower = AttackPower.Value;

    }

    /*Falls Transform.Collider(this) berührt Player.Collider
     * 
     *                                          
     * Führe aus isometricPlayer.TakeDamage(damage); <- Wobei damage = AttackPower
     * 
     * Während der Schaden von Enemy so berechnet wird:
     * 
     * in Isometric.Player, wenn Collider(this) berührt Enemy.Collider
     * call TakeDamage von Enemy Stats
     * 
    */
    public void TakeDamage(float damage)
    {
        damage -=Armor.Value;
        damage = Mathf.Clamp(damage, 0, int.MaxValue);
        Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
        if (Hp.Value <= 0)
            Die();

    }
    public virtual void Die()
    {
        Debug.Log("Enemy ist gestorben.");
        Destroy(this);
    }

}
