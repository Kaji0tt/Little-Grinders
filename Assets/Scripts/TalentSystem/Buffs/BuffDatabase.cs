using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffDatabase : MonoBehaviour
{
    public static BuffDatabase instance;

    public List<Buff> allBuffs;

    private void Awake()
    {
        instance = this;
    }

    public BuffInstance GetInstance(string name)
    {
        foreach(Buff buff in allBuffs)
        {
            if (buff.name == name)
                return new BuffInstance(buff);
        }
        return null;
    }

}
