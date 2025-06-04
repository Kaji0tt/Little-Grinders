#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        // Zeichne Standardfelder
        item.ItemName = EditorGUILayout.TextField("Item Name", item.ItemName);
        item.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", item.itemType);
        item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);

        // Nur wenn es eine Waffe ist, zeige das Combo-Feld
        if (item.itemType == ItemType.Weapon)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Settings", EditorStyles.boldLabel);
            item.weaponCombo = (WeaponCombo)EditorGUILayout.ObjectField("Weapon Combo", item.weaponCombo, typeof(WeaponCombo), false);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);

        }
    }
}
#endif