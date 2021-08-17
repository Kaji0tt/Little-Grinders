using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inter_Portal : Interactable
{

    public override void Use()
    {
        if(GetComponentInChildren<Light>())
        {
            GetComponentInChildren<Light>().intensity = 1;
        }

        GlobalMap.instance.currentMap.gotTeleporter = true;
        //später muss hier auch eingefügt werden, dass sich der Current MapSave um einen Teleport erweitert. Dieser muss so hinterlegt / abgespeichert werden,
        //dass dieser auch im Load an exakter Position neu spawned.
    }
}
