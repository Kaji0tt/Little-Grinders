using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponCombo", menuName = "Combat/Weapon Combo")]
public class WeaponCombo : ScriptableObject
{
    public List<AttackStep> comboSteps;
}