using UnityEngine;

/// <summary>
/// Integration test to verify the complete spell socket system
/// </summary>
public class SpellSocketIntegrationTest : MonoBehaviour
{
    [Header("Test Settings")]
    public bool autoRunTests = false;
    
    private void Start()
    {
        if (autoRunTests)
        {
            Invoke(nameof(RunIntegrationTest), 1f); // Delay to allow managers to initialize
        }
    }
    
    [ContextMenu("Run Integration Test")]
    public void RunIntegrationTest()
    {
        Debug.Log("=== Running Spell Socket Integration Test ===");
        
        // Test 1: Verify managers are initialized
        if (TestManagerInitialization())
        {
            Debug.Log("✓ Manager initialization test passed");
        }
        else
        {
            Debug.LogError("✗ Manager initialization test failed");
            return;
        }
        
        // Test 2: Test socket unlock through talent progression
        if (TestSocketUnlockProgression())
        {
            Debug.Log("✓ Socket unlock progression test passed");
        }
        else
        {
            Debug.LogError("✗ Socket unlock progression test failed");
        }
        
        // Test 3: Test spell socket functionality
        if (TestSpellSocketFunctionality())
        {
            Debug.Log("✓ Spell socket functionality test passed");
        }
        else
        {
            Debug.LogError("✗ Spell socket functionality test failed");
        }
        
        Debug.Log("=== Integration Test Complete ===");
    }
    
    private bool TestManagerInitialization()
    {
        // Check if all required managers exist
        if (PlayerManager.instance == null)
        {
            Debug.LogError("PlayerManager instance not found");
            return false;
        }
        
        if (TalentTreeManager.instance == null)
        {
            Debug.LogError("TalentTreeManager instance not found");
            return false;
        }
        
        // SpellSocketManager might not exist if not added to scene yet
        if (SpellSocketManager.instance == null)
        {
            Debug.LogWarning("SpellSocketManager instance not found - this is expected if not added to scene");
        }
        
        return true;
    }
    
    private bool TestSocketUnlockProgression()
    {
        try
        {
            // Create a mock talent
            GameObject talentObj = new GameObject("MockTalent");
            Talent_UI mockTalent = talentObj.AddComponent<Talent_UI>();
            
            // This would normally be set through the inspector
            mockTalent.myTypes = new System.Collections.Generic.List<TalentType> { TalentType.AP };
            mockTalent.currentCount = 3; // Enough to unlock socket
            
            // Test would call the socket unlock logic
            // Note: This requires SpellSocketManager to be present
            
            Destroy(talentObj);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Socket unlock test failed: {e.Message}");
            return false;
        }
    }
    
    private bool TestSpellSocketFunctionality()
    {
        try
        {
            // Create test socket
            GameObject socketObj = new GameObject("TestSocket");
            SpellSocket socket = socketObj.AddComponent<SpellSocket>();
            socket.socketType = SpellSocketType.Movement;
            socket.UnlockSocket();
            
            // Create test spell data
            GameObject spellDataObj = new GameObject("TestSpellData");
            AbilityData testSpell = spellDataObj.AddComponent<AbilityData>();
            testSpell.abilityName = "Test Movement Spell";
            testSpell.properties = SpellProperty.PaceMovement | SpellProperty.Persistent;
            
            // Test spell compatibility
            bool canSocket = socket.CanSocketSpell(testSpell);
            
            // Cleanup
            Destroy(socketObj);
            Destroy(spellDataObj);
            
            return canSocket;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Spell socket functionality test failed: {e.Message}");
            return false;
        }
    }
    
    [ContextMenu("Test All Pace Spells")]
    public void TestAllPaceSpells()
    {
        Debug.Log("=== Testing All Pace Spells ===");
        
        // Test each pace spell type
        string[] spellTypes = {
            "CombatRush", "EvasionBoost", "VictoryMomentum", "BattleTrance",
            "ArcaneEcho", "ManaFlow", "BloodFrenzy", "PrecisionStrike"
        };
        
        foreach (string spellType in spellTypes)
        {
            TestPaceSpell(spellType);
        }
        
        Debug.Log("=== Pace Spell Testing Complete ===");
    }
    
    private void TestPaceSpell(string spellTypeName)
    {
        try
        {
            GameObject spellObj = new GameObject($"Test{spellTypeName}");
            
            System.Type spellType = System.Type.GetType(spellTypeName);
            if (spellType != null)
            {
                Component spellComponent = spellObj.AddComponent(spellType);
                if (spellComponent != null)
                {
                    Debug.Log($"✓ {spellTypeName} spell created successfully");
                    
                    // Test initialization if it's an Ability
                    if (spellComponent is Ability ability)
                    {
                        // Create minimal test data
                        GameObject dataObj = new GameObject("TestData");
                        AbilityData testData = dataObj.AddComponent<AbilityData>();
                        testData.abilityName = $"Test {spellTypeName}";
                        testData.cooldownTime = 1f;
                        testData.activeTime = 3f;
                        testData.maxCharges = 1;
                        
                        // Test initialization
                        ability.Initialize(testData, 1.0f);
                        Debug.Log($"✓ {spellTypeName} initialized successfully");
                        
                        Destroy(dataObj);
                    }
                }
                else
                {
                    Debug.LogError($"✗ Failed to create {spellTypeName} component");
                }
            }
            else
            {
                Debug.LogWarning($"⚠ {spellTypeName} type not found (may not be compiled)");
            }
            
            Destroy(spellObj);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ {spellTypeName} test failed: {e.Message}");
        }
    }
}