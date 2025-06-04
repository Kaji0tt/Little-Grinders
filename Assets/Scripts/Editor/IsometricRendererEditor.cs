using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IsometricRenderer))]
public class IsometricRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        IsometricRenderer iso = (IsometricRenderer)target;

        if (iso.spriteSheet == null)
        {
            EditorGUILayout.HelpBox("Bitte ein SpriteSheet zuweisen!", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("🎞 Generate Animations from SpriteSheet"))
        {
            var controller = EnemyAnimationUtility.CreateGroupedAnimationsFromSpriteSheet(iso.spriteSheet.name, iso.spriteSheet);
            if (controller != null)
            {
                Animator animator = iso.gameObject.GetComponent<Animator>();
                if (animator == null)
                    animator = iso.gameObject.AddComponent<Animator>();

                animator.runtimeAnimatorController = controller;

                // Optional: GameObject im Editor hervorheben
                Selection.activeGameObject = iso.gameObject;
                EditorGUIUtility.PingObject(iso.gameObject);
            }
        }
    }
}
