using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem 
{
    private static string playerPath => Application.persistentDataPath + "/player.xxs";
    
    // FTP/Server Konfiguration
    private const string SERVER_URL = "https://blauzahn.eu/"; // Deine Domain
    private const string UPLOAD_ENDPOINT = SERVER_URL + "upload_save.php";
    private const string LEADERBOARD_ENDPOINT = SERVER_URL + "get_leaderboard.php";

    // Prüft, ob ein Savegame existiert
    public static bool HasSave()
    {
        bool exists = File.Exists(playerPath);
        Debug.Log($"[SaveSystem.HasSave] Prüfe Datei: {playerPath}, existiert: {exists}");
        return exists;
    }

    // Lädt das Savegame inkl. Seed
    public static PlayerSave LoadPlayer()
    {
        Debug.Log("=== [SaveSystem.LoadPlayer] START ===");
        Debug.Log($"[SaveSystem.LoadPlayer] Dateipfad: {playerPath}");
        
        if (HasSave())
        {
            Debug.Log("[SaveSystem.LoadPlayer] Save-Datei existiert");
            try
            {
                FileInfo fileInfo = new FileInfo(playerPath);
                Debug.Log($"[SaveSystem.LoadPlayer] Dateigröße: {fileInfo.Length} bytes");
                
                if (fileInfo.Length == 0)
                {
                    Debug.LogWarning("[SaveSystem.LoadPlayer] Save-Datei ist leer - erstelle neuen Save");
                    return NewSave();
                }

                Debug.Log("[SaveSystem.LoadPlayer] Starte Deserialisierung...");
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(playerPath, FileMode.Open);

                PlayerSave data = formatter.Deserialize(stream) as PlayerSave;
                stream.Close();

                Debug.Log($"[SaveSystem.LoadPlayer] Deserialisierung erfolgreich, data null: {data == null}");
                if (data != null)
                {
                    Debug.Log($"[SaveSystem.LoadPlayer] Geladene Daten - Level: {data.mySavedLevel}, XP: {data.mySavedXp}, HP: {data.hp}");
                    Debug.Log($"[SaveSystem.LoadPlayer] exploredMaps.Count: {data.exploredMaps?.Count ?? 0}");
                    
                    // Debug: Log erste paar Maps
                    if (data.exploredMaps != null && data.exploredMaps.Count > 0)
                    {
                        Debug.Log($"[SaveSystem.LoadPlayer] First 3 maps:");
                        for (int i = 0; i < Mathf.Min(3, data.exploredMaps.Count); i++)
                        {
                            MapSave map = data.exploredMaps[i];
                            Debug.Log($"  [{i}] ({map.mapIndexX}, {map.mapIndexY}): cleared={map.isCleared}, visited={map.isVisited}, fieldType[0]={map.fieldType?[0] ?? FieldType.Road}");
                        }
                    }
                }
                
                Debug.Log("=== [SaveSystem.LoadPlayer] ENDE - ERFOLGREICH ===");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem.LoadPlayer] ❌ Fehler beim Laden: {ex.Message}\nStackTrace: {ex.StackTrace}");
                Debug.LogError("[SaveSystem.LoadPlayer] Erstelle neuen Save wegen Fehler");
                return NewSave();
            }
        }
        else
        {
            Debug.LogWarning("[SaveSystem.LoadPlayer] Save-Datei existiert nicht - erstelle neuen Save");
            return NewSave();
        }
    }

    // Erstellt ein neues Savegame mit zufälligem Seed (mind. 10 Zeichen)
    public static PlayerSave NewSave()
    {
        Debug.Log("=== [SaveSystem.NewSave] START ===");
        
        PlayerSave save = new PlayerSave();
        save.talentTreeSeed = UnityEngine.Random.Range(1000000000, int.MaxValue);
        Debug.Log($"[SaveSystem.NewSave] Neuer Save erstellt mit TalentTreeSeed: {save.talentTreeSeed}");
        
        Debug.Log("[SaveSystem.NewSave] Rufe SavePlayer() auf...");
        SavePlayer(save);
        
        Debug.Log("=== [SaveSystem.NewSave] ENDE ===");
        return save;
    }

    // Neue Methode: Spielername setzen
    public static void SetPlayerName(string playerName)
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Spielername gesetzt: {playerName}");
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString("PlayerName", "");
    }

    public static bool HasPlayerName()
    {
        return !string.IsNullOrEmpty(GetPlayerName());
    }

    // Erweiterte SavePlayer Methode mit Cloud-Upload
    public static void SavePlayer(PlayerSave data)
    {
        Debug.Log("=== [SaveSystem.SavePlayer] START ===");
        Debug.Log($"[SaveSystem.SavePlayer] Saving data: Level={data.mySavedLevel}, XP={data.mySavedXp}, exploredMaps.Count={data.exploredMaps?.Count ?? 0}");
        
        // Debug: Log erste paar Maps
        if (data.exploredMaps != null && data.exploredMaps.Count > 0)
        {
            Debug.Log($"[SaveSystem.SavePlayer] First 3 maps:");
            for (int i = 0; i < Mathf.Min(3, data.exploredMaps.Count); i++)
            {
                MapSave map = data.exploredMaps[i];
                Debug.Log($"  [{i}] ({map.mapIndexX}, {map.mapIndexY}): cleared={map.isCleared}, visited={map.isVisited}, fieldType[0]={map.fieldType?[0] ?? FieldType.Road}");
            }
        }
        
        // Lokales Speichern (wie bisher)
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(playerPath, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
            Debug.Log("[SaveSystem.SavePlayer] ✓ Lokales Speichern erfolgreich");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveSystem.SavePlayer] ❌ Fehler beim lokalen Speichern: {ex.Message}");
            return;
        }

        // Cloud-Upload (wenn Spielername vorhanden)
        if (HasPlayerName())
        {
            string playerName = GetPlayerName();
            Debug.Log($"[SaveSystem.SavePlayer] 📤 Starte Cloud-Upload für Player: {playerName}");
            
            if (MonoBehaviourHelper.instance == null)
            {
                Debug.LogError("❌ MonoBehaviourHelper.instance ist NULL! Cloud-Upload nicht möglich!");
                return;
            }
            
            Debug.Log("[SaveSystem.SavePlayer] ✓ MonoBehaviourHelper gefunden, starte Coroutine");
            MonoBehaviourHelper.instance.StartCoroutine(UploadSaveToCloud(data));
        }
        else
        {
            Debug.LogWarning("[SaveSystem.SavePlayer] ⚠️ Kein Spielername gesetzt - Cloud-Upload übersprungen");
        }
        
        Debug.Log("=== [SaveSystem.SavePlayer] ENDE ===");
    }

    // Coroutine für Cloud-Upload
    private static IEnumerator UploadSaveToCloud(PlayerSave data)
    {
        string playerName = GetPlayerName();
        Debug.Log($"[UploadSaveToCloud] 🚀 Upload startet für: {playerName}");
        Debug.Log($"[UploadSaveToCloud] 📊 Daten - Level: {data.mySavedLevel}, XP: {data.mySavedXp}, HP: {data.hp}");
        
        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);
        form.AddField("level", data.mySavedLevel);
        form.AddField("xp", data.mySavedXp);
        form.AddField("hp", data.hp.ToString());
        form.AddField("lastPlayed", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        Debug.Log($"[UploadSaveToCloud] 🌐 Sende Request an: {UPLOAD_ENDPOINT}");

        using (UnityWebRequest www = UnityWebRequest.Post(UPLOAD_ENDPOINT, form))
        {
            yield return www.SendWebRequest();

            Debug.Log($"[UploadSaveToCloud] 📡 Request abgeschlossen. Status: {www.result}");
            Debug.Log($"[UploadSaveToCloud] 📄 Response: {www.downloadHandler.text}");

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[UploadSaveToCloud] ✅ Cloud-Upload erfolgreich!");
            }
            else
            {
                Debug.LogError($"[UploadSaveToCloud] ❌ Cloud-Upload fehlgeschlagen!");
                Debug.LogError($"[UploadSaveToCloud] Error: {www.error}");
                Debug.LogError($"[UploadSaveToCloud] Response Code: {www.responseCode}");
            }
        }
    }

    // Leaderboard abrufen
    public static IEnumerator GetLeaderboard(System.Action<List<PlayerLeaderboardEntry>> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(LEADERBOARD_ENDPOINT))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                PlayerLeaderboardData leaderboardData = JsonUtility.FromJson<PlayerLeaderboardData>(jsonResponse);
                callback?.Invoke(leaderboardData.players);
            }
            else
            {
                Debug.LogError($"[SaveSystem] ❌ Leaderboard-Abruf fehlgeschlagen: {www.error}");
                callback?.Invoke(new List<PlayerLeaderboardEntry>());
            }
        }
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

    // Neue Methode hinzufügen:
    public static void DeleteSave()
    {
        Debug.Log($"[SaveSystem.DeleteSave] Lösche Save-Datei: {playerPath}");
        
        if (File.Exists(playerPath))
        {
            File.Delete(playerPath);
            Debug.Log("[SaveSystem.DeleteSave] ✓ Save-Datei erfolgreich gelöscht");
        }
        else
        {
            Debug.Log("[SaveSystem.DeleteSave] Save-Datei existiert nicht - nichts zu löschen");
        }
    }
}

// Datenstrukturen für Leaderboard
[System.Serializable]
public class PlayerLeaderboardEntry
{
    public string playerName;
    public int level;
    public int xp;
    public string lastPlayed;
}

[System.Serializable]
public class PlayerLeaderboardData
{
    public List<PlayerLeaderboardEntry> players;
}
