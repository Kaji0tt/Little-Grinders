using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SoundType { Interface, Entities, Music, Atmosphere, Effect}
[System.Serializable]
public class Sound 
{
    public string name;

    public AudioClip clip;

    public SoundType soundType;

    [Range(0f, 1f)]
    public float volume;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
