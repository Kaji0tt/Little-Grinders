using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    //Definieren, welche Sounds es gibt, für mehr Übersicht im Inspektor.
    public List<Sound> interfaceSounds, musicSounds, atmosphereSounds, effectSounds;

    //Das Array Sounds, welches alle vorhergehenden Listen ordnet.

    public List<Sound> sounds;

    //Für den Signleton
    public static AudioManager instance;

    //Slider Initialisieren, welche über Unity und Ingame für die Optionen dienen.
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

        //Erstelle einen Array aus Sound-Listen, um diese in der finalen "Sounds" Liste zu speichern.
        List<Sound>[] allSounds = new List<Sound>[] { interfaceSounds, musicSounds, atmosphereSounds, effectSounds };
        PopulateSounds(allSounds);

        //Falls eine neue Szene geladen wird:
        SceneManager.sceneLoaded += OnSceneLoaded;


        //Setze die neuen Slider
        AwakeSetSliders();



        //Übertrage alle Einstellungen wie Clip, Loop und Volume auf die entsprechend hinterlegten Sounds.
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            
            s.source.loop = s.loop;

            if (s.soundType == SoundType.Music)
            {
                s.source.volume = PlayerPrefs.GetFloat("musicVol");
                musicSounds.Add(s);               
            }

            if (s.soundType == SoundType.Atmosphere)
            {
                s.source.volume = PlayerPrefs.GetFloat("atmosphereVol");
                atmosphereSounds.Add(s);
            }

            if (s.soundType == SoundType.Interface)
            {
                s.source.volume = PlayerPrefs.GetFloat("interfaceVol");
                interfaceSounds.Add(s);
            }

            if (s.soundType == SoundType.Effect)
            {
                s.source.volume = PlayerPrefs.GetFloat("effectVol");
                effectSounds.Add(s);
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
        
        musicSounds = new List<Sound>();
        interfaceSounds = new List<Sound>();
        effectSounds = new List<Sound>();
        atmosphereSounds = new List<Sound>();
        
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("musicVol");

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
            //print(i + " count: " + soundList[i].Count);
            foreach (Sound sound in soundList[i])
            {
                //print("Sound: " + sound.name + " added to sounds List");

                sounds.Add(sound);


                if (i == 0)
                    sound.soundType = SoundType.Interface;
                if (i == 1)
                    sound.soundType = SoundType.Music;
                if (i == 2)
                    sound.soundType = SoundType.Atmosphere;
                if (i == 3)
                    sound.soundType = SoundType.Effect;

            }
        }
    }

    void Start()
    {
        Play("MainMusic");

        // ⬇️ Event abonnieren
        GameEvents.Instance.OnEnemyWasAttacked += PlayEnemyHitSound;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (!atmoPlaying)
            {
                Play("ForestNight");
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
                Play("MainMusic");
                musicPlaying = true;
            }
            else
            {
                timeStamp = 0;
                musicPlaying = false;
            }

        }
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
    
    internal void SetInterfaceVolume(float newValue)
    {
        interfaceSlider.value = newValue;

        PlayerPrefs.SetFloat("interfaceVol", newValue);


        if (interfaceSounds.Count != 0)
            foreach (Sound s in interfaceSounds)
            {
                s.source.volume = interfaceSlider.value;
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


    private void PlayEnemyHitSound(float damage, Transform transform, bool crit)
    {
        string[] hitSounds = { "Mob_ZombieHit1", "Mob_ZombieHit2", "Mob_ZombieHit3" };

        string chosenSound = hitSounds[UnityEngine.Random.Range(0, hitSounds.Length)];

        Play(chosenSound);
    }

}
