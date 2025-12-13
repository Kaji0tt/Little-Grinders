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

        SaveSystem.SavePlayer(SaveSystem.NewSave());
        LogScript.instance.ShowLog("The Game has been saved!");

    }

    public void StartNewGame()
    {
        // Lösche alten Spielstand
        SaveSystem.DeleteSave();
        
        // Stelle sicher, dass kein "Load" Key gesetzt ist
        PlayerPrefs.DeleteKey("Load");
        
        // Lade direkt zur Intro-Szene (buildIndex 1)
        SceneManager.LoadScene(1);
    }

    public void LoadGame()
    {
        // Prüfe, ob überhaupt ein Spielstand existiert
        if (!SaveSystem.HasSave())
        {
            Debug.LogError("No save data found!");
            LogScript.instance.ShowLog("No save game found!");
            return;
        }

        Time.timeScale = 1f;

        PlayerSave data = SaveSystem.LoadPlayer();

        if (data == null)
        {
            Debug.LogError("Save data is corrupted!");
            return;
        }

        // Setze Load-Flag
        PlayerPrefs.SetInt("Load", 1);

        // Direkt zur ProceduralMap Scene laden (Index 2)
        int sceneToLoad = 2;
        
        // Optional: Falls currentScene gespeichert und > 1 ist, das verwenden
        if (data.currentScene > 1)
        {
            sceneToLoad = data.currentScene;
        }

        // Verstecke Death-Tab
        if(UI_Manager.instance != null)
        {
            UI_Manager.instance.ToggleDeathTab(false);
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
