#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ItemIDAssigner : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool itemsChanged = false;

        // Prüfe ob Items importiert oder verändert wurden
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.Contains("Resources/Items") && assetPath.EndsWith(".asset"))
            {
                Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);
                if (item != null && string.IsNullOrEmpty(item.ItemID))
                {
                    AssignUniqueID(item, assetPath);
                    itemsChanged = true;
                }
            }
        }

        if (itemsChanged)
        {
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Tools/Items/Reassign All Item IDs")]
    private static void ReassignAllItemIDs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Item", new[] { "Assets/Resources/Items" });
        int assignedCount = 0;
        int skippedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

            if (item != null)
            {
                if (string.IsNullOrEmpty(item.ItemID))
                {
                    AssignUniqueID(item, assetPath);
                    assignedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ItemIDAssigner] Fertig! {assignedCount} IDs vergeben, {skippedCount} übersprungen (hatten bereits IDs)");
    }

    [MenuItem("Tools/Items/Force Reassign ALL Item IDs (Overwrite Existing)")]
    private static void ForceReassignAllItemIDs()
    {
        if (!EditorUtility.DisplayDialog(
            "Force Reassign Item IDs",
            "ACHTUNG: Dies überschreibt ALLE existierenden Item-IDs!\n\nDies kann Savegames beschädigen!\n\nFortfahren?",
            "Ja, alles überschreiben",
            "Abbrechen"))
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Item", new[] { "Assets/Resources/Items" });
        int assignedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

            if (item != null)
            {
                AssignUniqueID(item, assetPath, forceReassign: true);
                assignedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ItemIDAssigner] {assignedCount} IDs neu vergeben (FORCE)");
    }

    private static void AssignUniqueID(Item item, string assetPath, bool forceReassign = false)
    {
        // Generiere ID basierend auf ItemType und Ordner
        string itemTypePrefix = GetItemTypePrefix(item.itemType);
        string folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(assetPath));
        string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        // Bereinige Dateinamen von Sonderzeichen
        fileName = fileName.Replace(" ", "_").Replace("-", "_");

        // Basis-ID: PREFIX_FOLDER_FILENAME
        string baseID = $"{itemTypePrefix}_{folderName}_{fileName}".ToUpper();

        // Prüfe ob diese ID bereits existiert
        string uniqueID = baseID;
        int counter = 1;

        while (IsIDAlreadyUsed(uniqueID, item))
        {
            uniqueID = $"{baseID}_{counter:D3}";
            counter++;

            if (counter > 999)
            {
                Debug.LogError($"[ItemIDAssigner] Konnte keine eindeutige ID für {item.name} finden!");
                return;
            }
        }

        item.ItemID = uniqueID;
        EditorUtility.SetDirty(item);
        Debug.Log($"[ItemIDAssigner] ID vergeben: {item.name} -> {uniqueID}");
    }

    private static string GetItemTypePrefix(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => "WPN",
            ItemType.Kopf => "HEAD",
            ItemType.Brust => "CHEST",
            ItemType.Beine => "LEGS",
            ItemType.Schuhe => "FEET",
            ItemType.Schmuck => "JEWELRY",
            ItemType.Gem => "GEM",
            _ => "ITEM"
        };
    }

    private static bool IsIDAlreadyUsed(string id, Item excludeItem)
    {
        string[] guids = AssetDatabase.FindAssets("t:Item");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(assetPath);

            if (item != null && item != excludeItem && item.ItemID == id)
            {
                return true;
            }
        }

        return false;
    }
}
#endif