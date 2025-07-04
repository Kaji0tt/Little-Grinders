using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


// NEU: Ein Enum, um die Sound-Kategorien klar zu definieren.
public enum SoundType { Music, Effect, Interface, Atmosphere }

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [HideInInspector]
    public AudioSource source;

    // NEU: Jedes Sound-Objekt weiß jetzt, zu welcher Kategorie es gehört.
    [HideInInspector]
    public SoundType soundType;
}
