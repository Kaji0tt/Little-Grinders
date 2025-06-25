using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MobStats : MonoBehaviour
{
    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed, Regeneration;

    public bool isDead { get; private set; } = false;

    private void Start()
    {
        CalculateMobStats();
        //Debug.Log(gameObject.name + ": My Health Base is " + Hp.BaseValue + ", after Calculation it is:" + Hp.Value);
        StartCoroutine(Regenerate());

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

            case EntitieStats.Regeneration:
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
    /// Carefull, currently currentHP is of no usage. It will be of important role, once Monster Modifications are implemented.
    /// Take BaseValue instead.
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

    public bool isRegenerating = true;

    IEnumerator Regenerate()
    {
        while (isRegenerating)
        {
            yield return new WaitForSeconds(1);
            if (currentHp < Hp.Value)
            {
                currentHp = Mathf.Min(currentHp + Regeneration.Value, Hp.Value);
            }
        }
    }

    public void Die()
    {

        if(!isDead)
        StartCoroutine(DelayedDeath());
    }

    private IEnumerator DelayedDeath()
    {
        this.isDead = true;

        ///<summary>
        /// Entferne Rigidbody / Collider oder NavMesh
        /// Du fauler Hund, mach doch einfach den richtig weg und streich den Rest vom Code.
        ///</summary>
        var collider = GetComponentInChildren<Collider>();
        if (collider != null)
            Destroy(collider);

        // XP geben
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        playerStats.Gain_xp(xp);

        if (this.level >= 1)
        {
            ItemDatabase.instance.GetWeightDrop(transform.position);
        }

        // Warte 10 Sekunden (Todesanimation + Liegenbleiben)
        yield return new WaitForSeconds(10f);

        Destroy(gameObject);
    }


}
