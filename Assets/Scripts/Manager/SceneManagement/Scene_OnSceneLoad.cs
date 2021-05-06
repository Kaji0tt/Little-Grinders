using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Falls der Spieler eine neue Szene lädt, muss dem Spiel vermittelt werden, dass entsprechende Dateien aus dem Systemordner ausgelesen werden müssen.
//Dazu dient diese Klasse, welche mit einem Collider im Eingangsbereich plaziert wird und nur einmalig geladen werden kann.
public class Scene_OnSceneLoad : MonoBehaviour
{
    public bool sceneGotLoaded = false;

    void Awake()
    {
        //Falls in den PlayerPrefs der String Load aktiviert wurde, deaktiviere den entsprechenden Bool.
        if(PlayerPrefs.HasKey("Load"))
        {
            sceneGotLoaded = true;
        }
    }



    private void OnTriggerEnter(Collider collider)
    {
        if (collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && sceneGotLoaded == false)
        {

            PlayerLoad playerload = FindObjectOfType<PlayerLoad>();

            PlayerSave data = SaveSystem.LoadScenePlayer();

            playerload.LoadPlayer(data);

            PlayerPrefs.DeleteKey("SceneLoad");

            sceneGotLoaded = true;


        }
    }
}
