﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public static event Action eventLevelUp;

    /// <summary>
    /// XP Stuff
    /// </summary>

    public int xp;
    public int Get_xp()
    {
        return xp;
    }

    public int Set_xp(int mod)
    {
        xp = xp + mod;
        if (xp >= LevelUp_need())
            LevelUp();
        return xp;
    }
    //public int xp_needed;


    /// <summary>
    /// Combat Values
    /// </summary>
    public float attackCD = 0f;

    public int Range;


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


    /// <summary>
    /// Skill-Point stuff
    /// </summary>
    [SerializeField]
    private int skillPoints = 0;

    public int Get_SkillPoints()
    {
        return skillPoints;
    }
    public void Set_SkillPoints(int amount)
    {
        skillPoints = amount;
       
    }
    public int Decrease_SkillPoints(int amount)
    {
        skillPoints -= amount;
        return skillPoints;
    }


    /// <summary>
    /// Level-Stuff
    /// </summary>
    public int level = 1;
    public int Get_level()
    { return level;}
    public int LevelUp()
    {
        xp -= LevelUp_need();
        level ++;
        if(level >= 2)
        skillPoints++;
        //Der Call für Game.Event löst beim Start noch ein error aus, stört das spiel aber nicht.
        eventLevelUp?.Invoke();
        #region "Tutorial"
        if (level == 2)
        {
            Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
            tutorialScript.ShowTutorial(6);

        }
        #endregion
        return level;

    }

    public void Update()
    {
        LevelUp_need();
    }


    public void Start()
    {
        //currentHp = Hp.Value;
        Set_xp(1);
        //Set_level(1);
    }




    public void TakeDamage(float damage)
    {
        damage = 10 * (damage * damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte
        damage = Mathf.Clamp(damage, 1, int.MaxValue);
        //currentHp -= damage;
        Set_currentHp(-damage);
        if (currentHp <= 0)
            Die();

    }

    public void Heal (int healAmount)
    {
        //Der Heal sollte gefixxed werden. So dass der healAmount nicht größer sein kann als die Differenz von currentHP zu maxHP.
        if (healAmount + (int)Get_currentHp() >= (int)Get_maxHp())
        {
            healAmount =  (int)Get_maxHp() - (int)Get_currentHp();
            print(healAmount);
            Set_currentHp(healAmount);
        }

        else Set_currentHp(healAmount);
    }

    public int LevelUp_need()
    {
        float max_xp;
        max_xp = (Mathf.Log(1 + Mathf.Pow(level, 3)) * 300) / 2.5f; // log(​1 +​x ^​3) *​300 /​2.5
        int xp_needed = Mathf.CeilToInt(max_xp);
        return xp_needed;
    }

    public virtual void Die()
    {
        //Debug.Log(transform.name + "ist gestorben.");
        AudioManager.instance.Play("Dead2");
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

}


