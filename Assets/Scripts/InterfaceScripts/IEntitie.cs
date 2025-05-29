using System.Collections.Generic;
using UnityEngine;

public enum EntitieStats { Hp, AbilityPower, AttackPower, MovementSpeed, AttackSpeed, Armor, Regeneration, None}

//Es sollte eine Funktion geben, die in IEntitie zwischen Player und Mob unterscheiden kann.
public interface IEntitie
{
    //public void AddNewStatModifier(EntitieStats stat, StatModifier mod);

    //public void RemoveStatModifier(EntitieStats stat, StatModifier mod);

    public void Die();

    public float Get_currentHp();

    public float Get_maxHp();

    public CharStats GetStat(EntitieStats stat);

    public Transform GetTransform();

    public List<BuffInstance> GetBuffs();

    //public void TakeDamage(float damage, int range);

    //public void TakeDirectDamage(float damage, float range);

    public void Heal(int healAmount);

    public void ApplyBuff(BuffInstance buff);

    public void RemoveBuff(BuffInstance buff);

}