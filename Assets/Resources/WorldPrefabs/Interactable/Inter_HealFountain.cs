using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inter_HealFountain : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private string healVFXName = "VFX_HealFountain";
    
    [Header("Bonus Settings")]
    [SerializeField] private float xpBonusPercentage = 0.25f; // 25% XP Bonus
    
    [Header("Usage Settings")]
    [SerializeField] private bool isUsed = false; // Verhindert mehrfache Nutzung

    private void OnTriggerStay(Collider other)
    {
        // Prüfe ob es der Player ist und Fountain noch nicht benutzt wurde
        if (other == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && !isUsed)
        {
            // Hole die Pick-Taste aus dem KeyManager
            KeyCode pickKey = KeyManager.MyInstance.Keybinds["PICK"];
            
            if (Input.GetKeyDown(pickKey))
            {
                UseFountain();
            }
        }
    }

    private void UseFountain()
    {
        if (isUsed) return;
        
        isUsed = true;
        
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        // Vollständige Heilung
        playerStats.Heal((int)playerStats.Get_maxHp());

        // 25% XP Bonus basierend auf benötigter XP für nächstes Level
        GiveXPBonus(playerStats);

        // Licht ausschalten
        if (gameObject.GetComponentInChildren<Light>())
        {
            gameObject.GetComponentInChildren<Light>().intensity = 0;
        }

        // VFX abspielen
        PlayHealVFX();

        // Log-Nachricht
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Fountain used! Full heal + XP bonus!", 3f);
        }
    }

    private void GiveXPBonus(PlayerStats playerStats)
    {
        // Berechne benötigte XP für nächstes Level
        int currentLevel = playerStats.Get_level();
        int currentXP = playerStats.Get_xp();
        
        // Verwende die bereits vorhandene LevelUp_need() Methode
        int xpRequired = playerStats.LevelUp_need();

        // 25% der benötigten XP als Bonus
        int xpBonus = Mathf.RoundToInt(xpRequired * xpBonusPercentage);

        // XP hinzufügen
        playerStats.Gain_xp(xpBonus);

        // Log für XP Bonus
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog($"+{xpBonus} XP Bonus!", 2f);
        }
    }

    private void PlayHealVFX()
    {
        if (VFX_Manager.instance != null)
        {
            // VFX an der Fountain-Position abspielen
            VFX_Manager.instance.PlayEffect(healVFXName, this.transform.position, Quaternion.identity);
        }
        else
        {
            // Fallback falls VFX_Manager nicht verfügbar
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Healing Fountain activated!", 2f);
            }
        }
    }

    // Optional: Gizmo für Trigger-Bereich im Editor
    private void OnDrawGizmosSelected()
    {
        // Zeige den Trigger-Bereich an (falls BoxCollider vorhanden)
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}
