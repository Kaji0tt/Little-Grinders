using UnityEngine;

public class SaveSystemTester : MonoBehaviour
{
    [ContextMenu("Test Cloud Upload")]
    void TestCloudUpload()
    {
        Debug.Log("=== SAVE SYSTEM TEST START ===");
        
        // 1. Prüfe MonoBehaviourHelper
        Debug.Log($"MonoBehaviourHelper.instance: {MonoBehaviourHelper.instance}");
        
        // 2. Setze Testname
        SaveSystem.SetPlayerName("TestPlayer_" + Random.Range(1000, 9999));
        Debug.Log($"Player Name: {SaveSystem.GetPlayerName()}");
        
        // 3. Erstelle Test-Save
        PlayerSave testSave = new PlayerSave();
        testSave.mySavedLevel = 10;
        testSave.mySavedXp = 5000;
        testSave.hp = 100f;
        
        // 4. Speichere (sollte Cloud-Upload triggern)
        SaveSystem.SavePlayer(testSave);
        
        Debug.Log("=== TEST ABGESCHLOSSEN ===");
    }
}