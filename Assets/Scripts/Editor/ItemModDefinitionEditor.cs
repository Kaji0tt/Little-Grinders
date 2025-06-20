using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemModDefinition))]
public class ItemModDefinitionEditor : Editor
{
    private bool showPreview = true;
    private float previewBaseValue = 0.05f; // z. B. 5% für Crit Chance

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemModDefinition modDef = (ItemModDefinition)target;

        GUILayout.Space(10);
        showPreview = EditorGUILayout.Foldout(showPreview, "Vorschau – Mod-Werte bei Level 15 & 50", true);

        if (!showPreview) return;

        if (modDef.rarityScalings == null || modDef.rarityScalings.Length == 0)
        {
            EditorGUILayout.HelpBox("Keine Rarity-Scalings definiert.", MessageType.Warning);
            return;
        }

        GUILayout.BeginVertical("box");

        bool isPercentMod = modDef.modType == ModType.Percent || modDef.modType == ModType.PercentFortune;

        if (isPercentMod)
        {
            EditorGUILayout.LabelField("Prozent-Vorschau", EditorStyles.boldLabel);
            previewBaseValue = EditorGUILayout.FloatField("Basiswert (z. B. 0.05 für 5%)", previewBaseValue);
            GUILayout.Space(5);
        }

        EditorGUILayout.LabelField("Rarity", "Wert @ L15  /  Wert @ L50");

        foreach (var rarityScaling in modDef.rarityScalings)
        {
            float value15 = modDef.GetValue(15, rarityScaling.rarity);
            float value50 = modDef.GetValue(50, rarityScaling.rarity);

            string rarityName = rarityScaling.rarity.ToString();

            if (isPercentMod)
            {
                float result15 = previewBaseValue + previewBaseValue * value15;
                float result50 = previewBaseValue + previewBaseValue * value50;

                EditorGUILayout.LabelField(
                    $"{rarityName,-10}:  +{value15:P1} ({result15:P1})  /  +{value50:P1} ({result50:P1})");
            }
            else
            {
                EditorGUILayout.LabelField(
                    $"{rarityName,-10}:  +{value15:F2} / +{value50:F2}");
            }
        }

        GUILayout.EndVertical();
    }
}
