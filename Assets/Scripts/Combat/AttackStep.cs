using UnityEditor.EditorTools;
using UnityEngine;

[System.Serializable]
public class AttackStep
{
    // Für den Troubleshoot wird animationClip ausgetauscht mit einem Namen.
    // Später sollten Animationen manuell erstellt werden über Transform-Pos Änderung.
    public AnimationClip animationClip;
    //public string attackStepName;
    [Tooltip("Multiplikator für den Schaden dieses AttackSteps")]
    public float damageMultiplier = 1f;
    [Tooltip("Wie lange dieser AttackStep dauern soll")]
    public float timeForAttackStep = 1f; // Optional: Zeitfenster für die nächste Eingabe
                                         //[Tooltip("Zeit für Combo-Input (unabhängig von AttackStep-Dauer)")]
    public float dashDistance = .5f;
    //public float comboWindowTime = 0.8f; // Zeit für Combo-Input (unabhängig von AttackStep-Dauer)


    /// <summary>
    /// Wenn du später z. B. zwischen Nahkampf- und Fernkampf-Waffen unterscheiden willst, 
    /// kannst du deine AttackStep auch noch um Felder erweitern wie:
    /// </summary>
    //public bool isRanged;
    //public GameObject projectilePrefab;
}