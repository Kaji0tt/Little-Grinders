using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds; //Alle weiteren Arrays dienen für Runtime-Handling!

    private List<Sound> interfaceSounds, musicSounds, entitieSounds, atmosphereSounds, effectSounds;

    public static AudioManager instance;

    public Slider interfaceSlider, musicSlider, entitieSlider, atmosphereSlider, effectSlider;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        AwakeSetSliders();

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
        musicSounds = new List<Sound>();
        entitieSounds = new List<Sound>();
        interfaceSounds = new List<Sound>();
        effectSounds = new List<Sound>();
        atmosphereSounds = new List<Sound>();

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



    void Start()
    {
        Play("MainMusic");
    }


    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
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

    #endregion
}
