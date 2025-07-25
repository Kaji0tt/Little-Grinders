using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public CanvasGroup mainMenu, saveMenu, optionsMenu, roadMenu, whiteBackground, inputNamePanel;

    [Header("Player Name Setup")]
    [SerializeField] private TMP_InputField nameInputField; // TMPro InputField
    [SerializeField] private Button confirmButton; // Unity Button
    [SerializeField] private TextMeshProUGUI errorMessageText; // TMPro Text für Fehlermeldungen

    private void Start()
    {
        CheckPlayerNameAndToggleMenus();
        
        // Button Event hinzufügen
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmName);
        }
        
        // Enter-Taste Support
        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(delegate { ConfirmName(); });
        }
    }

    private void CheckPlayerNameAndToggleMenus()
    {
        if (!SaveSystem.HasPlayerName())
        {
            // Kein Spielername -> Name Panel zeigen, Main Menu verstecken
            SetCanvasGroupState(inputNamePanel, true);
            SetCanvasGroupState(mainMenu, false);
        }
        else
        {
            // Spielername vorhanden -> Main Menu zeigen, Name Panel verstecken
            SetCanvasGroupState(inputNamePanel, false);
            SetCanvasGroupState(mainMenu, true);
        }
    }

    private void ConfirmName()
    {
        string playerName = nameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(playerName) || playerName.Length < 3)
        {
            ShowError("Bitte gib einen gültigen Namen (mind. 3 Zeichen) ein.");
            return;
        }
        
        // Zusätzliche Validierung für Sonderzeichen
        if (!System.Text.RegularExpressions.Regex.IsMatch(playerName, @"^[a-zA-Z0-9_-]+$"))
        {
            ShowError("Nur Buchstaben, Zahlen, _ und - erlaubt");
            return;
        }
        
        // Spielername speichern
        SaveSystem.SetPlayerName(playerName);
        
        // Error Text verstecken
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
        
        // Menüs umschalten
        SetCanvasGroupState(inputNamePanel, false);
        SetCanvasGroupState(mainMenu, true);
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }
    }

    private void SetCanvasGroupState(CanvasGroup canvasGroup, bool active)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = active ? 1f : 0f;
            canvasGroup.blocksRaycasts = active;
            canvasGroup.interactable = active;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            Application.Quit();
        else
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1f;
        }
    }

    public void ResumeGame()
    {

        UI_Manager.instance.Resume();
    }

    public void ToggleMainMenu()
    {
        mainMenu.alpha = mainMenu.alpha > 0 ? 0 : 1;
        if(whiteBackground != null)whiteBackground.alpha = whiteBackground.alpha > 0 ? 0 : 1;
        mainMenu.blocksRaycasts = mainMenu.blocksRaycasts == true ? false : true;
    }

    public void ToggleSaveMenu()
    {
        saveMenu.alpha = saveMenu.alpha > 0 ? 0 : 1;
        saveMenu.blocksRaycasts = saveMenu.blocksRaycasts == true ? false : true;
    }

    public void ToggleOptionsMenu()
    {
        optionsMenu.alpha = optionsMenu.alpha > 0 ? 0 : 1;
        optionsMenu.blocksRaycasts = optionsMenu.blocksRaycasts == true ? false : true;
    }

    public void ToggleRoadMapMenu()
    {
        roadMenu.alpha = roadMenu.alpha > 0 ? 0 : 1;
        roadMenu.blocksRaycasts = roadMenu.blocksRaycasts == true ? false : true;
    }


    
    // Sound Option Interface Settings
    public void SetMusicVolume(float newValue) //Set of Values should be configured in UI_Manager.cs, setting of source.volume should be conf
    {

        AudioManager.instance.SetMusicVolume(newValue);
    }
    
    public void SetInterfaceVolume(float newValue) 
    {

        AudioManager.instance.SetInterfaceVolume(newValue);
    }


    public void SetAtmosphereVolume(float newValue) 
    {

        AudioManager.instance.SetAtmosphereVolume(newValue);
    }
    public void SetEffectVolume(float newValue)
    {

        AudioManager.instance.SetEffectVolume(newValue);
    }
    

}
