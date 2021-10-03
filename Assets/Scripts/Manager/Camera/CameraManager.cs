using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Singleton
    public static CameraManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    public GameObject mainCamGO;
    public GameObject mapCamGO;

    public Camera mainCam;
    public Camera mapCam;

    public Camera activeCam;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            activeCam = mapCam;
        else
            activeCam = mainCam;

    }

}
