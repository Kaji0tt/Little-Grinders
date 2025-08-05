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
        
        // Enhanced feedback: Screen shake for level up
        if (ScreenShakeManager.Instance != null)
        {
            ScreenShakeManager.Instance.TriggerShake(ScreenShakeManager.ShakeType.Extreme);
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

        PlayEffect(effectName, entitie.GetTransform());
    }

    /// <summary>
    /// Public method to play any effect by name, parented to a transform.
    /// </summary>
    /// <param name="effectName">The name of the effect defined in the vfxList.</param>
    /// <param name="parentTransform">The transform to which the effect will be parented.</param>
    public void PlayEffect(string effectName, Transform parentTransform)
    {
        if (parentTransform == null)
        {
            Debug.LogWarning($"VFX_Manager: Provided transform for effect '{effectName}' is null.");
            return;
        }

        if (vfxDictionary.TryGetValue(effectName, out GameObject effectPrefab))
        {
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

    /// <summary>
    /// Public method to play any effect by name at a specific position and rotation without parenting.
    /// </summary>
    /// <param name="effectName">The name of the effect defined in the vfxList.</param>
    /// <param name="position">The world position where the effect should be spawned.</param>
    /// <param name="rotation">The rotation for the effect.</param>
    public void PlayEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        if (vfxDictionary.TryGetValue(effectName, out GameObject effectPrefab))
        {
            // Instanziiert den Effekt an der gegebenen Position und Rotation ohne Parent.
            GameObject effectInstance = Instantiate(effectPrefab, position, rotation);

            if (effectInstance == null)
            {
                Debug.LogWarning($"VFX_Manager: Failed to instantiate prefab for '{effectName}'. The prefab might be invalid or corrupted.");
                return;
            }

            // Zerstört das Effekt-GameObject nach 10 Sekunden.
            Destroy(effectInstance, 10f);
        }
        else
        {
            // Add a warning if the effect name is not found in the dictionary
            Debug.LogWarning($"VFX_Manager: Effect with name '{effectName}' not found. Check if the prefab exists in the 'Resources/VFX' folder and that the name is spelled correctly.");
        }
    }
}