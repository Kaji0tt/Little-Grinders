using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Scene_LoadNextScene : MonoBehaviour
{

    //Falls der Spieler das entsprechende Spielobjekt kollidiert, soll die neue Szene geladen werden und der Spieler gespeichert.
    private void OnTriggerEnter(Collider collider)
    {
        //Um auf den Spieler zuzugreifen, muss auf diesen mit PlayerManager.instance.play referiert werden.
        if (collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>())
        {
            SaveSystem.SaveScenePlayer();

            //ÜBERGANGSWEISE - beim Wechseln der Szene speichert der Spielstand automatisch.
            SaveSystem.SavePlayer();

            //Setze einen String, falls es sich wirklich um einen Szene übergang handelt und nicht um Wechsel der Karte / aus dem Menü.
            //PlayerPrefs.GetString("SceneLoad");



            //Lade die nächste Szene, solange wir nicht in der Prozeduralen Szene für Maps sind.
            if (SceneManager.GetActiveScene().buildIndex != 2)
            {

                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                PlayerPrefs.SetInt("MapX", 0);
                PlayerPrefs.SetInt("MapY", 0);

            }


            else
            {
                LoadNextMap(gameObject.name);
            }
        }
    }

    private void LoadNextMap(string exitDirection)
    {
        switch (exitDirection)
        {
            case "ExitRight":
                PlayerPrefs.SetInt("MapX", PlayerPrefs.GetInt("MapX") + 1);
                PlayerPrefs.SetInt("MapY", PlayerPrefs.GetInt("MapY") + 0);

                MapGenHandler.instance.ResetThisMap();
                MapGenHandler.instance.ScanForExploredMaps("SpawnLeft");
                GlobalMap.SetCurrentMap();

                break;

            case "ExitLeft":
                PlayerPrefs.SetInt("MapX", PlayerPrefs.GetInt("MapX") - 1);
                PlayerPrefs.SetInt("MapY", PlayerPrefs.GetInt("MapY") + 0);

                MapGenHandler.instance.ResetThisMap();
                MapGenHandler.instance.ScanForExploredMaps("SpawnRight");
                GlobalMap.SetCurrentMap();

                break;

            case "ExitTop":
                PlayerPrefs.SetInt("MapX", PlayerPrefs.GetInt("MapX") + 0);
                PlayerPrefs.SetInt("MapY", PlayerPrefs.GetInt("MapY") + 1);

                MapGenHandler.instance.ResetThisMap();
                MapGenHandler.instance.ScanForExploredMaps("SpawnBot");
                GlobalMap.SetCurrentMap();

                break;

            case "ExitBot":
                PlayerPrefs.SetInt("MapX", PlayerPrefs.GetInt("MapX") + 0);
                PlayerPrefs.SetInt("MapY", PlayerPrefs.GetInt("MapY") - 1);

                MapGenHandler.instance.ResetThisMap();
                MapGenHandler.instance.ScanForExploredMaps("SpawnTop");
                GlobalMap.SetCurrentMap();

                break;

            default: break;
        }
    }
}