using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 0.125f;

    [SerializeField] private float min = 10f;
    [SerializeField] private float max = 20f;
    private float zoom;

    [Header("Overview Settings")]
    [SerializeField] private Vector3 overviewPositionOffset = new Vector3(0, 8, -4);
    [SerializeField] private Vector3 overviewRotationOffset = new Vector3(20, 0, 0);
    [SerializeField] private float overviewFieldOfView = 70f;
    [SerializeField] private float transitionSpeed = 3f;

    [Header("Camera Lock Settings")]
    [SerializeField] private bool cameraLocked = false; // Standard: Kamera ist nicht gelockt
    [SerializeField] private float mouseInfluenceStrength = 2f; // Wie stark der Einfluss ist
    [SerializeField] private float mouseInfluenceMaxDistance = 5f; // Maximale Entfernung vom Player
    [SerializeField] private float mouseInfluenceSmoothing = 2f; // Wie schnell sich die Kamera anpasst

    // Overview System
    private bool isInOverviewMode = false;
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalRotation;
    private float originalFieldOfView;

    // Camera Lock System
    private Vector3 mouseInfluenceOffset = Vector3.zero;

    // Direkte Kamera-Referenz
    private Camera cam;

    RaycastHit[] raycastHits;

    void Start()
    {
        // Direkte Kamera-Referenz
        cam = GetComponent<Camera>();
        
        if (cam == null)
        {
            Debug.LogError("CameraFollow: No Camera component found on this GameObject!");
            return;
        }

        // Ursprungs-Einstellungen speichern
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localEulerAngles;
        originalFieldOfView = cam.fieldOfView;

        raycastHits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), Mathf.Infinity);
    }

    private void Update()
    {
        if (cam == null) return;

        // Zoom-Kontrolle (nur wenn nicht im Overview-Modus)
        if (!isInOverviewMode && Input.mouseScrollDelta.y != 0 && Time.timeScale == 1f)
        {
            if (!IsMouseOverUIWithIgnores())
                Zoom();
        }

        // Overview-Kontrolle
        HandleOverviewInput();

        // Camera Lock Kontrolle
        HandleCameraLockInput();

        // Mouse Influence berechnen (nur wenn nicht gelockt und nicht im Overview)
        if (!cameraLocked && !isInOverviewMode)
        {
            UpdateMouseInfluence();
        }
        
        // Kamera-Transitions
        UpdateCameraTransition();

        // Raycast-System für Transparenz
        HandleEnvironmentTransparency();
    }

    private void HandleOverviewInput()
    {
        // Tab gedrückt - Overview Mode
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            EnableOverviewMode();
        }
        
        // Tab losgelassen - Normal Mode
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            DisableOverviewMode();
        }
    }

    private void HandleCameraLockInput()
    {
        // Debug: Prüfe KeyManager-Status
        if (KeyManager.MyInstance == null)
        {
            Debug.LogWarning("KeyManager.MyInstance is null!");
            return;
        }

        if (!KeyManager.MyInstance.Keybinds.ContainsKey("CAMERA LOCK"))
        {
            Debug.LogWarning("CAMERA LOCK key not found in Keybinds!");
            return;
        }

        KeyCode lockKey = KeyManager.MyInstance.Keybinds["CAMERA LOCK"];
        
        // Debug: Zeige aktuellen Key
        if (Input.GetKeyDown(KeyCode.Mouse2)) // Direkte Prüfung
        {
            Debug.Log("Mouse2 pressed directly!");
        }
        
        if (Input.GetKeyDown(lockKey))
        {
            Debug.Log($"Camera lock key pressed: {lockKey}");
            ToggleCameraLock();
        }
    }

    private void ToggleCameraLock()
    {
        cameraLocked = !cameraLocked;

        if (LogScript.instance != null)
        {
            string status = cameraLocked ? "LOCKED" : "UNLOCKED";
            LogScript.instance.ShowLog($"Camera {status}", 1.5f);
        }

        // Reset Mouse Influence wenn Kamera gelockt wird
        if (cameraLocked)
        {
            mouseInfluenceOffset = Vector3.zero;
        }
    }

    private void UpdateMouseInfluence()
    {
        // Screen-Edge basierte Mouse Influence
        Vector3 targetInfluence = CalculateScreenEdgeInfluence();

        // Sanfte Interpolation zum Ziel-Offset
        mouseInfluenceOffset = Vector3.Lerp(
            mouseInfluenceOffset, 
            targetInfluence, 
            Time.deltaTime * mouseInfluenceSmoothing
        );
    }

    private Vector3 CalculateScreenEdgeInfluence()
    {
        Vector2 mouseScreenPos = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );

        Vector2 screenOffset = mouseScreenPos - Vector2.one * 0.5f;


        // WICHTIG: Verwende WELT-Koordinaten, nicht lokale!
        // Da die Kamera im Player-Objekt ist und dessen Rotation erbt,
        // müssen wir die Transformation in Welt-Koordinaten machen

        // Hole die Parent-Transform (Player) für Welt-Orientierung
        Transform playerTransform = transform.parent;
        
        Vector3 worldForward = Vector3.forward;
        Vector3 worldRight = Vector3.right;

        if (playerTransform != null)
        {
            // Verwende die Player-Transform für die Welt-Orientierung
            worldForward = playerTransform.forward;
            worldForward.y = 0;
            worldForward = Vector3.Normalize(worldForward);
            worldRight = Quaternion.Euler(new Vector3(0, 90, 0)) * worldForward;
        }

        // Berechne Einfluss in Welt-Koordinaten
        Vector3 worldInfluence = (worldRight * screenOffset.x + worldForward * screenOffset.y) * mouseInfluenceStrength;

        // Transformiere zurück in lokale Koordinaten der Kamera
        Vector3 localInfluence = transform.InverseTransformDirection(worldInfluence);

        // Debug-Output
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log($"World Forward: {worldForward}, World Right: {worldRight}");
            Debug.Log($"World Influence: {worldInfluence}, Local Influence: {localInfluence}");
        }

        return Vector3.ClampMagnitude(localInfluence, mouseInfluenceMaxDistance);
    }

    private void EnableOverviewMode()
    {
        if (isInOverviewMode) return;
        
        isInOverviewMode = true;

        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Overview Mode ON", 1f);
        }
    }

    private void DisableOverviewMode()
    {
        if (!isInOverviewMode) return;
        
        isInOverviewMode = false;

        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Overview Mode OFF", 1f);
        }
    }

    private void UpdateCameraTransition()
    {
        // Berechne Basis-Zielwerte basierend auf Overview-Modus
        Vector3 baseTargetPosition = isInOverviewMode ? 
            originalLocalPosition + overviewPositionOffset : 
            originalLocalPosition;

        Vector3 targetRotation = isInOverviewMode ? 
            originalLocalRotation + overviewRotationOffset : 
            originalLocalRotation;

        float targetFOV = isInOverviewMode ? 
            overviewFieldOfView : 
            originalFieldOfView;

        // Füge Mouse Influence hinzu (nur wenn nicht im Overview-Modus)
        Vector3 finalTargetPosition = baseTargetPosition;
        if (!isInOverviewMode)
        {
            finalTargetPosition += mouseInfluenceOffset;
        }

        // Sanfte Interpolation
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, 
            finalTargetPosition, 
            Time.deltaTime * transitionSpeed
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            Quaternion.Euler(targetRotation),
            Time.deltaTime * transitionSpeed
        );

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFOV,
            Time.deltaTime * transitionSpeed
        );
    }

    private void HandleEnvironmentTransparency()
    {
        if (PlayerManager.instance?.player == null) return;

        Vector3 playerPos = PlayerManager.instance.player.transform.position;
        Vector3 camPos = transform.position;
        Vector3 direction = (camPos - playerPos).normalized;
        float distance = Vector3.Distance(playerPos, camPos);

        raycastHits = Physics.RaycastAll(playerPos, direction, distance);

        HashSet<GameObject> hitEnvObjects = new HashSet<GameObject>();
        foreach (RaycastHit hit in raycastHits)
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Env"))
            {
                hitEnvObjects.Add(hit.collider.gameObject);
            }
        }

        foreach (GameObject envObj in GameObject.FindGameObjectsWithTag("Env"))
        {
            SpriteRenderer sr = envObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var renderer = sr as Renderer;
                if (hitEnvObjects.Contains(envObj))
                {
                    sr.color = new Color(1, 1, 1, 0.3f);
                    renderer.lightProbeUsage = LightProbeUsage.CustomProvided;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                else
                {
                    sr.color = new Color(1, 1, 1, 1f);
                    renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                }
            }
        }
    }

    private bool IsMouseOverUIWithIgnores()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);

        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject.GetComponent<ClickThrough>() != null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }

        return raycastResultList.Count > 0;
    }

    void Zoom()
    {
        max = 20.0f;
        min = 10.0f;

        if (Input.mouseScrollDelta.y > 0 && zoom > min)
        {
            cam.fieldOfView = cam.fieldOfView - Input.mouseScrollDelta.y;
        }

        if (Input.mouseScrollDelta.y < 0 && zoom < max)
        {
            cam.fieldOfView = cam.fieldOfView - Input.mouseScrollDelta.y;
        }
        
        zoom = cam.fieldOfView;

        if (!isInOverviewMode)
        {
            originalFieldOfView = zoom;
        }

        if (PlayerManager.instance?.player != null)
        {
            var isometricPlayer = PlayerManager.instance.player.GetComponent<IsometricPlayer>();
            if (isometricPlayer != null)
            {
                isometricPlayer.userFOV = zoom;
            }
        }
    }

    // Öffentliche Methoden
    public bool IsInOverviewMode() => isInOverviewMode;
    public bool IsCameraLocked() => cameraLocked;
    public Vector3 GetCameraPosition() => transform.position;
}
