using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponCombo", menuName = "Combat/Weapon Combo")]
public class WeaponCombo : ScriptableObject
{
    public List<AttackStep> comboSteps;

    public float comboCooldown = 1; // Zeit zwischen den Schritten der Kombos

}