using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NextLevelScript : MonoBehaviour
{


    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject == PlayerColliderManager.instance.player_collider)
        {
            SaveSystem.SaveScenePlayer();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

            //Nach dem Game mit India hier den Player-Save callen. In der neuen Szene den entsprechenden PlayerSave laden - ez gg.
        }
    }
}
