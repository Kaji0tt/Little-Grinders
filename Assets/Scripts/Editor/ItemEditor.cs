#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    private bool showPercentValues = true;
    private bool showFlatValues = true;

    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        // Versuche automatisch ItemType zu setzen (nur wenn noch None)
        if (item.itemType == ItemType.None)
        {
            string assetPath = AssetDatabase.GetAssetPath(item);
            string folderName = System.IO.Path.GetDirectoryName(assetPath).Split('/').Last().ToLower();

            switch (folderName)
            {
                case "waffen":
                    item.itemType = ItemType.Weapon;
                    break;
                case "hosen":
                    item.itemType = ItemType.Beine;
                    break;
                case "brueste":
                    item.itemType = ItemType.Brust;
                    break;
                case "kopf":
                    item.itemType = ItemType.Kopf;
                    break;
                case "schmuck":
                    item.itemType = ItemType.Schmuck;
                    break;
                case "consumable":
                    item.itemType = ItemType.Consumable;
                    break;
            }
        }

        // Standard Informationen
        item.ItemName = EditorGUILayout.TextField("Item Name", item.ItemName);
        item.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", item.itemType);
        item.level_needed = EditorGUILayout.IntField("Required Level", item.level_needed);
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
            item.p_hp = EditorGUILayout.FloatField("HP Boost (%)", item.p_hp);
            item.p_armor = EditorGUILayout.FloatField("Armor Boost (%)", item.p_armor);
            item.p_attackPower = EditorGUILayout.FloatField("Attack Power Boost (%)", item.p_attackPower);
            item.p_abilityPower = EditorGUILayout.FloatField("Ability Power Boost (%)", item.p_abilityPower);
            item.p_attackSpeed = EditorGUILayout.FloatField("Attack Speed Boost (%)", item.p_attackSpeed);
            item.p_movementSpeed = EditorGUILayout.FloatField("Movement Speed Boost (%)", item.p_movementSpeed);
        }

        // "Flat Values" ausklappbar machen
        showFlatValues = EditorGUILayout.Foldout(showFlatValues, "Flat Values");

        if (showFlatValues)
        {
            item.hp = EditorGUILayout.IntField("Health (+)", item.hp);
            item.armor = EditorGUILayout.IntField("Armor (+)", item.armor);
            item.attackPower = EditorGUILayout.IntField("Attack Power (+)", item.attackPower);
            item.abilityPower = EditorGUILayout.IntField("Ability Power (+)", item.abilityPower);
            item.reg = EditorGUILayout.IntField("Regeneration (+)", item.reg);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
    }
}
#endif
