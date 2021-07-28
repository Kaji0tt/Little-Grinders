using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Falls der Spieler eine neue Szene lädt, muss dem Spiel vermittelt werden, dass entsprechende Dateien aus dem Systemordner ausgelesen werden müssen.
//Dazu dient diese Klasse, welche mit einem Collider im Eingangsbereich plaziert wird und nur einmalig geladen werden kann.
public class Scene_OnSceneLoad
{
    /*
    public bool sceneGotLoaded = false;

    public void LoadScenePlayer(PlayerLoad playerload)
    {
        

        PlayerSave data = SaveSystem.LoadPlayer();

        playerload.LoadPlayer(data);

        

    }
    */
    


 
    
    /*

    public static void LoadPlayer(PlayerLoad playerload)
    {
        if (sceneGotLoaded == false)
        {
            Debug.Log("We are reloading the player data.");
            //PlayerLoad playerload = FindObjectOfType<PlayerLoad>();

            PlayerSave data = SaveSystem.LoadPlayer();

            playerload.LoadPlayer(data);

            PlayerPrefs.DeleteKey("SceneLoad");

            sceneGotLoaded = true;


        }
    }
    */
}
