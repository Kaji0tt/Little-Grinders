using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    private Dictionary<string, List<Sound>> soundGroups = new Dictionary<string, List<Sound>>();
    
    // NEU: Aktive Sound-Verwaltung
    private Dictionary<string, List<ActiveSound>> activeSounds = new Dictionary<string, List<ActiveSound>>();
    private List<ActiveSound> allActiveSounds = new List<ActiveSound>();
    
    public static AudioManager instance;

    public Slider interfaceSlider, musicSlider, atmosphereSlider, effectSlider;

    private bool atmoPlaying = false;
    float timeStamp;
    bool musicPlaying = false;

    // NEU: Struktur für aktive Sounds
    [System.Serializable]
    public class ActiveSound
    {
        public string groupKey;
        public AudioSource source;
        public GameObject tempGameObject; // Für temporäre AudioSources
        public bool isLooping;
        public bool isTemporary; // Wird automatisch zerstört wenn fertig
        
        public ActiveSound(string key, AudioSource audioSource, bool loop = false, bool temporary = true)
        {
            groupKey = key;
            source = audioSource;
            isLooping = loop;
            isTemporary = temporary;
        }
    }

    void Awake()
    {
        //Singleton Anweisung, zur globalen Reference AudioManager.instance
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        //Der AudioManager muss Szeneübergreifen bestehen bleiben.
        DontDestroyOnLoad(gameObject);

        // --- NEUE AUTOMATISCHE LADE-LOGIK ---
        LoadAndGroupSounds();

        //Falls eine neue Szene geladen wird:
        SceneManager.sceneLoaded += OnSceneLoaded;

        //Setze die neuen Slider
        AwakeSetSliders();
        
        // NEU: Starte Sound-Cleanup Coroutine
        StartCoroutine(CleanupFinishedSounds());
    }

    /// <summary>
    /// Lädt alle Sounds aus den kategoriespezifischen Unterordnern und gruppiert sie.
    /// </summary>
    private void LoadAndGroupSounds()
    {
        // Lade Sounds für jede Kategorie aus dem entsprechenden Unterordner.
        LoadSoundsFromFolder("Sounds/Music", SoundType.Music);
        LoadSoundsFromFolder("Sounds/Effects", SoundType.Effect);
        LoadSoundsFromFolder("Sounds/Interface", SoundType.Interface);
        LoadSoundsFromFolder("Sounds/Atmosphere", SoundType.Atmosphere);

        Debug.Log($"AudioManager: {soundGroups.Values.Sum(list => list.Count)} Sounds geladen und in {soundGroups.Count} Gruppen sortiert.");
    }

    /// <summary>
    /// Eine Hilfsmethode, die alle Clips aus einem bestimmten Ordner lädt und verarbeitet.
    /// </summary>
    private void LoadSoundsFromFolder(string folderPath, SoundType type)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(folderPath);
        foreach (var clip in clips)
        {
            string groupKey = GetSoundGroupKey(clip.name);

            Sound newSound = new Sound
            {
                name = clip.name,
                clip = clip,
                soundType = type, // Weise den korrekten Typ zu!
                source = gameObject.AddComponent<AudioSource>()
            };
            newSound.source.clip = newSound.clip;
            newSound.source.playOnAwake = false;

            if (!soundGroups.ContainsKey(groupKey))
            {
                soundGroups[groupKey] = new List<Sound>();
            }
            soundGroups[groupKey].Add(newSound);
        }
    }

    /// <summary>
    /// Ermittelt den Gruppenschlüssel aus einem Clip-Namen (z.B. "Wurf" aus "Wurf-1").
    /// </summary>
    private string GetSoundGroupKey(string clipName)
    {
        int lastUnderscore = clipName.LastIndexOf('-');
        if (lastUnderscore != -1 && char.IsDigit(clipName.Last()))
        {
            // Wenn ein Unterstrich da ist und das letzte Zeichen eine Ziffer ist,
            // nimm den Teil vor dem Unterstrich.
            return clipName.Substring(0, lastUnderscore);
        }
        // Ansonsten ist der ganze Name der Schlüssel (z.B. für "MainMusic").
        return clipName;
    }


    #region Sound Verwaltung
    /// <summary>
    /// Spielt einen zufälligen Sound aus der angegebenen Gruppe ab.
    /// </summary>
    public void PlaySound(string groupKey)
    {
        PlaySound(groupKey, false, false);
    }

    /// <summary>
    /// Spielt einen Sound mit erweiterten Optionen ab.
    /// </summary>
    /// <param name="groupKey">Schlüssel der Sound-Gruppe</param>
    /// <param name="loop">Soll der Sound geloopt werden?</param>
    /// <param name="trackActive">Soll der Sound in der aktiven Liste verfolgt werden?</param>
    /// <returns>Referenz auf den aktiven Sound (falls trackActive = true)</returns>
    public ActiveSound PlaySound(string groupKey, bool loop = false, bool trackActive = false)
    {
        if (soundGroups.TryGetValue(groupKey, out List<Sound> group))
        {
            if (group.Count > 0)
            {
                // Wähle einen zufälligen Sound aus der Gruppe aus.
                Sound soundTemplate = group[UnityEngine.Random.Range(0, group.Count)];
                
                if (trackActive || loop)
                {
                    // Erstelle temporäre AudioSource für verwaltete Sounds
                    GameObject tempGO = new GameObject($"TempAudio_{groupKey}");
                    AudioSource tempSource = tempGO.AddComponent<AudioSource>();
                    
                    // Kopiere Einstellungen vom Template
                    tempSource.clip = soundTemplate.clip;
                    tempSource.volume = soundTemplate.source.volume;
                    tempSource.pitch = soundTemplate.source.pitch;
                    tempSource.loop = loop;
                    tempSource.playOnAwake = false;
                    
                    // Spiele ab
                    tempSource.Play();
                    
                    // Erstelle ActiveSound und verwalte ihn
                    ActiveSound activeSound = new ActiveSound(groupKey, tempSource, loop, true);
                    activeSound.tempGameObject = tempGO;
                    
                    AddActiveSound(activeSound);
                    
                    return activeSound;
                }
                else
                {
                    // Standard-Verhalten: Spiele direkt ab
                    soundTemplate.source.Play();
                }
            }
            else
            {
                Debug.LogWarning($"Sound-Gruppe '{groupKey}' ist leer.");
            }
        }
        else
        {
            Debug.LogWarning($"Sound-Gruppe '{groupKey}' nicht gefunden.");
        }
        
        return null;
    }

    /// <summary>
    /// Stoppt alle Sounds einer bestimmten Gruppe.
    /// </summary>
    public void StopSound(string groupKey)
    {
        if (activeSounds.ContainsKey(groupKey))
        {
            List<ActiveSound> soundsToStop = new List<ActiveSound>(activeSounds[groupKey]);
            
            foreach (ActiveSound activeSound in soundsToStop)
            {
                StopActiveSound(activeSound);
            }
        }
    }

    /// <summary>
    /// Stoppt einen spezifischen aktiven Sound.
    /// </summary>
    public void StopActiveSound(ActiveSound activeSound)
    {
        if (activeSound != null && activeSound.source != null)
        {
            activeSound.source.Stop();
            
            RemoveActiveSound(activeSound);
            
            // Zerstöre temporäre GameObject falls vorhanden
            if (activeSound.tempGameObject != null)
            {
                Destroy(activeSound.tempGameObject);
            }
        }
    }

    /// <summary>
    /// Stoppt alle aktuell abspielenden Sounds.
    /// </summary>
    public void StopAllSounds()
    {
        List<ActiveSound> soundsToStop = new List<ActiveSound>(allActiveSounds);
        
        foreach (ActiveSound activeSound in soundsToStop)
        {
            StopActiveSound(activeSound);
        }
    }

    /// <summary>
    /// Stoppt alle Sounds eines bestimmten Typs.
    /// </summary>
    public void StopSoundsByType(SoundType soundType)
    {
        List<ActiveSound> soundsToStop = new List<ActiveSound>();
        
        foreach (ActiveSound activeSound in allActiveSounds)
        {
            if (soundGroups.ContainsKey(activeSound.groupKey))
            {
                var group = soundGroups[activeSound.groupKey];
                if (group.Count > 0 && group[0].soundType == soundType)
                {
                    soundsToStop.Add(activeSound);
                }
            }
        }
        
        foreach (ActiveSound activeSound in soundsToStop)
        {
            StopActiveSound(activeSound);
        }
    }

    /// <summary>
    /// Fügt einen aktiven Sound zur Verwaltung hinzu.
    /// </summary>
    private void AddActiveSound(ActiveSound activeSound)
    {
        // Zur globalen Liste hinzufügen
        allActiveSounds.Add(activeSound);
        
        // Zur gruppenspezifischen Liste hinzufügen
        if (!activeSounds.ContainsKey(activeSound.groupKey))
        {
            activeSounds[activeSound.groupKey] = new List<ActiveSound>();
        }
        activeSounds[activeSound.groupKey].Add(activeSound);
    }

    /// <summary>
    /// Entfernt einen aktiven Sound aus der Verwaltung.
    /// </summary>
    private void RemoveActiveSound(ActiveSound activeSound)
    {
        allActiveSounds.Remove(activeSound);
        
        if (activeSounds.ContainsKey(activeSound.groupKey))
        {
            activeSounds[activeSound.groupKey].Remove(activeSound);
            
            // Entferne leere Gruppen
            if (activeSounds[activeSound.groupKey].Count == 0)
            {
                activeSounds.Remove(activeSound.groupKey);
            }
        }
    }

    /// <summary>
    /// Coroutine die automatisch beendete Sounds aufräumt.
    /// </summary>
    private IEnumerator CleanupFinishedSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Prüfe jede Sekunde
            
            List<ActiveSound> soundsToRemove = new List<ActiveSound>();
            
            foreach (ActiveSound activeSound in allActiveSounds)
            {
                // Prüfe ob AudioSource noch existiert und spielt
                if (activeSound.source == null || 
                    (!activeSound.source.isPlaying && !activeSound.isLooping && activeSound.isTemporary))
                {
                    soundsToRemove.Add(activeSound);
                }
            }
            
            // Entferne beendete Sounds
            foreach (ActiveSound activeSound in soundsToRemove)
            {
                if (activeSound.tempGameObject != null)
                {
                    Destroy(activeSound.tempGameObject);
                }
                RemoveActiveSound(activeSound);
            }
        }
    }

    /// <summary>
    /// Prüft ob ein Sound einer bestimmten Gruppe gerade abgespielt wird.
    /// </summary>
    public bool IsSoundPlaying(string groupKey)
    {
        return activeSounds.ContainsKey(groupKey) && activeSounds[groupKey].Count > 0;
    }

    /// <summary>
    /// Gibt alle aktiven Sounds einer Gruppe zurück.
    /// </summary>
    public List<ActiveSound> GetActiveSounds(string groupKey)
    {
        if (activeSounds.ContainsKey(groupKey))
        {
            return new List<ActiveSound>(activeSounds[groupKey]);
        }
        return new List<ActiveSound>();
    }

    #endregion


    #region Sound Options
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // NEU: Stoppe Hauptmenü-Musik, wenn wir die Hauptmenü-Szene verlassen
        if (scene.buildIndex != 0)
        {
            StopSound("Abyssal_Echoes - Main Theme");
        }
        
        // NEU: Spiele Hauptmenü-Musik, wenn wir zur Hauptmenü-Szene zurückkehren
        if (scene.buildIndex == 0)
        {
            PlaySound("Abyssal_Echoes - Main Theme", loop: true, trackActive: true);
        }

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("SoundControl"))
        {
            if (go.name == "MusicSlider")
            {
                musicSlider = go.GetComponent<Slider>();

                musicSlider.value = PlayerPrefs.GetFloat("musicVol");
            }

            if (go.name == "InterfaceSlider")
            {
                interfaceSlider = go.GetComponent<Slider>();

                interfaceSlider.value = PlayerPrefs.GetFloat("interfaceVol");
            }

            if (go.name == "AtmosphereSlider")
            {
                atmosphereSlider = go.GetComponent<Slider>();

                atmosphereSlider.value = PlayerPrefs.GetFloat("atmosphereVol");
            }

            if (go.name == "EffectSlider")
            {
                effectSlider = go.GetComponent<Slider>();

                effectSlider.value = PlayerPrefs.GetFloat("effectVol");
            }
        }

    }

    private void AwakeSetSliders()
    {
        
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("musicVol");

        if (interfaceSlider != null)
            interfaceSlider.value = PlayerPrefs.GetFloat("interfaceVol");

        if (effectSlider != null)
            effectSlider.value = PlayerPrefs.GetFloat("effectVol");

        if (atmosphereSlider != null)
            atmosphereSlider.value = PlayerPrefs.GetFloat("atmosphereVol");
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (!atmoPlaying)
            {
                PlaySound("ForestNight");
                atmoPlaying = true;
            }
        }
        else
            atmoPlaying = false;


        if (SceneManager.GetActiveScene().buildIndex == 2)
            MusicManagement();
    }

    private void MusicManagement()
    {
        float intervall = 65f;

        timeStamp += Time.deltaTime;

        if(timeStamp >= intervall && !musicPlaying)
        {
            int rnd = UnityEngine.Random.Range(1, 5);
            if(rnd == 1)
            {
                //Hier sollte nachher auf ein Array verwiesen werden, mit unterschiedlichen Tracks von Mirco
                PlaySound("MainMusic");
                musicPlaying = true;
            }
            else
            {
                timeStamp = 0;
                musicPlaying = false;
            }

        }
    }


    public void SetMusicVolume(float newValue)
    {
        SetVolumeForType(SoundType.Music, newValue);
        if (musicSlider != null) musicSlider.value = newValue;
        PlayerPrefs.SetFloat("musicVol", newValue);
    }
    
    public void SetInterfaceVolume(float newValue)
    {
        SetVolumeForType(SoundType.Interface, newValue);
        if (interfaceSlider != null) interfaceSlider.value = newValue;
        PlayerPrefs.SetFloat("interfaceVol", newValue);
    }

    public void SetAtmosphereVolume(float newValue)
    {
        SetVolumeForType(SoundType.Atmosphere, newValue);
        if (atmosphereSlider != null) atmosphereSlider.value = newValue;
        PlayerPrefs.SetFloat("atmosphereVol", newValue);
    }

    public void SetEffectVolume(float newValue)
    {
        SetVolumeForType(SoundType.Effect, newValue);
        if (effectSlider != null) effectSlider.value = newValue;
        PlayerPrefs.SetFloat("effectVol", newValue);
    }

    /// <summary>
    /// Eine zentrale Methode, um die Lautstärke für eine ganze Kategorie zu setzen.
    /// </summary>
    private void SetVolumeForType(SoundType type, float volume)
    {
        // Gehe durch alle Sound-Gruppen...
        foreach (var group in soundGroups.Values)
        {
            // ...und durch alle Sounds in jeder Gruppe.
            foreach (var sound in group)
            {
                // Wenn der Sound zur richtigen Kategorie gehört, setze die Lautstärke.
                if (sound.soundType == type)
                {
                    sound.source.volume = volume;
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// Spielt einen Sound über die AudioSource einer Entität ab.
    /// </summary>
    public void PlayEntitySound(string groupKey, GameObject entity)
    {
        if (entity == null)
        {
            return;
        }
        
        AudioClip clip = GetClip(groupKey);
        
        if (clip == null)
        {
            return;
        }
        
        var audioSource = entity.GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = entity.AddComponent<AudioSource>();
            
            // Standard-Einstellungen für neue AudioSource
            audioSource.volume = 1.0f;
            audioSource.pitch = 1.0f;
            audioSource.spatialBlend = 0.5f; // 3D Sound
            audioSource.playOnAwake = false;
        }
        
        // Prüfe AudioSource Zustand
        if (!audioSource.enabled)
        {
            return;
        }
        
        // Spiele Sound ab
        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// Gibt einen zufälligen AudioClip aus einer Sound-Gruppe zurück.
    /// </summary>
    public AudioClip GetClip(string groupKey)
    {
        if (soundGroups.TryGetValue(groupKey, out List<Sound> group) && group.Count > 0)
        {
            return group[UnityEngine.Random.Range(0, group.Count)].clip;
        }
        return null;
    }
}
