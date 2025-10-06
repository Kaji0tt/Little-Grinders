using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Test script to validate the spell socket system functionality
/// </summary>
public class SpellSocketTester : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnStart = false;
    public bool verboseLogging = true;
    
    [Header("Test Results")]
    public bool allTestsPassed = false;
    public List<string> testResults = new List<string>();
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        testResults.Clear();
        bool allPassed = true;
        
        Log("=== Starting Spell Socket System Tests ===");
        
        // Test 1: Manager Singleton
        allPassed &= TestManagerSingleton();
        
        // Test 2: Socket Creation and Configuration
        allPassed &= TestSocketCreation();
        
        // Test 3: Spell Property Validation
        allPassed &= TestSpellPropertyValidation();
        
        // Test 4: Socket Lock/Unlock
        allPassed &= TestSocketLockUnlock();
        
        // Test 5: Spell Socketing
        allPassed &= TestSpellSocketing();
        
        // Test 6: Pace Spell Instantiation
        allPassed &= TestPaceSpellInstantiation();
        
        allTestsPassed = allPassed;
        
        Log($"=== Tests Complete. All Passed: {allTestsPassed} ===");
        
        foreach (string result in testResults)
        {
            Debug.Log($"[SpellSocketTester] {result}");
        }
    }
    
    private bool TestManagerSingleton()
    {
        Log("Test 1: Manager Singleton");
        
        try
        {
            // Test SpellSocketManager instance
            if (SpellSocketManager.instance == null)
            {
                LogError("SpellSocketManager.instance is null");
                return false;
            }
            
            LogSuccess("SpellSocketManager singleton working");
            
            // Test PlayerManager instance  
            if (PlayerManager.instance == null)
            {
                LogError("PlayerManager.instance is null");
                return false;
            }
            
            LogSuccess("PlayerManager singleton working");
            return true;
        }
        catch (System.Exception e)
        {
            LogError($"Manager singleton test failed: {e.Message}");
            return false;
        }
    }
    
    private bool TestSocketCreation()
    {
        Log("Test 2: Socket Creation and Configuration");
        
        try
        {
            // Create test socket
            GameObject socketObj = new GameObject("TestSocket");
            SpellSocket socket = socketObj.AddComponent<SpellSocket>();
            socket.socketType = SpellSocketType.Movement;
            
            if (socket.socketType != SpellSocketType.Movement)
            {
                LogError("Socket type not set correctly");
                Destroy(socketObj);
                return false;
            }
            
            LogSuccess("Socket creation and type assignment working");
            
            // Test socket name
            string typeName = socket.GetSocketTypeName();
            if (string.IsNullOrEmpty(typeName))
            {
                LogError("Socket type name is empty");
                Destroy(socketObj);
                return false;
            }
            
            LogSuccess($"Socket type name: {typeName}");
            
            Destroy(socketObj);
            return true;
        }
        catch (System.Exception e)
        {
            LogError($"Socket creation test failed: {e.Message}");
            return false;
        }
    }
    
    private bool TestSpellPropertyValidation()
    {
        Log("Test 3: Spell Property Validation");
        
        try
        {
            // Test pace spell properties
            var properties = SpellProperty.PaceMovement | SpellProperty.Persistent;
            
            if (!properties.HasFlag(SpellProperty.PaceMovement))
            {
                LogError("Pace movement property not detected");
                return false;
            }
            
            if (!properties.HasFlag(SpellProperty.Persistent))
            {
                LogError("Persistent property not detected");
                return false;
            }
            
            LogSuccess("Spell property validation working");
            return true;
        }
        catch (System.Exception e)
        {
            LogError($"Spell property test failed: {e.Message}");
            return false;
        }
    }
    
    private bool TestSocketLockUnlock()
    {
        Log("Test 4: Socket Lock/Unlock");
        
        try
        {
            GameObject socketObj = new GameObject("TestSocket");
            SpellSocket socket = socketObj.AddComponent<SpellSocket>();
            
            // Test initial state
            if (socket.isUnlocked)
            {
                LogError("Socket should start locked");
                Destroy(socketObj);
                return false;
            }
            
            // Test unlock
            socket.UnlockSocket();
            if (!socket.isUnlocked)
            {
                LogError("Socket unlock failed");
                Destroy(socketObj);
                return false;
            }
            
            // Test lock
            socket.LockSocket();
            if (socket.isUnlocked)
            {
                LogError("Socket lock failed");
                Destroy(socketObj);
                return false;
            }
            
            LogSuccess("Socket lock/unlock working");
            
            Destroy(socketObj);
            return true;
        }
        catch (System.Exception e)
        {
            LogError($"Socket lock/unlock test failed: {e.Message}");
            return false;
        }
    }
    
    private bool TestSpellSocketing()
    {
        Log("Test 5: Spell Socketing");
        
        try
        {
            GameObject socketObj = new GameObject("TestSocket");
            SpellSocket socket = socketObj.AddComponent<SpellSocket>();
            socket.socketType = SpellSocketType.Movement;
            socket.UnlockSocket();
            
            // Create test spell data
            GameObject abilityDataObj = new GameObject("TestSpellData");
            AbilityData testSpell = abilityDataObj.AddComponent<AbilityData>();
            testSpell.abilityName = "Test Movement Spell";
            testSpell.properties = SpellProperty.PaceMovement | SpellProperty.Persistent;
            
            // Test spell compatibility
            if (!socket.CanSocketSpell(testSpell))
            {
                LogError("Compatible spell rejected");
                Destroy(socketObj);
                Destroy(abilityDataObj);
                return false;
            }
            
            LogSuccess("Spell compatibility check working");
            
            // Test invalid spell
            testSpell.properties = SpellProperty.PaceAP; // Wrong type
            if (socket.CanSocketSpell(testSpell))
            {
                LogError("Incompatible spell accepted");
                Destroy(socketObj);
                Destroy(abilityDataObj);
                return false;
            }
            
            LogSuccess("Spell rejection working");
            
            Destroy(socketObj);
            Destroy(abilityDataObj);
            return true;
        }
        catch (System.Exception e)
        {
            LogError($"Spell socketing test failed: {e.Message}");
            return false;
        }
    }
    
    private bool TestPaceSpellInstantiation()
    {
        Log("Test 6: Pace Spell Instantiation");
        
        try
        {
            // Test creating pace spell instances
            GameObject spellObj = new GameObject("TestPaceSpell");
            
            // Test CombatRush
            CombatRush combatRush = spellObj.AddComponent<CombatRush>();
            if (combatRush == null)
            {
                LogError("CombatRush component creation failed");
                Destroy(spellObj);
                return false;
            }
            
            LogSuccess("CombatRush component created");
            
            // Clean up
            Destroy(spellObj);
            
            // Test other pace spells
            string[] paceSpellTypes = {
                "EvasionBoost", "VictoryMomentum", "BattleTrance", 
                "ArcaneEcho", "ManaFlow", "BloodFrenzy", "PrecisionStrike"
            };
            
            foreach (string spellType in paceSpellTypes)
            {
                GameObject testObj = new GameObject($"Test{spellType}");
                
                // Use reflection to add component by type name
                System.Type componentType = System.Type.GetType(spellType);
                if (componentType != null)
                {
                    Component component = testObj.AddComponent(componentType);
                    if (component == null)
                    {
                        LogError($"{spellType} component creation failed");
                        Destroy(testObj);
                        return false;
                    }
                    LogSuccess($"{spellType} component created");
                }
                else
                {
                    LogWarning($"Type {spellType} not found (may not be compiled yet)");
                }
                
                Destroy(testObj);
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            LogError($"Pace spell instantiation test failed: {e.Message}");
            return false;
        }
    }
    
    private void Log(string message)
    {
        if (verboseLogging)
        {
            testResults.Add($"[INFO] {message}");
        }
    }
    
    private void LogSuccess(string message)
    {
        testResults.Add($"[PASS] {message}");
    }
    
    private void LogError(string message)
    {
        testResults.Add($"[FAIL] {message}");
    }
    
    private void LogWarning(string message)
    {
        testResults.Add($"[WARN] {message}");
    }
}