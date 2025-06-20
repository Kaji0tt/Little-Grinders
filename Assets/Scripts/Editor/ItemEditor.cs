#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    private bool showPercentValues = true;
    private bool showFlatValues = true;

    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        // Standard Informationen
        item.ItemName = EditorGUILayout.TextField("Item Name", item.ItemName);
        item.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", item.itemType);
        item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);

        // Textarea für Beschreibung
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        item.ItemDescription = EditorGUILayout.TextArea(item.ItemDescription, GUILayout.MinHeight(60));

        // Falls Waffe
        if (item.itemType == ItemType.Weapon)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Settings", EditorStyles.boldLabel);
            item.weaponCombo = (WeaponCombo)EditorGUILayout.ObjectField("Weapon Combo", item.weaponCombo, typeof(WeaponCombo), false);
        }

        // "Percent Values" ausklappbar machen
        showPercentValues = EditorGUILayout.Foldout(showPercentValues, "Percent Values");

        if (showPercentValues)
        {
            // Beispiel: item.attackSpeedBoost, item.criticalChanceBoost etc.
            item.p_hp = EditorGUILayout.FloatField("HP Boost (%)", item.p_hp);
            item.p_armor = EditorGUILayout.FloatField("Armor Boost (%)", item.p_armor);
            item.p_attackPower = EditorGUILayout.FloatField("Attack Power Boost (%)", item.p_attackPower);
            item.p_abilityPower = EditorGUILayout.FloatField("Ability Power Boost (%)", item.p_abilityPower);
            item.p_attackSpeed = EditorGUILayout.FloatField("Attack Speed Boost (%)", item.p_attackSpeed);
            item.p_movementSpeed = EditorGUILayout.FloatField("Movement Speed Boost (%)", item.p_movementSpeed);

    // weitere percentbasierte Werte...
        }

        // "Flat Values" ausklappbar machen
        showFlatValues = EditorGUILayout.Foldout(showFlatValues, "Flat Values");

        if (showFlatValues)
        {
            // Beispiel: item.damage, item.health, etc.
            item.hp = EditorGUILayout.IntField("Health (+)", item.hp);
            item.armor = EditorGUILayout.IntField("Armor (+)", item.armor);
            item.attackPower = EditorGUILayout.IntField("Attack Power (+)", item.attackPower);
            item.abilityPower = EditorGUILayout.IntField("Ability Power (+)", item.abilityPower);
            item.reg = EditorGUILayout.IntField("Regeneration (+)", item.reg);
            // weitere flat Werte...
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
    }
}
#endif