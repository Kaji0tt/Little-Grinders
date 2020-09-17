using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombat : MonoBehaviour
{ 
    IsometricPlayer isometricPlayer;
    //EnemyStats enemyStats;
    private void Start()
    {
        isometricPlayer = GetComponent<IsometricPlayer>();
    }

  public void AttackPlayer (IsometricPlayer playerStats)
    {
       playerStats.TakeDamage(isometricPlayer.AttackPower.Value);
    }
    
}
