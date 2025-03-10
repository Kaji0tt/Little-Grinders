using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ability), true)] // "true" sorgt dafür, dass auch Unterklassen (wie Heal) erfasst werden
public class EditorAbilityInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Ability myAbility = (Ability)target;
        SerializedObject so = new SerializedObject(myAbility);

        // Zeichne explizit den "isPersistent"-Toggle
        SerializedProperty isPersistentProp = so.FindProperty("isPersistent");
        EditorGUILayout.PropertyField(isPersistentProp);

        // Wenn isPersistent NICHT aktiv ist, zeige "activeTime"
        if (!isPersistentProp.boolValue)
        {
            SerializedProperty activeTimeProp = so.FindProperty("activeTime");
            EditorGUILayout.PropertyField(activeTimeProp);
        }

        // Alle anderen Felder anzeigen
        DrawPropertiesExcluding(so, "m_Script", "isPersistent", "activeTime");

        so.ApplyModifiedProperties();
    }
}