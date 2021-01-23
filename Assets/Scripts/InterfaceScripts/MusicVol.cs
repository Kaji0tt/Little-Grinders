using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MusicVol : MonoBehaviour
{
    public Slider musicVolSlider;
    AudioSource musicVol;

    private void Awake()
    {
        musicVol = GetComponent<AudioSource>();
    }


    void Update()
    {
        musicVol.volume = musicVolSlider.value;
    }
}
