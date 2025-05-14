using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;

public static class EnemyAnimationUtility
{
    public static void CreateGroupedAnimationsFromSpriteSheet(string baseName, Sprite spriteSheet)
    {
        // Namen für die Animationen in Reihenfolge (je 8 Sprites pro Animation)
        string[] clipNames = { "Idle", "Walk", "Attack", "Casting", "Hitting", "Dying", "Open1", "Open2" };
        int framesPerClip = 8;

        // Pfad und ImportSettings holen
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            importer.spritePixelsPerUnit = 800;
            importer.SaveAndReimport(); // Änderungen anwenden
        }

        // Alle Sprites aus dem SpriteSheet laden
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        Sprite[] sprites = allAssets
        .OfType<Sprite>()
        .OrderBy(s => ExtractNumber(s.name))
        .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning("❌ No sliced sprites found in the sprite sheet.");
            return;
        }

        // Zielordner erstellen: Assets/Animations/<baseName>/
        string animRootDir = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(animRootDir))
            AssetDatabase.CreateFolder("Assets", "Animations");

        string animDir = $"{animRootDir}/{baseName}";
        if (!AssetDatabase.IsValidFolder(animDir))
            AssetDatabase.CreateFolder(animRootDir, baseName);

        // AnimatorController erzeugen
        string controllerPath = $"{animDir}/{baseName}_Controller.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Gruppiere Sprites in feste 8er-Blöcke
        var groups = new Dictionary<string, List<Sprite>>();
        for (int i = 0; i < clipNames.Length; i++)
        {
            string clipName = clipNames[i];
            int startIndex = i * framesPerClip;

            if (startIndex + framesPerClip > sprites.Length)
            {
                Debug.LogWarning($"⚠️ Not enough sprites for clip '{clipName}'. Skipping.");
                continue;
            }

            List<Sprite> clipSprites = sprites.Skip(startIndex).Take(framesPerClip).ToList();
            groups.Add(clipName, clipSprites);
        }

        // Für jede Gruppe eine Animation erstellen und zum Controller hinzufügen
        foreach (var group in groups)
        {
            string clipName = group.Key;
            List<Sprite> frames = group.Value;

            AnimationClip clip = new AnimationClip();
            clip.frameRate = 6;

            // Loop aktivieren
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[frames.Count];
            for (int i = 0; i < frames.Count; i++)
            {
                keyFrames[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyFrames);

            string clipPath = $"{animDir}/{baseName}_{clipName}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            var state = controller.AddMotion(clip);
            state.name = clipName; // ⬅️ Nur "Idle", "Walk", etc. im State-Machine-Namen
        }

        // 🔁 AnimatorController dem aktiven GameObject zuweisen
        GameObject selectedGO = Selection.activeGameObject;
        if (selectedGO != null)
        {
            Animator animator = selectedGO.GetComponent<Animator>();
            if (animator == null)
                animator = selectedGO.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;

            Debug.Log($"✅ {baseName} AnimatorController wurde dem GameObject '{selectedGO.name}' zugewiesen.");
        }
        else
        {
            Debug.LogWarning("⚠️ Kein GameObject ausgewählt. AnimatorController konnte nicht zugewiesen werden.");
        }

        Debug.Log($"✅ Generated {groups.Count} animations and AnimatorController for '{baseName}' at '{animDir}'");
    }

    private static object ExtractNumber(string name)
    {
        string number = new string(name.Reverse()
            .TakeWhile(char.IsDigit)
            .Reverse()
            .ToArray());

        return int.TryParse(number, out int result) ? result : 0;
    }
}