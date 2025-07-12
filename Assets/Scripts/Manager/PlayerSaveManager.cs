using UnityEngine;

public class PlayerSaveManager
{
    private const string SaveKey = "PlayerSave";

    // Prüft, ob ein Savegame existiert
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    // Lädt das Savegame aus PlayerPrefs
    public static PlayerSave Load()
    {
        if (!HasSave()) return null;
        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<PlayerSave>(json);
    }

    // Erstellt ein neues Savegame und speichert es
    public static PlayerSave NewSave()
    {
        PlayerSave save = new PlayerSave();
        Save(save);
        return save;
    }

    // Speichert das Savegame in PlayerPrefs
    public static void Save(PlayerSave save)
    {
        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }
}
