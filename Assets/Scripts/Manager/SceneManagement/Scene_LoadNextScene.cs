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
                Debug.Log("Amd tje game should save.!");
                //MapSave newMap = new MapSave();

                //GlobalMap.instance.AddNewMap(newMap);

                //Setze lastSpawnpoint auf Left
                //GlobalMap.instance.lastSpawnpoint = "SpawnRight";

                //Speichere anschließend den Spieler
                SaveSystem.SavePlayer();

                //Lade die nächste Szene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

            }


            //Falls der Spieler den Collider zum MapWechsel berührt, während er bereits in der Prozeduralen Szene ist, ScanExitDirection
            else
            {
                SaveSystem.SavePlayer();

                LoadNextMap(ScanExitDirection(gameObject.name));

            }
        }
    }

    private string ScanExitDirection(string exitDirection)
    {
        switch (exitDirection)
        {
            case "ExitRight":

                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x + 1, GlobalMap.instance.currentPosition.y);

                return("SpawnLeft");


            case "ExitLeft":

                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x - 1, GlobalMap.instance.currentPosition.y);

                return("SpawnRight");

            case "ExitTop":

                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x, GlobalMap.instance.currentPosition.y + 1);

                return("SpawnBot");

            case "ExitBot":

                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x, GlobalMap.instance.currentPosition.y - 1);

                return("SpawnTop");

             

            default: return("SpawnRight");
        }
    }

    private void LoadNextMap(string nextSpawnpoint)
    {
        //Setting the globalMap to safe nextSpawnpoint as lasSpawnpoint for Save & Load purposes.
        GlobalMap.instance.lastSpawnpoint = nextSpawnpoint;

        //Reset the Map
        MapGenHandler.instance.ResetThisMap();

        //Play Next Map Sound at Random
        if(AudioManager.instance != null)
        {
            string[] nextMapSound = { "NextMap1", "NextMap2" };
            AudioManager.instance.Play(nextMapSound[UnityEngine.Random.Range(0, 2)]);

        }

        //Tell the MaGenHandler to either create a NewMap, if its not explored yet or Load the explored one.
        MapGenHandler.instance.LoadMap(GlobalMap.instance.ScanIfNextMapIsExplored(), nextSpawnpoint);


    }
}