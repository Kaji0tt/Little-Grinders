using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Scene_LoadNextScene : MonoBehaviour
{


    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject == PlayerColliderManager.instance.player_collider)
        {
            SaveSystem.SaveScenePlayer();

            PlayerPrefs.GetString("SceneLoad");

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        }
    }
}
