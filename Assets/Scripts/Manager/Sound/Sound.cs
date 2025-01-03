﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SoundType { Interface, Music, Atmosphere, Effect}
[System.Serializable]
public class Sound 
{
    public string name;

    public AudioClip clip;

    [HideInInspector]
    public SoundType soundType;

    [Range(0f, 1f)]
    public float volume;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
