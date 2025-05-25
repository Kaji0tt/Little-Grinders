using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(EnemyController))]
public class EnemyControllerEditor : Editor
{
    private static readonly string materialPath = "Materials/SpriteMAT URP";

    private void OnEnable()
    {
        ApplySpriteSettings((EnemyController)target);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Manuell Sprite-Einstellungen anwenden"))
        {
            ApplySpriteSettings((EnemyController)target);
        }
    }

    private static void ApplySpriteSettings(EnemyController controller)
    {
        var go = controller.gameObject;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"'{go.name}' hat keinen SpriteRenderer – übersprungen.");
            return;
        }

        Material spriteMat = Resources.Load<Material>(materialPath);
        if (spriteMat == null)
        {
            Debug.LogError($"Material '{materialPath}' nicht gefunden in Resources.");
            return;
        }

        Undo.RecordObject(sr, "Auto-Apply Sprite Settings");

        sr.material = spriteMat;
        sr.shadowCastingMode = ShadowCastingMode.On;
        sr.receiveShadows = true;

        EditorUtility.SetDirty(sr);
        Debug.Log($"✅ SpriteRenderer von '{go.name}' automatisch konfiguriert.");
    }
}