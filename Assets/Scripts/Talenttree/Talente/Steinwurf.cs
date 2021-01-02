using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steinwurf : Spell, IUseable
{


    IsometricPlayer player;

    public void Use()
    {

        player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();
        if (!onCoolDown)
        {



            onCoolDown = true;
        }
    }


}
