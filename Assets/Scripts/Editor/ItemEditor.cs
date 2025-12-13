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
            
            // Prüfe ob Pfad gültig ist
            if (!string.IsNullOrEmpty(assetPath))
            {
                string directoryPath = System.IO.Path.GetDirectoryName(assetPath);
                
                // Prüfe ob Verzeichnispfad gültig ist
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    string folderName = directoryPath.Split('/').Last().ToLower();

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
                        case "gems":
                            item.itemType = ItemType.Gem;
                            break;
                    }
                }
            }
        }

        // Standard Informationen
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent("Item ID", "Automatisch generiert - nicht manuell ändern!"));
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField(item.ItemID);
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("Regenerate", GUILayout.Width(80)))
        {
            if (EditorUtility.DisplayDialog(
                "Regenerate Item ID",
                $"Möchtest du die ID für '{item.ItemName}' wirklich neu generieren?\n\nAlte ID: {item.ItemID}\n\nDies kann Savegames beschädigen!",
                "Ja, neu generieren",
                "Abbrechen"))
            {
                item.ItemID = ""; // Leere ID, damit sie beim nächsten Save neu generiert wird
                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        item.ItemName = EditorGUILayout.TextField("Item Name", item.ItemName);
        item.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", item.itemType);
        item.icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.icon, typeof(Sprite), false);
        
        // Drop Configuration
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Drop Configuration", EditorStyles.boldLabel);
        item.dropLevel = EditorGUILayout.IntField(
            new GUIContent("Drop Level", "Mindest-Level ab dem dieses Item droppen kann"),
            item.dropLevel);
        item.dropWeight = EditorGUILayout.IntField(
            new GUIContent("Drop Weight", "Relatives Gewicht (höher = häufiger). Beispiel: 100 = Common, 50 = Uncommon, 10 = Rare, 1 = Very Rare"),
            item.dropWeight);

        // Textarea f�r Beschreibung
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        item.ItemDescription = EditorGUILayout.TextArea(item.ItemDescription, GUILayout.MinHeight(60));


        // Falls Waffe
        if (item.itemType == ItemType.Weapon)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weapon Settings", EditorStyles.boldLabel);
            item.weaponCombo = (WeaponCombo)EditorGUILayout.ObjectField("Weapon Combo", item.weaponCombo, typeof(WeaponCombo), false);
        }

        // Falls Gem
        if (item.itemType == ItemType.Gem)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gem Settings", EditorStyles.boldLabel);
            
            // Gem Type mit erweitertem Tooltip
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Gem Type", 
                "Ruby = Hauptangriff\n" +
                "Sapphire = Bewegung/Dash\n" +
                "Emerald = Flächenangriff (AoE)\n" +
                "Amethyst = Finisher/Execute\n" +
                "Topaz = Unterstützung/Buff"));
            item.gemType = (GemType)EditorGUILayout.EnumPopup(item.gemType);
            EditorGUILayout.EndHorizontal();
            
            item.gemAbility = (AbilityData)EditorGUILayout.ObjectField(
                new GUIContent("Gem Ability", "Die Fähigkeit die dieses Gem gewährt (wird normalerweise zur Laufzeit durch ItemMods gesetzt)"), 
                item.gemAbility, 
                typeof(AbilityData), 
                false);
        }

        // "Percent Values" ausklappbar machen
        showPercentValues = EditorGUILayout.Foldout(showPercentValues, "Percent Values (0.01 = 1%)");

        if (showPercentValues)
        {
            EditorGUILayout.HelpBox("Hinweis: Prozentwerte als Dezimalzahl eingeben (z.B. 0.01 für 1%, 0.15 für 15%)", MessageType.Info);
            
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
