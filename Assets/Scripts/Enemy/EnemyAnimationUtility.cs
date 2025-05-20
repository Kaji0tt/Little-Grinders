using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;

public static class EnemyAnimationUtility
{
    public static void CreateGroupedAnimationsFromSpriteSheet(string baseName, Sprite spriteSheet)
    {
        // Animationen mit beliebigen Frame-Gruppen definieren: (Startindex, Frameanzahl)
        var clipDefinitions = new Dictionary<string, List<(int startIndex, int frameCount)>>()
        {
            { "Idle",   new List<(int, int)> { (0, 8) } },
            { "Walk",   new List<(int, int)> { (8, 8) } },
            { "Attack", new List<(int, int)> { (16, 4), (20, 4) } }, // Zwei 4er-Blöcke für Attack
            { "Casting",new List<(int, int)> { (24, 8) } },
            { "Hit",    new List<(int, int)> { (28, 4), (32, 4) } }, // Zwei 4er-Blöcke für Hit
            { "Die",    new List<(int, int)> { (36, 4), (40, 4) } }, // Zwei 4er-Blöcke für Die
            { "Open1",  new List<(int, int)> { (48, 8) } },
            { "Open2",  new List<(int, int)> { (56, 8) } },
        };

        // Pfad und ImportSettings holen
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            importer.spritePixelsPerUnit = 800;
            importer.SaveAndReimport(); // Änderungen anwenden
        }

        // Alle Sprites aus dem SpriteSheet laden und sortieren
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

        // Animationen aus Sprite-Blöcken bauen
        foreach (var kvp in clipDefinitions)
        {
            string clipName = kvp.Key;
            List<(int, int)> ranges = kvp.Value;

            List<Sprite> clipSprites = new();

            foreach (var (startIndex, frameCount) in ranges)
            {
                if (startIndex + frameCount > sprites.Length)
                {
                    Debug.LogWarning($"⚠️ Not enough sprites for clip '{clipName}'. Skipping.");
                    continue;
                }

                clipSprites.AddRange(sprites.Skip(startIndex).Take(frameCount));
            }

            if (clipSprites.Count == 0)
                continue;

            // AnimationClip erstellen
            AnimationClip clip = new AnimationClip();
            clip.frameRate = 6;

            // Loop aktivieren
            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

            // Sprite Keyframes setzen
            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[clipSprites.Count];
            for (int i = 0; i < clipSprites.Count; i++)
            {
                keyFrames[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = clipSprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyFrames);

            // Clip speichern
            string clipPath = $"{animDir}/{baseName}_{clipName}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            var state = controller.AddMotion(clip);
            state.name = clipName;
        }

        // AnimatorController dem aktiven GameObject zuweisen
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

        Debug.Log($"✅ Generated {clipDefinitions.Count} animations and AnimatorController for '{baseName}' at '{animDir}'");
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