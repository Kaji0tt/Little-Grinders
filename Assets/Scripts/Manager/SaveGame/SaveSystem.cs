using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    private static string playerPath => Application.persistentDataPath + "/player.xxs";

    // Prüft, ob ein Savegame existiert
    public static bool HasSave()
    {
        return File.Exists(playerPath);
    }

    // Lädt das Savegame inkl. Seed
    public static PlayerSave LoadPlayer()
    {
        if (HasSave())
        {
            try
            {
                FileInfo fileInfo = new FileInfo(playerPath);
                if (fileInfo.Length == 0)
                {
                    Debug.LogWarning("Save file is empty. Creating new save.");
                    return NewSave();
                }

                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(playerPath, FileMode.Open);

                PlayerSave data = formatter.Deserialize(stream) as PlayerSave;
                stream.Close();

                Debug.Log("Player Data loaded.");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load save file: " + ex.Message + "\nCreating new save.");
                return NewSave();
            }
        }
        else
        {
            Debug.LogWarning("Save file not found. Creating new save.");
            return NewSave();
        }
    }

    // Erstellt ein neues Savegame mit zufälligem Seed (mind. 10 Zeichen)
    public static PlayerSave NewSave()
    {
        PlayerSave save = new PlayerSave();
        save.talentTreeSeed = UnityEngine.Random.Range(1000000000, int.MaxValue);
        SavePlayer(save);
        return save;
    }

    // Speichert das Savegame inkl. Seed
    public static void SavePlayer(PlayerSave data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(playerPath, FileMode.Create);

        formatter.Serialize(stream, data);
        stream.Close();

        Debug.Log("SAVE TROUBLESHOOT -- player.xxs saved");
    }

    // Szenen-Save bleibt wie gehabt
    public static void SaveScenePlayer()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.scene";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerSave data = new PlayerSave();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerSave LoadScenePlayer()
    {
        string path = Application.persistentDataPath + "/player.scene";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerSave data = formatter.Deserialize(stream) as PlayerSave;

            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
