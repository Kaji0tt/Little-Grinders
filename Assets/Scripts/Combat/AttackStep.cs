using UnityEngine;

[System.Serializable]
public class AttackStep
{
    // Für den Troubleshoot wird animationClip ausgetauscht mit einem Namen.
    // Später sollten Animationen manuell erstellt werden über Transform-Pos Änderung.
    public AnimationClip animationClip;
    //public string attackStepName;
    public float damageMultiplier = 1f;
    public float timeToNextAttack = 1f; // Optional: Zeitfenster für die nächste Eingabe


    /// <summary>
    /// Wenn du später z. B. zwischen Nahkampf- und Fernkampf-Waffen unterscheiden willst, 
    /// kannst du deine AttackStep auch noch um Felder erweitern wie:
    /// </summary>
    //public bool isRanged;
    //public GameObject projectilePrefab;
}