using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponCombo", menuName = "Combat/Weapon Combo")]
public class WeaponCombo : ScriptableObject
{
    public List<AttackStep> comboSteps;

    public float comboCooldown = 1; // Zeit zwischen den Schritten der Kombos

    public GameObject trailEffect; // Effekt, der bei den Kombos angezeigt wird

    public GameObject TrailEffectOrDefault
    {
        get
        {
            if (trailEffect != null)
                return trailEffect;
            // Standard aus Resources laden
            return Resources.Load<GameObject>("VFX/Combo/Sword_1/CFX_Sword Trail Effect");
        }
    }

}