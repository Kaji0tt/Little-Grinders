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
            Debug.Log("You got here!");
            //Wenn wir im Tutorial sind (buildIndex 1), erschaffe eine Map und füge sie der GlobalMap Instanz hinzu.
            if (SceneManager.GetActiveScene().buildIndex != 2)
            {
                // Speichere den aktuellen Fortschritt
                PlayerSave currentSave = new PlayerSave();
                SaveSystem.SavePlayer(currentSave);

                // WICHTIG: SETZE das Load-Flag - wir wollen den Tutorial-Fortschritt laden!
                PlayerPrefs.SetInt("Load", 1);

                //Lade die nächste Szene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            //Falls der Spieler den Collider zum MapWechsel berührt, während er bereits in der Prozeduralen Szene ist, ScanExitDirection
            else
            {
                // HIER: Verwende den aktuellen Save, erstelle KEINEN neuen!
                PlayerSave currentSave = new PlayerSave(); // Das erstellt einen Save aus dem aktuellen Spielzustand
                SaveSystem.SavePlayer(currentSave);

                LoadNextMap(ScanExitDirection(gameObject.name));
            }
        }
    }

    private SpawnPoint ScanExitDirection(string exitDirection)
    {
        switch (exitDirection)
        {
            case "ExitRight":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x + 1, GlobalMap.instance.currentPosition.y);
                return SpawnPoint.SpawnLeft;

            case "ExitLeft":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x - 1, GlobalMap.instance.currentPosition.y);
                return SpawnPoint.SpawnRight;

            case "ExitTop":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x, GlobalMap.instance.currentPosition.y + 1);
                return SpawnPoint.SpawnBot;

            case "ExitBot":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x, GlobalMap.instance.currentPosition.y - 1);
                return SpawnPoint.SpawnTop;

            default: 
                return SpawnPoint.SpawnRight;
        }
    }

    private void LoadNextMap(SpawnPoint nextSpawnpoint)
    {
        //Setting the globalMap to safe nextSpawnpoint as lasSpawnpoint for Save & Load purposes.
        GlobalMap.instance.lastSpawnpoint = nextSpawnpoint;

        //Reset the Map
        MapGenHandler.instance.ResetThisMap();

        //Play Next Map Sound at Random
        if(AudioManager.instance != null)
        {
            string[] nextMapSound = { "NextMap1", "NextMap2" };
            AudioManager.instance.PlaySound(nextMapSound[UnityEngine.Random.Range(0, 2)]);
        }

        //Tell the MaGenHandler to either create a NewMap, if its not explored yet or Load the explored one.
        MapGenHandler.instance.LoadMap(GlobalMap.instance.ScanIfNextMapIsExplored(), nextSpawnpoint);
    }
}