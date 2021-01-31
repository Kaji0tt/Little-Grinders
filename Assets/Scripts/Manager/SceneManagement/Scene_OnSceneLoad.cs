using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_OnSceneLoad : MonoBehaviour
{
    public bool sceneGotLoaded = false;

    void Awake()
    {
        if(PlayerPrefs.HasKey("Load"))
        {
            sceneGotLoaded = true;
        }
    }



    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == PlayerColliderManager.instance.player_collider && sceneGotLoaded == false)
        {

            PlayerLoad playerload = FindObjectOfType<PlayerLoad>();

            PlayerSave data = SaveSystem.LoadScenePlayer();

            playerload.LoadPlayer(data);

            sceneGotLoaded = true;


        }
    }
}
