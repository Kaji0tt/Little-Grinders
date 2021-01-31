using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            Application.Quit();

        else
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1f;
        }

    }

    public void ResumeGame()
    {

        UI_Manager.instance.Resume();
    }


}
