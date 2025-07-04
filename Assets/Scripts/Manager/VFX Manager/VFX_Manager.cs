using UnityEngine;
using System.Collections.Generic;

public class VFX_Manager : MonoBehaviour
{
    public static VFX_Manager instance;

    // The dictionary remains for fast lookups.
    private Dictionary<string, GameObject> vfxDictionary;

    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the dictionary
        vfxDictionary = new Dictionary<string, GameObject>();

        // Load all GameObject prefabs from the "Assets/Resources/VFX" folder
        GameObject[] allVfxPrefabs = Resources.LoadAll<GameObject>("VFX");

        // Populate the dictionary from the loaded prefabs
        foreach (var prefab in allVfxPrefabs)
        {
            if (!vfxDictionary.ContainsKey(prefab.name))
            {
                vfxDictionary.Add(prefab.name, prefab);
            }
        }
    }

    void OnEnable()
    {
        PlayerStats.eventLevelUp += PlayLevelUpEffect;
    }

    void OnDisable()
    {
        PlayerStats.eventLevelUp -= PlayLevelUpEffect;
    }

    void PlayLevelUpEffect()
    {
        if (PlayerManager.instance.player == null)
        {
            return;
        }
        

        if (PlayerManager.instance.playerStats != null)
        {
            // Rufen Sie die neue Methode mit der Entität auf
            PlayEffect("VFX_LevelUp", PlayerManager.instance.playerStats);
        }
        else
        {
            Debug.LogWarning("VFX_Manager: Das Spieler-GameObject hat keine Komponente, die IEntitie implementiert.");
        }
    }

    /// <summary>
    /// Public method to play any effect by name, parented to an entity.
    /// </summary>
    /// <param name="effectName">The name of the effect defined in the vfxList.</param>
    /// <param name="entitie">The entity to which the effect will be parented.</param>
    public void PlayEffect(string effectName, IEntitie entitie)
    {
        if (entitie == null || entitie.GetTransform() == null)
        {
            Debug.LogWarning($"VFX_Manager: Provided entity for effect '{effectName}' is null or has no transform.");
            return;
        }

        if (vfxDictionary.TryGetValue(effectName, out GameObject effectPrefab))
        {
            Transform parentTransform = entitie.GetTransform();

            // Instanziiert den Effekt an der Position der Entität und setzt diese als Parent.
            GameObject effectInstance = Instantiate(effectPrefab, parentTransform.position, Quaternion.identity, parentTransform);

            // --- DEIN FIX: Rotation des Parents ignorieren ---
            // Setze die Welt-Rotation der Instanz auf die ursprüngliche Welt-Rotation des Prefabs.
            // Dies überschreibt die geerbte Rotation vom Parent.
            effectInstance.transform.rotation = effectPrefab.transform.rotation;

            // --- NEUE LOGIK ZUR KORREKTEN SKALIERUNG ---

            // 1. Speichere die ursprüngliche Skalierung des Prefabs.
            Vector3 originalPrefabScale = effectPrefab.transform.localScale;

            // 2. Holen Sie sich die globale Skalierung des Parents.
            Vector3 parentLossyScale = parentTransform.lossyScale;

            // 3. Berechnen Sie die neue localScale, um die Skalierung des Parents auszugleichen.
            Vector3 newLocalScale = new Vector3(
                originalPrefabScale.x / parentLossyScale.x,
                originalPrefabScale.y / parentLossyScale.y,
                originalPrefabScale.z / parentLossyScale.z
            );

            // 4. Wenden Sie die korrigierte localScale an.
            effectInstance.transform.localScale = newLocalScale;


            if (effectInstance == null)
            {
                Debug.LogWarning($"VFX_Manager: Failed to instantiate prefab for '{effectName}'. The prefab might be invalid or corrupted.");
                return;
            }

            // Zerstört das Effekt-GameObject nach 10 Sekunden.
            // Dies ist eine einfache und robuste Methode, die sicherstellt, dass keine Effekte
            // in der Szene zurückbleiben. Sie setzt voraus, dass die Effekte von selbst starten.
            Destroy(effectInstance, 10f);
        }
        else
        {
            // Add a warning if the effect name is not found in the dictionary
            Debug.LogWarning($"VFX_Manager: Effect with name '{effectName}' not found. Check if the prefab exists in the 'Resources/VFX' folder and that the name is spelled correctly.");
        }
    }
}