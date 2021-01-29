using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadMenu : MonoBehaviour
{
    private void Awake()
    {
        //player
    }
    public void SaveGame()
    {
        SaveSystem.SavePlayer();

        // Kein Lust mehr, hier weiter schauen: 
        // https://www.youtube.com/watch?v=XOjd_qU2Ido&t=915s
    }

    public void LoadGame()
    {
        PlayerSave data = SaveSystem.LoadPlayer();

        SceneManager.LoadScene(data.scene);

        SaveSystem.LoadPlayer();
    }
}
