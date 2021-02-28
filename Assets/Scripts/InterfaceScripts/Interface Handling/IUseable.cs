using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseable
{
    void Use();

    bool IsOnCooldown();

    float GetCooldown();

    float CooldownTimer();
}
