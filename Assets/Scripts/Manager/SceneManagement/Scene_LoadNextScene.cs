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

            }


            else
            {
                ScanExitDirection(gameObject.name);
            }
        }
    }

    private void ScanExitDirection(string exitDirection)
    {
        switch (exitDirection)
        {
            case "ExitRight":

                GlobalMap.currentPosition = new Vector2(GlobalMap.currentPosition.x + 1, GlobalMap.currentPosition.y);

                LoadNextMap("SpawnLeft");

                break;

            case "ExitLeft":

                GlobalMap.currentPosition = new Vector2(GlobalMap.currentPosition.x - 1, GlobalMap.currentPosition.y);

                LoadNextMap("SpawnRight");

                break;

            case "ExitTop":

                GlobalMap.currentPosition = new Vector2(GlobalMap.currentPosition.x, GlobalMap.currentPosition.y + 1);

                LoadNextMap("SpawnBot");

                break;

            case "ExitBot":

                GlobalMap.currentPosition = new Vector2(GlobalMap.currentPosition.x, GlobalMap.currentPosition.y - 1);

                LoadNextMap("SpawnTop");

                break;

            default: break;
        }
    }

    private void LoadNextMap(string nextSpawnpoint)
    {

        MapGenHandler.instance.ResetThisMap();

        GlobalMap.lastSpawnpoint = nextSpawnpoint;

        GlobalMap.GetNextMap();

    }
}