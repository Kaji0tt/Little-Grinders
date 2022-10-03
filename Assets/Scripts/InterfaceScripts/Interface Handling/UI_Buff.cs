using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Könnte für Buffs ebensow ie Debuffs verwendet werden.
public class UI_Buff : MonoBehaviour
{
    #region Singleton
    public static UI_Buff instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public GameObject templateBuff;

    public List<UI_BuffHolder> activeUIBuffs = new List<UI_BuffHolder>();

    public List<BuffInstance> activeBuffs = new List<BuffInstance>();

    private List<BuffInstance> newBuffs = new List<BuffInstance>();

    private List<BuffInstance> expiredBuffs = new List<BuffInstance>();

    //Erfasse alle Kinder und überprüfe ob der in ihnen gespeicherte Buff den gleichen Namen hat, wie der neue Buff.
    //Falls ja, entferne das alte GameObject und füge das neue hinzu
    //Falls nein, mache nichts.

    ///Approach New:
    ///Wenn der Spieler in seiner aktiven Liste einen neuen Buff erhält, füge eine Kopie vom UI_BuffHolder mit der BuffInstanz hinzu.
    ///Der BuffHolder sollte überprüfen, wie hoch die ZeitIntervall des aktiven Buffs ist. Falls dieses kleiner als 0 ist, soll er sich löschen.
    
    public void ApplyUIBuff(BuffInstance newBuff)
    {
        //Überprüfe ob es bereits eine Instanz dieses Buffs gibt.
        if(DoesBuffExist(newBuff) != null)
        {
            //Falls ja, zerstöre das UI GO auf dem der Buff liegt.
            DoesBuffExist(newBuff).DestroyGameObject();

        }

        //füge eine Kopie vom UI_BuffHolder 
        GameObject tmp = Instantiate(templateBuff, this.gameObject.transform);

        //Setting Sprite
        tmp.GetComponent<Image>().sprite = newBuff.icon;

        tmp.SetActive(true);

        //mit der BuffInstanz hinzu.
        tmp.GetComponent<UI_BuffHolder>().buff = newBuff;


        /*
        //Erfasse alle Kinder
        foreach (UI_BuffHolder uiBuff in GetComponentsInChildren<UI_BuffHolder>())
        {
            activeUIBuffs.Add(uiBuff);
        }
        //Falls ein neuer Buff hinzugefügt wird, überprüfe ob dieser Stackbar ist.
        if (!buff.stackable)
        {
            //Falls nicht, erstelle ein Template des Buffs, falls er sich bereits in der aktiven Liste befindet.
            BuffInstance tmp = activeUIBuffs.Find(x.buff => x.buff.buffName == buff.buffName);

            if (tmp != null)
            {
                //Falls ein aktiver Buff vorhanden war, erneuere ihn mit dem neuen Buff.
                expiredBuffs.Add(tmp);
            }

            CreateUIBuff(buff);
        }

        activeBuffs.Add(buff);
        */

    }

    public UI_BuffHolder DoesBuffExist(BuffInstance newBuff)
    {
        UI_BuffHolder[] allUIBUffs = GetComponentsInChildren<UI_BuffHolder>();

        if(allUIBUffs.Length > 0)
        foreach(UI_BuffHolder uiBuff in allUIBUffs)
        {
            if (uiBuff.buff.buffName == newBuff.buffName)
                return uiBuff;

            else return null;
        }

        print("Error occured circling through UI_BuffHolder.cs to return active Buffs.");
        return null;

    }

    public void CreateUIBuff(BuffInstance newBuff)
    {
        /*
         GameObject tmp = Instantiate(templateBuff, this.gameObject.transform);


        tmp.GetComponent<UI_BuffHolder>().buff = newBuff.buffName;
        tmp.GetComponent<Image>().sprite = buff.icon;
        
        tmp.SetActive(true);
        */


    }
    

}
