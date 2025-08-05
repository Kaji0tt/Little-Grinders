using UnityEngine;

/// <summary>
/// Test script for validating the enhanced animation and feedback systems.
/// This script can be attached to a test GameObject to verify all systems work correctly.
/// </summary>
public class FeedbackSystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode testScreenShakeKey = KeyCode.F1;
    [SerializeField] private KeyCode testHitStopKey = KeyCode.F2;
    [SerializeField] private KeyCode testCriticalHitKey = KeyCode.F3;
    [SerializeField] private KeyCode testLevelUpKey = KeyCode.F4;
    [SerializeField] private KeyCode testStatusEffectKey = KeyCode.F5;
    [SerializeField] private KeyCode testDamageFlashKey = KeyCode.F6;
    [SerializeField] private KeyCode testHealFlashKey = KeyCode.F7;

    [Header("Test Settings")]
    [SerializeField] private Sprite testIcon;

    private void Update()
    {
        HandleTestInputs();
    }

    private void HandleTestInputs()
    {
        // Test screen shake
        if (Input.GetKeyDown(testScreenShakeKey))
        {
            TestScreenShake();
        }

        // Test hit stop
        if (Input.GetKeyDown(testHitStopKey))
        {
            TestHitStop();
        }

        // Test critical hit feedback
        if (Input.GetKeyDown(testCriticalHitKey))
        {
            TestCriticalHitFeedback();
        }

        // Test level up feedback
        if (Input.GetKeyDown(testLevelUpKey))
        {
            TestLevelUpFeedback();
        }

        // Test status effect
        if (Input.GetKeyDown(testStatusEffectKey))
        {
            TestStatusEffect();
        }

        // Test damage flash
        if (Input.GetKeyDown(testDamageFlashKey))
        {
            TestDamageFlash();
        }

        // Test heal flash
        if (Input.GetKeyDown(testHealFlashKey))
        {
            TestHealFlash();
        }
    }

    private void TestScreenShake()
    {
        Debug.Log("Testing Screen Shake (F1)");
        
        if (ScreenShakeManager.Instance != null)
        {
            ScreenShakeManager.Instance.TriggerShake(ScreenShakeManager.ShakeType.Medium);
        }
        else
        {
            Debug.LogWarning("ScreenShakeManager not found!");
        }
    }

    private void TestHitStop()
    {
        Debug.Log("Testing Hit Stop (F2)");
        
        if (HitStopManager.Instance != null)
        {
            HitStopManager.Instance.TriggerHitStop(HitStopManager.HitStopType.Medium);
        }
        else
        {
            Debug.LogWarning("HitStopManager not found!");
        }
    }

    private void TestCriticalHitFeedback()
    {
        Debug.Log("Testing Critical Hit Feedback (F3)");
        
        if (FeedbackSystemManager.Instance != null)
        {
            FeedbackSystemManager.Instance.TriggerCriticalHitFeedback();
        }
        else
        {
            Debug.LogWarning("FeedbackSystemManager not found!");
        }
    }

    private void TestLevelUpFeedback()
    {
        Debug.Log("Testing Level Up Feedback (F4)");
        
        if (FeedbackSystemManager.Instance != null)
        {
            FeedbackSystemManager.Instance.TriggerLevelUpFeedback();
        }
        else
        {
            Debug.LogWarning("FeedbackSystemManager not found!");
        }
    }

    private void TestStatusEffect()
    {
        Debug.Log("Testing Status Effect (F5)");
        
        if (StatusEffectVisualManager.Instance != null)
        {
            StatusEffectVisualManager.Instance.ShowBuffEffect("TestBuff", testIcon, 5f);
        }
        else
        {
            Debug.LogWarning("StatusEffectVisualManager not found!");
        }
    }

    private void TestDamageFlash()
    {
        Debug.Log("Testing Damage Flash (F6)");
        
        if (FeedbackSystemManager.Instance != null)
        {
            FeedbackSystemManager.Instance.TriggerDamageTakenFeedback();
        }
        else
        {
            Debug.LogWarning("FeedbackSystemManager not found!");
        }
    }

    private void TestHealFlash()
    {
        Debug.Log("Testing Heal Flash (F7)");
        
        if (FeedbackSystemManager.Instance != null)
        {
            FeedbackSystemManager.Instance.TriggerHealingFeedback();
        }
        else
        {
            Debug.LogWarning("FeedbackSystemManager not found!");
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Feedback System Tester", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
        GUILayout.Space(10);
        
        GUILayout.Label("F1 - Test Screen Shake");
        GUILayout.Label("F2 - Test Hit Stop");
        GUILayout.Label("F3 - Test Critical Hit");
        GUILayout.Label("F4 - Test Level Up");
        GUILayout.Label("F5 - Test Status Effect");
        GUILayout.Label("F6 - Test Damage Flash");
        GUILayout.Label("F7 - Test Heal Flash");
        
        GUILayout.Space(10);
        
        // System status
        GUILayout.Label("System Status:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        GUILayout.Label($"ScreenShake: {(ScreenShakeManager.Instance != null ? "✓" : "✗")}");
        GUILayout.Label($"HitStop: {(HitStopManager.Instance != null ? "✓" : "✗")}");
        GUILayout.Label($"StatusEffects: {(StatusEffectVisualManager.Instance != null ? "✓" : "✗")}");
        GUILayout.Label($"ScreenOverlay: {(ScreenOverlayManager.Instance != null ? "✓" : "✗")}");
        GUILayout.Label($"FeedbackManager: {(FeedbackSystemManager.Instance != null ? "✓" : "✗")}");
        
        GUILayout.EndArea();
    }
}