﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public CanvasGroup mainMenu, saveMenu, optionsMenu, roadMenu, whiteBackground;
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            Application.Quit();

        else
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1f;
        }

    }

    public void ResumeGame()
    {

        UI_Manager.instance.Resume();
    }

    public void ToggleMainMenu()
    {
        mainMenu.alpha = mainMenu.alpha > 0 ? 0 : 1;
        if(whiteBackground != null)whiteBackground.alpha = whiteBackground.alpha > 0 ? 0 : 1;
        mainMenu.blocksRaycasts = mainMenu.blocksRaycasts == true ? false : true;
    }

    public void ToggleSaveMenu()
    {
        saveMenu.alpha = saveMenu.alpha > 0 ? 0 : 1;
        saveMenu.blocksRaycasts = saveMenu.blocksRaycasts == true ? false : true;
    }

    public void ToggleOptionsMenu()
    {
        optionsMenu.alpha = optionsMenu.alpha > 0 ? 0 : 1;
        optionsMenu.blocksRaycasts = optionsMenu.blocksRaycasts == true ? false : true;
    }

    public void ToggleRoadMapMenu()
    {
        roadMenu.alpha = roadMenu.alpha > 0 ? 0 : 1;
        roadMenu.blocksRaycasts = roadMenu.blocksRaycasts == true ? false : true;
    }


    
    // Sound Option Interface Settings
    public void SetMusicVolume(float newValue) //Set of Values should be configured in UI_Manager.cs, setting of source.volume should be conf
    {

        AudioManager.instance.SetMusicVolume(newValue);
    }
    
    public void SetInterfaceVolume(float newValue) 
    {

        AudioManager.instance.SetInterfaceVolume(newValue);
    }


    public void SetAtmosphereVolume(float newValue) 
    {

        AudioManager.instance.SetAtmosphereVolume(newValue);
    }
    public void SetEffectVolume(float newValue)
    {

        AudioManager.instance.SetEffectVolume(newValue);
    }
    

}
