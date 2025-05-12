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
            EnemyAnimationUtility.CreateGroupedAnimationsFromSpriteSheet(iso.spriteSheet.name, iso.spriteSheet);
        }
    }
}