using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TroubleShootPlayerPrefs : MonoBehaviour
{
    // Start is called before the first frame update
    public void DebugPlayerPrefsCords()
    {
        Debug.Log("Current PlayerPref Cords:" + GlobalMap.instance.currentPosition);

    }

    public void DebugGlobalMapCurrentMap()
    {
        Debug.Log("GlobalMap.GetCurrentMap Cords:" + GlobalMap.instance.GetMapCoordinates(GlobalMap.instance.currentMap));
    }

    public void DebugGlobalMapCurrentMapVariabel()
    {
        Debug.Log("GlobalMap.currentMap Cords:" + GlobalMap.instance.currentMap.mapIndexX + GlobalMap.instance.currentMap.mapIndexY);
    }
}
