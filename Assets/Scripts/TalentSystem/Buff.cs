using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


///Buffsystem oder StatusEffekt?
///StatusEffekt -> Buff -> Dornen

/// Ein Buff ist eine eigene Klasse. Ihr liegt ein oder mehrere StatModifier zu Grunde.
/// 
/// StatMod
/// Duration
/// ParticelSystems
/// Selbst geschriebener Effekt (Angreifer erleiden schaden...)
///                             
///
///Ein Buff ist ein StatusEffekt, welcher �ber eine angegebene Zeitspanne entweder 
///                                                                  a) einen StatModifier hizuf�gt, oder 
///                                                                  b) �ber Ticks auswirkungen auf PlayerStats oder MobStats hat.
///(Angreifer erleiden schaden...) Dornen
///Immer wenn TakeDamage gecalled wird, muss 'damage / 10' als TakeDamage auf die Origin der Klasse zur�ck gecalled werden.. irgendwie so.
///Wenn wir nicht in die TakeDamage Funktion eingreifen wollen, kann dies lediglich �ber das Event-System geschehen.
///Das BuffSystem h�rt dem Event Attack() von Mobs zu, falls getriggered, calle auf der gleichen Instanz TakeDamage(damage / 10)

public class Buff : MonoBehaviour
{

    public float duration;

    public StatModifier mod;

    public GameObject particleEffect;

    //Buffs �bers EventSystem?

    //
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
