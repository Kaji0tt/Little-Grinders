using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicVol : MonoBehaviour
{
    /*
    public Slider musicVolSlider;
    AudioSource musicVol;

    bool foundNewSlider;

    private void Awake()
    {
        musicVol = GetComponent<AudioSource>();

        GameObject[] musicObj = GameObject.FindGameObjectsWithTag("GameMusic");

        if(musicObj.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }


    void Update()
    {

        if(SceneManager.GetActiveScene().buildIndex >= 1 && foundNewSlider == false)
        {
            CheckForSlider();
        }

        musicVol.volume = musicVolSlider.value;
        PlayerPrefs.SetFloat("music", musicVol.volume);
    }

    void CheckForSlider()
    {
        GameObject sliderObj = GameObject.FindWithTag("SoundControl");
        musicVolSlider = sliderObj.GetComponent<Slider>();
        foundNewSlider = true;
    }
    */
}
