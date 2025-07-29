using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    #region Singleton
    public static CameraManager instance;
    private void Awake()
    {
        // Singleton Pattern mit DontDestroyOnLoad
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Scene-Wechsel Event registrieren
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Event deregistrieren
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    [Header("Camera References")]
    public GameObject mainCamGO;
    public Camera mainCam;
    public Camera activeCam;

    // Event für andere Scripts wenn Kamera bereit ist
    public System.Action OnCameraInitialized;

    // Debug-Kontrolle für Spam-Vermeidung
    private bool hasLoggedWarning = false;
    private int warningCount = 0;
    private float lastWarningTime = 0f;

    private void Start()
    {
        InitializeCameraReferences();
    }

    // Wird bei jedem Szenenwechsel aufgerufen
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset Warning-Flags bei Szenenwechsel
        hasLoggedWarning = false;
        warningCount = 0;
        
        StartCoroutine(InitializeCameraAfterSceneLoad());
    }

    private IEnumerator InitializeCameraAfterSceneLoad()
    {
        // Kurz warten bis alle Objekte geladen sind
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // Extra Frame für Sicherheit
        
        InitializeCameraReferences();
    }

    private void InitializeCameraReferences()
    {
        // Finde MainCam über Tag
        FindMainCameraByTag();

        // Event auslösen wenn Kamera bereit ist
        if (mainCam != null)
        {
            OnCameraInitialized?.Invoke();
            
            // Reset Warning-Flags wenn Kamera gefunden
            hasLoggedWarning = false;
            warningCount = 0;
        }
    }

    private void FindMainCameraByTag()
    {
        // Suche nach GameObject mit "MainCamera" Tag
        GameObject mainCamObject = GameObject.FindGameObjectWithTag("MainCamera");
        
        if (mainCamObject != null)
        {
            mainCamGO = mainCamObject;
            mainCam = mainCamObject.GetComponent<Camera>();
            
            if (mainCam != null)
            {
                activeCam = mainCam;
                Debug.Log("MainCam found and initialized.");
            }
            else
            {
                Debug.LogWarning("MainCam GameObject found but no Camera component!");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject with 'MainCamera' tag found in scene!");
            
            // Fallback: Suche nach Camera.main
            if (Camera.main != null)
            {
                mainCam = Camera.main;
                mainCamGO = mainCam.gameObject;
                activeCam = mainCam;
                
                Debug.Log("Using Camera.main as fallback");
            }
        }
    }

    // Sichere Methode um Kamera-Position zu bekommen
    public Vector3 GetCameraPosition()
    {
        if (mainCam != null && mainCam.transform != null)
        {
            return mainCam.transform.position;
        }
        
        // Reduzierte Debug-Ausgabe
        LogCameraWarning();
        return Vector3.zero;
    }

    // Sichere Methode um zu prüfen ob Kamera verfügbar ist
    public bool IsCameraReady()
    {
        return mainCam != null && mainCam.transform != null;
    }

    // Intelligentes Warning-System
    private void LogCameraWarning()
    {
        warningCount++;
        
        // Erste Warnung sofort
        if (!hasLoggedWarning)
        {
            Debug.LogWarning("CameraManager: MainCam not available during initialization. This is normal.");
            hasLoggedWarning = true;
            lastWarningTime = Time.time;
        }
        // Danach nur alle 5 Sekunden oder alle 100 Aufrufe
        else if (Time.time - lastWarningTime > 5f || warningCount % 100 == 0)
        {
            Debug.LogWarning($"CameraManager: MainCam still not available after {warningCount} attempts. Check camera setup!");
            lastWarningTime = Time.time;
        }
    }

    // Für andere Scripts: Warten auf Kamera-Initialisierung
    public IEnumerator WaitForCameraReady()
    {
        while (!IsCameraReady())
        {
            yield return null;
        }
    }
}
