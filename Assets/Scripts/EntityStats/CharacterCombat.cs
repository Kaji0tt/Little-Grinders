using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{ 

    //gerade Tote CS - außerdem: get isometricPlayer through PlayerManager
    IsometricPlayer playerStats;
    //EnemyStats enemyStats;
    private void Start()
    {
        playerStats = GetComponent<IsometricPlayer>();
    }



    /*
    public void AttackPlayer(IsometricPlayer playerStats)
    {
        playerStats.TakeDamage(isometricPlayer.AttackPower.Value);
    }
    */

}
