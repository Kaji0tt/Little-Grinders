using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(EnemyController))]
public class EnemyControllerEditor : Editor
{
    private static readonly string materialPath = "SpriteMAT URP";

    private void OnEnable()
    {
        ApplySpriteSettings((EnemyController)target);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyController controller = (EnemyController)target;

        // Check für IAttackBehavior Component
        CheckForAttackBehavior(controller);

        if (GUILayout.Button("Manuell Sprite-Einstellungen anwenden"))
        {
            ApplySpriteSettings(controller);
        }
    }

    private void CheckForAttackBehavior(EnemyController controller)
    {
        // Suche nach IAttackBehavior Components
        var behaviorComponents = controller.GetComponents<MonoBehaviour>();
        bool hasAttackBehavior = false;

        foreach (var component in behaviorComponents)
        {
            if (component is IAttackBehavior)
            {
                hasAttackBehavior = true;
                break;
            }
        }

        if (!hasAttackBehavior)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Kein IAttackBehavior Component gefunden!", MessageType.Warning);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("PendingAttack hinzufügen"))
            {
                Undo.AddComponent<PendingAttack>(controller.gameObject);
                EditorUtility.SetDirty(controller);
            }

            // Hier können Sie weitere Attack Behaviors hinzufügen
            if (GUILayout.Button("Anderes Behavior hinzufügen"))
            {
                ShowAttackBehaviorMenu(controller);
            }

            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("IAttackBehavior Component gefunden.", MessageType.Info);
        }
    }

    private void ShowAttackBehaviorMenu(EnemyController controller)
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("PendingAttack"), false, () => {
            Undo.AddComponent<PendingAttack>(controller.gameObject);
            EditorUtility.SetDirty(controller);
        });

        // Hier können Sie weitere Attack Behavior Types hinzufügen:
        // menu.AddItem(new GUIContent("RangedAttack"), false, () => {
        //     Undo.AddComponent<RangedAttack>(controller.gameObject);
        //     EditorUtility.SetDirty(controller);
        // });

        menu.ShowAsContext();
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