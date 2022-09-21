using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//Ggf. wäre es sinvoll eine Interface Klasse zu erstellen, welche die gemeinsamkeiten zwischen MobStats und PlayerStats handled. 
//Buffs und Abilities könnten sich dann stets auf die InterfaceKlasse beziehen.
public class PlayerStats : MonoBehaviour, IEntitie
{
    #region Singleton
    public static PlayerStats instance;
    private void Awake()
    {
        instance = this;

    }
    #endregion

    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public static event Action eventLevelUp;


    /// <summary>
    /// Buff / Debuff System
    /// </summary>

    public List<BuffInstance> activeBuffs = new List<BuffInstance>();

    public List<BuffInstance> GetBuffs()
    {
        return activeBuffs;
    }

    private List<BuffInstance> newBuffs = new List<BuffInstance>();

    private List<BuffInstance> expiredBuffs = new List<BuffInstance>();

    public void ApplyBuff(BuffInstance buff)
    {
        //Falls ein neuer Buff hinzugefügt wird, überprüfe ob dieser Stackbar ist.
        if (!buff.stackable)
        {
            //Falls nicht, erstelle ein Template des Buffs, falls er sich bereits in der aktiven Liste befindet.
            BuffInstance tmp = activeBuffs.Find(x => x.buffName == buff.buffName);

            if (tmp != null)
            {
                //Falls ein aktiver Buff vorhanden war, erneuere ihn mit dem neuen Buff.
                expiredBuffs.Add(tmp);
            }

            buff.Activated(this, transform);
        }

        activeBuffs.Add(buff);

    }

    public void RemoveBuff(BuffInstance buff)
    {
        this.expiredBuffs.Add(buff);
        //buff.Expired();
    }

    private void HandleBuffs()
    {
        //print(activeBuffs.Count);
        if(activeBuffs.Count > 0)
        {
            foreach(BuffInstance buff in activeBuffs)
            {
                buff.active = true;
                buff.Update();
            }
        }

        if(newBuffs.Count > 0)
        {
            activeBuffs.AddRange(newBuffs);
            newBuffs.Clear();
        }

        if(expiredBuffs.Count > 0)
        {
            foreach(BuffInstance buff in expiredBuffs)
            {
                buff.active = false;
                activeBuffs.Remove(buff);
            }

            expiredBuffs.Clear();
        }

    }

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

        eventLevelUp?.Invoke();
        #region "Tutorial"
        if (level >= 2)
        {
            /* Currently disabled due to destroy of TutorialGameObject
            Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
            tutorialScript.ShowTutorial(6);
            */
            if(AudioManager.instance != null)
            AudioManager.instance.Play("Level-UP");

        }
        #endregion
        return level;

    }

    public void Update()
    {
        LevelUp_need();

        HandleBuffs();

        if (Input.GetKeyDown(KeyCode.J))
        {
            BuffInstance buffInstance = BuffDatabase.instance.GetInstance("Reflection");
            print(buffInstance.buffName);
            buffInstance.ApplyBuff(this);
        }
    }


    public void Start()
    {
        //currentHp = Hp.Value;
        Set_xp(1);
        //Set_level(1);




        //activeBuffs = new List<Buff>();
    }


    public void AddNewStatModifier(EntitieStats stats, StatModifier modifier)
    {
        switch (stats)
        {
            case EntitieStats.AbilityPower:
                AbilityPower.AddModifier(modifier);
                return;

            case EntitieStats.AttackPower:
                AttackPower.AddModifier(modifier);
                return;

            case EntitieStats.Armor:
                Armor.AddModifier(modifier);
                return;

            case EntitieStats.Hp:
                Hp.AddModifier(modifier);
                return;

            case EntitieStats.MovementSpeed:
                MovementSpeed.AddModifier(modifier);
                return;

            case EntitieStats.AttackSpeed:
                AttackSpeed.AddModifier(modifier);
                return;


            default: return;
        }
    }

    public void RemoveStatModifier(EntitieStats stats, StatModifier modifier)
    {
        switch (stats)
        {
            case EntitieStats.AbilityPower:
                AbilityPower.RemoveModifier(modifier);
                return;

            case EntitieStats.AttackPower:
                AttackPower.RemoveModifier(modifier);
                return;

            case EntitieStats.Armor:
                Armor.RemoveModifier(modifier);
                return;

            case EntitieStats.Hp:
                Hp.RemoveModifier(modifier);
                return;

            case EntitieStats.MovementSpeed:
                MovementSpeed.RemoveModifier(modifier);
                return;

            case EntitieStats.AttackSpeed:
                AttackSpeed.RemoveModifier(modifier);
                return;


            default: return;
        }
    }

    public float GetStat(EntitieStats stat)
    {
        switch (stat)
        {
            case EntitieStats.AbilityPower:
                return AbilityPower.Value;

            case EntitieStats.AttackPower:
                return AttackPower.Value;

            case EntitieStats.Armor:
                return Armor.Value;

            case EntitieStats.Hp:
                return Hp.Value;

            case EntitieStats.MovementSpeed:
                return MovementSpeed.Value;

            case EntitieStats.AttackSpeed:
                return AttackSpeed.Value;

            default:
                Debug.Log("No Stat.Value found.");
                return 0;
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void TakeDamage(float damage, int range)
    {
        //Die Angriffsreichweite der Mobs (range) spielt derzeit keine rolle. Alle Angriffsverweise auf den Spieler können somit schließlich einen beliebigen Wert besitzen.
        damage = 10 * (damage * damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte
        damage = Mathf.Clamp(damage, 1, int.MaxValue);
        
        //Berichte den Gamevents vom verursachten Schaden am Spieler.
        //GameEvents.current.PlayerWasAttacked(damage);

        //Ziehe den Schaden vom Spielerleben ab.
        Set_currentHp(-damage);

        //Falls das Leben gegen 0 geht, stirbt der Spieler.
        if (currentHp <= 0)
            Die();

    }

    public void TakeDirectDamage(float damage, float range)
    {
        Set_currentHp(-damage);
        if (currentHp <= 0)
            Die();
    }

    //Ggf. TakeDirectDamage hinzufügen? Oder TakeMagicDamage oder ähnliches? Bis dahin wird Schaden lediglich abzgl. Armor abgerechnet.

    public void Heal (int healAmount)
    {
        //Der Heal sollte gefixxed werden. So dass der healAmount nicht größer sein kann als die Differenz von currentHP zu maxHP.
        if (healAmount + (int)Get_currentHp() >= (int)Get_maxHp())
        {
            healAmount =  (int)Get_maxHp() - (int)Get_currentHp();

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


