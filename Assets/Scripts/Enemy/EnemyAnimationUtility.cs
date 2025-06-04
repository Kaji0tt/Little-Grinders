using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;

public static class EnemyAnimationUtility
{
    public static AnimatorController CreateGroupedAnimationsFromSpriteSheet(string baseName, Sprite spriteSheet)
    {
        var clipDefinitions = new Dictionary<string, List<(int startIndex, int frameCount)>>()
        {
            { "Idle",       new List<(int, int)> { (0, 8) } },
            { "Walk",       new List<(int, int)> { (8, 8) } },
            { "Attack1",    new List<(int, int)> { (16, 4) } },
            { "Attack2",    new List<(int, int)> { (20, 4) } },
            { "Casting",    new List<(int, int)> { (24, 8) } },
            { "Die1",       new List<(int, int)> { (32, 4) } },
            { "Die2",       new List<(int, int)> { (36, 4) } },
            { "Hit1",       new List<(int, int)> { (40, 4) } },
            { "Hit2",       new List<(int, int)> { (44, 4) } },
            { "Open1",      new List<(int, int)> { (48, 8) } },
            { "Open2",      new List<(int, int)> { (56, 8) } },
        };

        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            importer.spritePixelsPerUnit = 800;
            importer.SaveAndReimport();
        }

        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        Sprite[] sprites = allAssets
            .OfType<Sprite>()
            .OrderBy(s => ExtractNumber(s.name))
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning("❌ No sliced sprites found in the sprite sheet.");
            return null;
        }

        string animRootDir = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(animRootDir))
            AssetDatabase.CreateFolder("Assets", "Animations");

        string animDir = $"{animRootDir}/{baseName}";
        if (!AssetDatabase.IsValidFolder(animDir))
            AssetDatabase.CreateFolder(animRootDir, baseName);

        string controllerPath = $"{animDir}/{baseName}_Controller.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

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

            AnimationClip clip = new AnimationClip();
            clip.frameRate = 6;

            var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            if (clipName.StartsWith("Die"))
            {
                clipSettings.loopTime = false;
                clip.wrapMode = WrapMode.ClampForever;
            }
            else
            {
                clipSettings.loopTime = true;
            }

            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

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

            string clipPath = $"{animDir}/{baseName}_{clipName}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            var state = controller.AddMotion(clip);
            state.name = clipName;
        }

        Debug.Log($"✅ Generated {clipDefinitions.Count} animations and AnimatorController for '{baseName}' at '{animDir}'");
        return controller;
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
