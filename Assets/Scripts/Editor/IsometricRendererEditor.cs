using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IsometricRenderer))]
public class IsometricRendererEditor : Editor
{
    SerializedProperty weaponAnimatorProp;
    SerializedProperty isPerformingActionProp;
    SerializedProperty weaponPivotProp;
    SerializedProperty spriteSheetProp;
    SerializedProperty mirrorSpritesheetProp;
    SerializedProperty turnOffIsometricRendererProp;

    void OnEnable()
    {
        weaponAnimatorProp = serializedObject.FindProperty("weaponAnimator");
        isPerformingActionProp = serializedObject.FindProperty("isPerformingAction");
        weaponPivotProp = serializedObject.FindProperty("weaponPivot");
        spriteSheetProp = serializedObject.FindProperty("spriteSheet");
        mirrorSpritesheetProp = serializedObject.FindProperty("mirrorSpritesheet");
        turnOffIsometricRendererProp = serializedObject.FindProperty("turnOffIsometricRenderer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        IsometricRenderer iso = (IsometricRenderer)target;

        bool isCharacter = iso.CompareTag("Player");

        if (isCharacter)
        {
            EditorGUILayout.PropertyField(weaponAnimatorProp);
            EditorGUILayout.PropertyField(isPerformingActionProp);
            EditorGUILayout.PropertyField(weaponPivotProp);
        }
        else
        {
            EditorGUILayout.PropertyField(spriteSheetProp);
            EditorGUILayout.PropertyField(mirrorSpritesheetProp);

            // Intro Mob Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Disable IsoRenderer", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(turnOffIsometricRendererProp);

            if (iso.spriteSheet == null)
            {
                EditorGUILayout.HelpBox("Bitte ein SpriteSheet zuweisen!", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("🎞 Generate Animations from SpriteSheet"))
                {
                    var controller = EnemyAnimationUtility.CreateGroupedAnimationsFromSpriteSheet(iso.spriteSheet.name, iso.spriteSheet);
                    if (controller != null)
                    {
                        Animator animator = iso.gameObject.GetComponent<Animator>();
                        if (animator == null)
                            animator = iso.gameObject.AddComponent<Animator>();

                        animator.runtimeAnimatorController = controller;

                        Selection.activeGameObject = iso.gameObject;
                        EditorGUIUtility.PingObject(iso.gameObject);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
