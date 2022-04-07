using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Reflection : Buff
{
    void Start()
    {
        GameEvents.current.entitieAttacked += OnEntitieAttack;
    }

    private void OnEntitieAttack(float damage, GameObject entitie)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
