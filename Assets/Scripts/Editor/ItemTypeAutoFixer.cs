#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class ItemTypeAutoFixer
{
    [MenuItem("Tools/Fix All ItemTypes Based on Folder")]
    public static void FixAllItemTypes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Item", new[] { "Assets/Resources/Items" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(path);

            if (item == null || item.itemType != ItemType.None)
                continue;

            string folderName = Path.GetDirectoryName(path).Split('/').Last().ToLower();

            switch (folderName)
            {
                case "Waffen":
                    item.itemType = ItemType.Weapon;
                    break;
                case "Hosen":
                    item.itemType = ItemType.Beine;
                    break;
                case "Brueste":
                    item.itemType = ItemType.Brust;
                    break;
                case "Kopf":
                    item.itemType = ItemType.Kopf;
                    break;
                case "Schmuck":
                    item.itemType = ItemType.Schmuck;
                    break;
                case "Consumable":
                    item.itemType = ItemType.Gem;
                    break;
            }

            EditorUtility.SetDirty(item);
            Debug.Log($"Item '{item.name}' updated with ItemType '{item.itemType}'");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ItemType auto-fix complete.");
    }
}
#endif
