using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MobStats : MonoBehaviour, IEntitie
{
    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public bool pulled;
    //public int experience { get; private set; }

    private void Start()
    {
        CalculateMobStats();

    }

    void Update()
    {
        HandleBuffs();
    }

    public CharStats GetStat(EntitieStats stat)
    {
        switch (stat)
        {
            case EntitieStats.AbilityPower:
                return AbilityPower;

            case EntitieStats.AttackPower:
                return AttackPower;

            case EntitieStats.Armor:
                return Armor;

            case EntitieStats.Hp:
                return Hp;

            case EntitieStats.MovementSpeed:
                return MovementSpeed;

            case EntitieStats.AttackSpeed:
                return AttackSpeed;

            default:
                Debug.Log("No Stat.Value found.");
                return null;
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void CalculateMobStats()
    {
        //Alles noch nicht durchdacht, für pre alpha 0.3 solls reichen.
        if (GlobalMap.instance != null)
        {
            //Das Level des Mobs skaliert mit dem Level der Karte
            level = level + GlobalMap.instance.currentMap.mapLevel;

            //Falls das Level des Mobs größer ist, als das Level der aktuell geladenen Karte,
            if (level > GlobalMap.instance.currentMap.mapLevel)
            {
                //Erhöhe die HP um den Wert des Kartenlevels * 10 / 2. 
                Hp.BaseValue = Hp.BaseValue * (GlobalMap.instance.currentMap.mapLevel * 5);
                //OLD: Hp.AddModifier(new StatModifier(Hp.Value + (GlobalMap.instance.currentMap.mapLevel * 5), StatModType.Flat));

                //Erhöhe Rüstung um den Wert des Kartenlevels.
                Armor.BaseValue = Armor.BaseValue * (GlobalMap.instance.currentMap.mapLevel + 1);
                //OLD: Armor.AddModifier(new StatModifier((PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() / (GlobalMap.instance.currentMap.mapLevel + 1)), StatModType.Flat));

                //Erhöhe Angriff um den Wert des Kartenlevels.
                AttackPower.BaseValue = AttackPower.BaseValue + (GlobalMap.instance.currentMap.mapLevel * 2);
                //AttackPower.AddModifier(new StatModifier(AttackPower.Value + (GlobalMap.instance.currentMap.mapLevel * 2), StatModType.Flat));


            }

            //Erhöhe abschließend die ausgeteilte Erfahrung um das Kartenlevel * 10
            xp += GlobalMap.instance.currentMap.mapLevel * 10;

        }

    }
    /// <summary>
    /// Buff / Debuff System
    /// Es ist wichtig 3 unterschiedliche Listen zu führen, damit keine Fehler beim iterieren der einzelnen Listen auftreten.
    /// </summary>

    //Aktive Buffs
    public List<BuffInstance> activeBuffs = new List<BuffInstance>();

    //Eine Methode um die aktiven Buffs abzurufen, für IEntitie
    public List<BuffInstance> GetBuffs()
    {
        return activeBuffs;
    }

    //Neue Buffs
    private List<BuffInstance> newBuffs = new List<BuffInstance>();

    //Ausgelaufene Buffs. 
    private List<BuffInstance> expiredBuffs = new List<BuffInstance>();


    public void ApplyBuff(BuffInstance buff)
    {
        //Falls ein neuer Buff hinzugefügt wird, überprüfe ob dieser Stackbar ist.
        if(!buff.stackable)
        {
            //Falls nicht, erstelle ein Template des Buffs, falls er sich bereits in der aktiven Liste befindet.
            BuffInstance tmp = activeBuffs.Find(x => x.buffName == buff.buffName);

            if (tmp != null) 
            {
                //Falls ein aktiver Buff vorhanden war, erneuere ihn mit dem neuen Buff.
                RemoveBuff(tmp);
                //expiredBuffs.Add(tmp);
            }
        }

        this.activeBuffs.Add(buff);

    }

    public void RemoveBuff(BuffInstance buff)
    {
        this.expiredBuffs.Add(buff);
        buff.Expired(buff.MyTargetEntitie, buff.MyOriginEntitie);
    }

    private void HandleBuffs()
    {
        if (activeBuffs.Count > 0)
        {
            foreach (BuffInstance buff in activeBuffs)
            {
                buff.Update();
            }
        }

        if (newBuffs.Count > 0)
        {
            activeBuffs.AddRange(newBuffs);
            newBuffs.Clear();
        }

        if (expiredBuffs.Count > 0)
        {
            foreach (BuffInstance buff in expiredBuffs)
            {
                activeBuffs.Remove(buff);
            }

            expiredBuffs.Clear();
        }

    }


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

    public void TakeDamage(float incoming_damage, int range_radius_ofDMG)
    {

        float player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range_radius_ofDMG)
        {
            incoming_damage = 10 * (incoming_damage * incoming_damage) / (Armor.Value + (10 * incoming_damage));            // DMG & Armor als werte

            incoming_damage = Mathf.Clamp(incoming_damage, 1, int.MaxValue);

            Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));


            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

                //Play a Sound at random.
                AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);

            //Füge eine Hit Animation für den Animator hinzu
            IsometricRenderer isoRend = GetComponent<IsometricRenderer>();
            isoRend.PlayHit();
            //isoRend.


            pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }

        if (Hp.Value <= 0)
            Die();
    }


    //Take Direct Damage ignoriert die Rüstungswerte der Entitie - besonders relevant für AP Schaden.
    public virtual void TakeDirectDamage(float incoming_damage, float range_radius_ofDMG)
    {
        float player_distance = Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position);

        if (player_distance <= range_radius_ofDMG)
        {

            Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));

            //Sound-Array mit den dazugehörigen Sound-Namen
            string[] hitSounds = new string[] { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            if (AudioManager.instance != null)

                //Play a Sound at random.
                AudioManager.instance.Play(hitSounds[UnityEngine.Random.Range(0, 2)]);

            pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }

        if (Hp.Value <= 0)
            Die();
    }

    public void Heal(int healAmount)
    {
        //Der Heal sollte gefixxed werden. So dass der healAmount nicht größer sein kann als die Differenz von currentHP zu maxHP.
        if (healAmount + (int)Get_currentHp() >= (int)Get_maxHp())
        {
            healAmount = (int)Get_maxHp() - (int)Get_currentHp();

            Set_currentHp(healAmount);
        }

        else Set_currentHp(healAmount);
    }

    public void Die()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.Gain_xp(xp);

        if (this.level >= 1)
            //Eine Funktion die in ABhängigkeit vom Moblevel, die ANzahl der Drops erhöhen soll. Hier wäre ein Lerp-Funktion sinnvoll.
            for (int i = 0; i <= level / 2; i++)
            {
                ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);
            }

        Destroy(this);

        Destroy(gameObject);
    }


}
