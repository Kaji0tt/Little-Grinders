using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Talent_UI))]
public class EditorTalentInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Talent_UI myTalent = (Talent_UI)target;
        SerializedObject so = new SerializedObject(myTalent);

        // "passive"-Toggle anzeigen
        SerializedProperty passiveProp = so.FindProperty("passive");
        EditorGUILayout.PropertyField(passiveProp);

        so.ApplyModifiedProperties(); // Änderungen speichern

        GameObject talentGameObject = myTalent.gameObject;
        Ability abilityComponent = talentGameObject.GetComponent<Ability>();

        if (passiveProp.boolValue)
        {
            // Warnung ausgeben, wenn eine Ability vorhanden ist
            if (abilityComponent != null)
            {
                EditorGUILayout.HelpBox("Dieses Talent ist passiv! Entferne die Ability-Komponente, da sie nicht benötigt wird.", MessageType.Warning);
            }

            // Alle anderen Properties anzeigen, außer "myAbility"
            SerializedProperty prop = so.GetIterator();
            prop.NextVisible(true); // Erstes sichtbare Property (normalerweise "Script")

            while (prop.NextVisible(false))
            {
                if (prop.name != "myAbility" && prop.name != "passive") // "myAbility" & "passive" überspringen
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
        }
        else
        {
            // Falls keine Ability vorhanden ist, Möglichkeit zum Hinzufügen bieten
            if (abilityComponent == null)
            {
                EditorGUILayout.HelpBox("Dieses Talent ist aktiv! Es sollte eine Ability-Komponente enthalten.", MessageType.Warning);

                if (GUILayout.Button("Ability hinzufügen"))
                {
                    talentGameObject.AddComponent<Ability>(); // Füge eine Standard-Ability hinzu (sollte evtl. eine spezialisierte sein)
                }
            }

            // "myAbility"-Feld anzeigen
            SerializedProperty abilityProp = so.FindProperty("myAbility");
            EditorGUILayout.PropertyField(abilityProp);
        }

        so.ApplyModifiedProperties(); // Änderungen speichern
    }
}
