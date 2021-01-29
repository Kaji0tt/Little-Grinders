using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{

    //Derzeit verwende ich keine weiteren Konstruktor für den PlayerSave! Im Tutorial hatte er die PlayerStats mit angebgebn, sind hinfällig vorerst.
    public static void SavePlayer()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/player.xxs";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerSave data = new PlayerSave();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static PlayerSave LoadPlayer ()
    {
        string path = Application.persistentDataPath + "/player.xxs";
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
