using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Very Basic. Poison should fetch the AP Values of it's origin.
public class Poison : Buff
{
    float tickTimer;
    float tick;

    public float damage;

    new float duration;
    // Start is called before the first frame update
    void Start()
    {

        tick = tickTimer;
    }

    // Update is called once per frame
    void Update()
    {
        
        tick -= Time.deltaTime;

        if(tick <= 0)
        {
            OnTick();
            tick = tickTimer;
        }


        duration -= Time.deltaTime;
        if (duration <= 0)
            Destroy(this);
    }

    private void OnTick()
    {
        PlayerStats pStats = this.gameObject.GetComponent<PlayerStats>();
        pStats.TakeDirectDamage(damage);
    }
}
