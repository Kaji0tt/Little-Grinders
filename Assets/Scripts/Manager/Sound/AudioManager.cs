using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    private Dictionary<string, List<Sound>> soundGroups = new Dictionary<string, List<Sound>>();
    public static AudioManager instance;

    public Slider interfaceSlider, musicSlider, atmosphereSlider, effectSlider;

    private bool atmoPlaying = false;

    float timeStamp;

    bool musicPlaying = false;

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
    /// Ermittelt den Gruppenschlüssel aus einem Clip-Namen (z.B. "Wurf" aus "Wurf_1").
    /// </summary>
    private string GetSoundGroupKey(string clipName)
    {
        int lastUnderscore = clipName.LastIndexOf('_');
        if (lastUnderscore != -1 && char.IsDigit(clipName.Last()))
        {
            // Wenn ein Unterstrich da ist und das letzte Zeichen eine Ziffer ist,
            // nimm den Teil vor dem Unterstrich.
            return clipName.Substring(0, lastUnderscore);
        }
        // Ansonsten ist der ganze Name der Schlüssel (z.B. für "MainMusic").
        return clipName;
    }

    /// <summary>
    /// Spielt einen zufälligen Sound aus der angegebenen Gruppe ab.
    /// </summary>
    public void PlaySound(string groupKey)
    {
        if (soundGroups.TryGetValue(groupKey, out List<Sound> group))
        {
            if (group.Count > 0)
            {
                // Wähle einen zufälligen Sound aus der Gruppe aus.
                Sound soundToPlay = group[UnityEngine.Random.Range(0, group.Count)];
                
                // Die Lautstärke wird jetzt durch die Slider-Methoden gesetzt.
                soundToPlay.source.Play();
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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("SoundControl"))
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

    #region Sound Options - ANGEPASST
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


    private void PlayEnemyHitSound(float damage, Transform transform, bool crit)
    {
        // Der alte Code:
        // string[] hitSounds = { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };
        // string chosenSound = hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)];
        // PlaySound(chosenSound);

        // Der neue, einfache Aufruf:
        PlaySound("Mob_ZombieHit");
    }
}
