using UnityEngine;
using UnityEditor;

// In einem Ordner namens "Editor"
[CustomEditor(typeof(AbilityData))]
public class AbilityDataEditor : Editor
{
    // Serialized Properties für alle Felder in AbilityData
    private SerializedProperty abilityNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty iconProp;
    private SerializedProperty propertiesProp;
    private SerializedProperty abilityPrefabProp;
    private SerializedProperty rangeProp;
    private SerializedProperty areaOfEffectRadiusProp;
    private SerializedProperty projectilePrefabProp;
    private SerializedProperty projectileSpeedProp;
    private SerializedProperty channelTimeProp;
    private SerializedProperty activeTimeProp;
    private SerializedProperty tickTimerProp;
    private SerializedProperty cooldownTimeProp;
    private SerializedProperty maxChargesProp;

    private void OnEnable()
    {
        // Verknüpfe die Properties beim Aktivieren des Editors
        abilityNameProp = serializedObject.FindProperty("abilityName");
        descriptionProp = serializedObject.FindProperty("description");
        iconProp = serializedObject.FindProperty("icon");
        propertiesProp = serializedObject.FindProperty("properties");
        abilityPrefabProp = serializedObject.FindProperty("abilityPrefab");
        rangeProp = serializedObject.FindProperty("range");
        areaOfEffectRadiusProp = serializedObject.FindProperty("areaOfEffectRadius");
        projectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
        projectileSpeedProp = serializedObject.FindProperty("projectileSpeed");
        channelTimeProp = serializedObject.FindProperty("channelTime");
        activeTimeProp = serializedObject.FindProperty("activeTime");
        tickTimerProp = serializedObject.FindProperty("tickTimer");
        cooldownTimeProp = serializedObject.FindProperty("cooldownTime");
        maxChargesProp = serializedObject.FindProperty("maxCharges");
    }

    public override void OnInspectorGUI()
    {
        // Lade den aktuellen Zustand des Objekts
        serializedObject.Update();

        // --- Zeichne die immer sichtbaren Felder ---
        EditorGUILayout.LabelField("Core Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(abilityNameProp);
        EditorGUILayout.PropertyField(descriptionProp);
        EditorGUILayout.PropertyField(iconProp);
        EditorGUILayout.PropertyField(propertiesProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Logic Prefab", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(abilityPrefabProp);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Mechanics", EditorStyles.boldLabel);

        // --- Zeichne die konditionalen Felder basierend auf den Flags ---
        SpellProperty properties = (SpellProperty)propertiesProp.enumValueFlag;

        if (properties.HasFlag(SpellProperty.Ranged))
            EditorGUILayout.PropertyField(rangeProp);

        if (properties.HasFlag(SpellProperty.AoE))
            EditorGUILayout.PropertyField(areaOfEffectRadiusProp);

        if (properties.HasFlag(SpellProperty.Projectile))
        {
            EditorGUILayout.PropertyField(projectilePrefabProp);
            EditorGUILayout.PropertyField(projectileSpeedProp);
        }

        if (properties.HasFlag(SpellProperty.Active))
        {
            EditorGUILayout.PropertyField(activeTimeProp, new GUIContent("Active Duration"));
            EditorGUILayout.PropertyField(tickTimerProp, new GUIContent("Active Tick Rate"));
        }

        if (properties.HasFlag(SpellProperty.Channeling))
        {
            EditorGUILayout.PropertyField(channelTimeProp);
            EditorGUILayout.PropertyField(tickTimerProp, new GUIContent("Channeling Tick Rate"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Core Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cooldownTimeProp);
        EditorGUILayout.PropertyField(maxChargesProp);

        // Speichere alle Änderungen, die im Inspector gemacht wurden
        serializedObject.ApplyModifiedProperties();
    }
}