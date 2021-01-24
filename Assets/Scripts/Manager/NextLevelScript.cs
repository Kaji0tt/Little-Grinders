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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
