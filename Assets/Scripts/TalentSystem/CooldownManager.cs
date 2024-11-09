using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    public static CooldownManager instance;

    private Dictionary<IUseable, float> cooldownTimers = new Dictionary<IUseable, float>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void Register(IUseable useable)
    {
        if (!cooldownTimers.ContainsKey(useable))
            cooldownTimers.Add(useable, 0f);
    }

    public void UpdateCooldowns()
    {
        List<IUseable> useablesOnCooldown = new List<IUseable>(cooldownTimers.Keys);

        foreach (IUseable useable in useablesOnCooldown)
        {
            if (cooldownTimers[useable] > 0)
            {
                cooldownTimers[useable] -= Time.deltaTime;

                if (cooldownTimers[useable] <= 0)
                {
                    /*
                    ability.RecoverCharge();
                    cooldownTimers[ability] = ability.IsFullyCharged ? 0 : ability.cooldownTime;
                    */
                }
            }
        }
    }

    public bool IsOnCooldown(IUseable useable)
    {
        return cooldownTimers.ContainsKey(useable) && cooldownTimers[useable] > 0;
    }

    public float GetCooldownTime(IUseable useable)
    {
        return cooldownTimers.TryGetValue(useable, out float cooldown) ? cooldown : 0f;
    }

    public float GetCooldownTimeRemaining(IUseable useable)
    {
        return Mathf.Max(0, cooldownTimers[useable]);
    }

}
