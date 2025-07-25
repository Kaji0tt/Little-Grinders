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


    [SerializeField]
    private float min = 10f;
    [SerializeField]
    private float max = 20f;

    private float zoom;

    //List<RaycastHit> hitsList = new List<RaycastHit>();

    RaycastHit[] raycastHits;

    //List<SpriteRenderer> lowAlphaSprites = new List<SpriteRenderer>();

    //public float zoomSpeed = 4f;
    //private float currentZoom = 10f;
    /*
    private bool perspectiveView;

    private Action[] toggleCamPos = new Action[4];

    private int activeCam = 0;
    */
    void Awake()
    {
        //toggleCamPos[0] = CamPos1;
        //toggleCamPos[1] = CamPos2;
        //toggleCamPos[2] = CamPos3;
        //toggleCamPos[3] = CamPos4;

    }

    void Start()
    {
        raycastHits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), Mathf.Infinity);
    }

    private void Update()
    {

        if (Input.mouseScrollDelta.y != 0 && Time.timeScale == 1f)
            if (!IsMouseOverUIWithIgnores())
                Zoom();

        // Berechne Abstand zur Kamera
        float distSelfCamera = (PlayerManager.instance.player.transform.position - CameraManager.instance.activeCam.transform.position).sqrMagnitude;


        // Raycast von Spieler zur Kamera
        Vector3 playerPos = PlayerManager.instance.player.transform.position;
        Vector3 camPos = transform.position;
        Vector3 direction = (camPos - playerPos).normalized;
        float distance = Vector3.Distance(playerPos, camPos);

        raycastHits = Physics.RaycastAll(playerPos, direction, distance);

        //Debug.Log($"[CameraFollow] Raycast from {playerPos} to {camPos} | Direction: {direction} | Distance: {distance} | Hits: {raycastHits.Length}");

        // Sammle alle getroffenen "Env"-Objekte in ein HashSet
        HashSet<GameObject> hitEnvObjects = new HashSet<GameObject>();
        foreach (RaycastHit hit in raycastHits)
        {
            if (hit.collider != null)
            {
                //Debug.Log($"[CameraFollow] RaycastHit: {hit.collider.gameObject.name} | Tag: {hit.collider.gameObject.tag}");
                if (hit.collider.gameObject.CompareTag("Env"))
                {
                    hitEnvObjects.Add(hit.collider.gameObject);
                    SpriteRenderer sr = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        //Debug.Log($"[CameraFollow] SpriteRenderer FOUND on {hit.collider.gameObject.name}");
                    }
                    else
                    {
                        //Debug.LogWarning($"[CameraFollow] SpriteRenderer MISSING on {hit.collider.gameObject.name}");
                    }
                }
            }
            else
            {
                //Debug.LogWarning("[CameraFollow] RaycastHit with NULL collider!");
            }
        }

        // Setze für jedes "Env"-Objekt die Transparenz je nach Raycast-Hit
        foreach (GameObject envObj in GameObject.FindGameObjectsWithTag("Env"))
        {
            SpriteRenderer sr = envObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var renderer = sr as Renderer;
                if (hitEnvObjects.Contains(envObj))
                {
                    sr.color = new Color(1, 1, 1, 0.3f);
                    // LightProbeUsage auf 4 (Custom Provided) setzen
                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.CustomProvided;
                    // Schattenwurf deaktivieren
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    //Debug.Log($"[CameraFollow] Set TRANSPARENT & LightProbeUsage=CustomProvided & Shadows=Off: {envObj.name}");
                }
                else
                {
                    sr.color = new Color(1, 1, 1, 1f);
                    // LightProbeUsage zurück auf Blend Probes (Standard: 1)
                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                    // Schattenwurf wieder aktivieren (z.B. On)
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    //Debug.Log($"[CameraFollow] Set OPAQUE & LightProbeUsage=BlendProbes & Shadows=On: {envObj.name}");
                }
            }
            else
            {
                //Debug.LogWarning($"[CameraFollow] SpriteRenderer MISSING on EnvObj: {envObj.name}");
            }
        }
    }

    private bool IsMouseOverUIWithIgnores() //C @CodeMonkey
    {
        //Erstelle eine lokale Variabel von der Position der Maus
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        //Erstelle eine Liste aus Raycast-Daten
        List<RaycastResult> raycastResultList = new List<RaycastResult>();

        //Erstelle Raycasts von der Position der Maus und speichere ihre Ergebnisse in der Liste
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);

        //Filter die Liste auf vom Raycast getroffene Objekte und entferne jene, welche das ClickThrough Skript besitzen.
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
        //Ggf. sollten die Mausrad zoomies invertiert werden.
        //float _zoom = Camera.main.fieldOfView;

        //float max, min;
        max = 20.0f;
        min = 10.0f;

        if (Input.mouseScrollDelta.y > 0 && zoom > min)
        {
            CameraManager.instance.mainCam.fieldOfView = CameraManager.instance.mainCam.fieldOfView - Input.mouseScrollDelta.y;
            //PlayerManager.instance.player.GetComponent<IsometricPlayer>().userFOV = CameraManager.instance.mainCam.fieldOfView;
        }

        if (Input.mouseScrollDelta.y < 0 && zoom < max)
        {
            CameraManager.instance.mainCam.fieldOfView = CameraManager.instance.mainCam.fieldOfView - Input.mouseScrollDelta.y;
            //PlayerManager.instance.player.GetComponent<IsometricPlayer>().userFOV = CameraManager.instance.mainCam.fieldOfView;
        }
        zoom = CameraManager.instance.mainCam.fieldOfView;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().userFOV = zoom;

    }

}
