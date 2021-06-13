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
        if(collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>())
        {
            SaveSystem.SaveScenePlayer();

            //ÜBERGANGSWEISE - beim Wechseln der Szene speichert der Spielstand automatisch.
            SaveSystem.SavePlayer();

            //PlayerPrefs können in Unity direkt für das Projekt eingestellt werden. Diese bestehen aus Dateien, welche in System-Verzeichnissen gespeichert werden und deshalb beständig sind.
            PlayerPrefs.GetString("SceneLoad");

            //Lade die nächste Szene im BuildIndex -> Muss überarbeitet werden, sobald es die "Map" gibt á la: LoadScene(A5 oder G19) - entsprechend der Kachel auf der Karte
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        }
    }
}
