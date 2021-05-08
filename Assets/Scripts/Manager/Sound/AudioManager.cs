using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    //Definieren, welche Sounds es gibt, für mehr Übersicht im Inspektor.
    public List<Sound> interfaceSounds, musicSounds, entitieSounds, atmosphereSounds, effectSounds;

    //Das Array Sounds, welches alle vorhergehenden Listen ordnet.

    public List<Sound> sounds;

    //Für den Signleton
    public static AudioManager instance;

    //Slider Initialisieren, welche über Unity und Ingame für die Optionen dienen.
    public Slider interfaceSlider, musicSlider, entitieSlider, atmosphereSlider, effectSlider;

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

        //Erstelle einen Array aus Sound-Listen, um diese in der finalen "Sounds" Liste zu speichern.
        List<Sound>[] allSounds = new List<Sound>[] { interfaceSounds, musicSounds, entitieSounds, atmosphereSounds, effectSounds };
        PopulateSounds(allSounds);

        //Falls eine neue Szene geladen wird:
        SceneManager.sceneLoaded += OnSceneLoaded;


        //Setze die neuen Slider
        AwakeSetSliders();




        //Übertrage alle Einstellungen wie Clip, Loop und Volume auf die entsprechend hinterlegten Sounds.
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            
            s.source.loop = s.loop;

            if (s.soundType == SoundType.Music)
            {
                s.source.volume = PlayerPrefs.GetFloat("musicVol");
                musicSounds.Add(s);               
            }            
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
        }
        
    }

    private void AwakeSetSliders()
    {
        /*
        musicSounds = new List<Sound>();
        entitieSounds = new List<Sound>();
        interfaceSounds = new List<Sound>();
        effectSounds = new List<Sound>();
        atmosphereSounds = new List<Sound>();
        */
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("musicVol");

        if (entitieSlider != null)
            entitieSlider.value = PlayerPrefs.GetFloat("entitieVol");

        if (interfaceSlider != null)
            interfaceSlider.value = PlayerPrefs.GetFloat("interfaceVol");

        if (effectSlider != null)
            effectSlider.value = PlayerPrefs.GetFloat("effectVol");

        if (atmosphereSlider != null)
            atmosphereSlider.value = PlayerPrefs.GetFloat("atmosphereVol");
    }

    void PopulateSounds(List<Sound>[] soundList)
    {
        for (int i = 0; i < soundList.Length; i++)
        {
            print(i + " count: " + soundList[i].Count);
            foreach (Sound sound in soundList[i])
            {
                print("Sound: " + sound.name + " added to sounds List");

                sounds.Add(sound);


                if (i == 0)
                    sound.soundType = SoundType.Interface;
                if (i == 1)
                    sound.soundType = SoundType.Music;
                if (i == 2)
                    sound.soundType = SoundType.Entities;
                if (i == 3)
                    sound.soundType = SoundType.Atmosphere;
                if (i == 4)
                    sound.soundType = SoundType.Effect;

            }
        }
    }

    void Start()
    {
        Play("MainMusic");
    }


    public void Play(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Couldn't find Sound of Name: " + s.name);
            return;
        }

        s.source.Play();


        //Hier könnte die Ref zu einem Slider aus dem Options-Menü bzgl. SoundType sein
        //s.volume = options.interface.value;

    }



    #region Sound Options
    public void SetMusicVolume(float newValue) //Set of Values should be configured in UI_Manager.cs, setting of source.volume should be conf
    {
        musicSlider.value = newValue;

        PlayerPrefs.SetFloat("musicVol", newValue);

        
        if(musicSounds.Count != 0)
        foreach(Sound s in musicSounds)
        {
            s.source.volume = musicSlider.value;
        }
        
    }
    /*
    internal void SetInterfaceVolume(float newValue)
    {
        interfaceSlider.value = newValue;

        PlayerPrefs.SetFloat("interfaceVol", newValue);


        if (interfaceSounds.Count != 0)
            foreach (Sound s in musicSounds)
            {
                s.source.volume = interfaceSlider.value;
            }
    }

    internal void SetEntitiesVolume(float newValue)
    {
        entitieSlider.value = newValue;

        PlayerPrefs.SetFloat("entitieVol", newValue);


        if (entitieSounds.Count != 0)
            foreach (Sound s in entitieSounds)
            {
                s.source.volume = entitieSlider.value;
            }
    }
    internal void SetAtmosphereVolume(float newValue)
    {
        atmosphereSlider.value = newValue;

        PlayerPrefs.SetFloat("atmosphereVol", newValue);


        if (atmosphereSounds.Count != 0)
            foreach (Sound s in atmosphereSounds)
            {
                s.source.volume = atmosphereSlider.value;
            }
    }
    internal void SetEffectVolume(float newValue)
    {
        effectSlider.value = newValue;

        PlayerPrefs.SetFloat("effectVol", newValue);


        if (effectSounds.Count != 0)
            foreach (Sound s in effectSounds)
            {
                s.source.volume = effectSlider.value;
            }
    }
    */
    #endregion
}
