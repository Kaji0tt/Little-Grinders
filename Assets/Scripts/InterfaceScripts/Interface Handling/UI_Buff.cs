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

    //Erfasse alle Kinder und �berpr�fe ob der in ihnen gespeicherte Buff den gleichen Namen hat, wie der neue Buff.
    //Falls ja, entferne das alte GameObject und f�ge das neue hinzu
    //Falls nein, mache nichts.

    ///Approach New:
    ///Wenn der Spieler in seiner aktiven Liste einen neuen Buff erh�lt, f�ge eine Kopie vom UI_BuffHolder mit der BuffInstanz hinzu.
    ///Der BuffHolder sollte �berpr�fen, wie hoch die ZeitIntervall des aktiven Buffs ist. Falls dieses kleiner als 0 ist, soll er sich l�schen.
    
    public void ApplyUIBuff(BuffInstance newBuff)
    {
        //�berpr�fe ob es bereits eine Instanz dieses Buffs gibt.
        if(DoesBuffExist(newBuff) != null)
        {
            //Falls ja, zerst�re das UI GO auf dem der Buff liegt.
            DoesBuffExist(newBuff).DestroyGameObject();

        }

        //f�ge eine Kopie vom UI_BuffHolder 
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
        //Falls ein neuer Buff hinzugef�gt wird, �berpr�fe ob dieser Stackbar ist.
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
        UI_BuffHolder[] allUIBuffs = GetComponentsInChildren<UI_BuffHolder>();

        if (allUIBuffs.Length > 0)
        {
            foreach (UI_BuffHolder uiBuff in allUIBuffs)
            {
                // Prüfe ob buff null ist (ScriptableObject kann destroyed sein)
                if (uiBuff.buff == null)
                {
                    Debug.LogWarning($"[UI_Buff] UI_BuffHolder hat null buff! Überspringe...");
                    continue;
                }

                // Prüfe ob buffName übereinstimmt
                if (uiBuff.buff.buffName == newBuff.buffName)
                {
                    return uiBuff; // ✅ Gefunden!
                }
                // ✅ KEIN else return null hier! Weiter mit nächstem Element
            }
        }

        // ✅ Nur null zurückgeben wenn ALLE durchlaufen wurden
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

    /// <summary>
    /// Entfernt den UI-Buff für die angegebene BuffInstance
    /// </summary>
    public void RemoveUIBuff(BuffInstance buff)
    {
        if (buff == null)
        {
            Debug.LogWarning("[UI_Buff] RemoveUIBuff - Buff ist null!");
            return;
        }

        // Finde den entsprechenden UI_BuffHolder
        UI_BuffHolder holder = DoesBuffExist(buff);
        
        if (holder != null)
        {
            Debug.Log($"[UI_Buff] Entferne UI für Buff: {buff.buffName}");
            holder.DestroyGameObject();
        }
        else
        {
            Debug.LogWarning($"[UI_Buff] RemoveUIBuff - Kein UI_BuffHolder für '{buff.buffName}' gefunden!");
        }
    }
    

}
