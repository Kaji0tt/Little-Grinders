using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadMenu : MonoBehaviour
{
    //public LoadFromMenu loadFromMenu;
    private void Awake()
    {


    }
    public void SaveGame()
    {
        SaveSystem.SavePlayer();

    }

    public void LoadGame()
    {
        //Should not be here.
        Time.timeScale = 1f;

        PlayerSave data = SaveSystem.LoadPlayer();

        //Der Int wird im späteren Verlauf den Status des gewählen SavedGames reflektieren, damit man mehrere Saves haben kann.
        PlayerPrefs.SetInt("Load", 1);

        SceneManager.LoadScene(data.MyScene);

    }
}
