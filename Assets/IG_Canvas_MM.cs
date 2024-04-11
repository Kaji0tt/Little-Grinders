using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IG_Canvas_MM : MonoBehaviour
{

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            gameObject.SetActive(false);
        }

    }

}
