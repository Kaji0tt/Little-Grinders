using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inter_Portal : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private string activationVFXName = "VFX_PortalActivation";

    public void Use()
    {
        if(GetComponentInChildren<Light>())
        {
            GetComponentInChildren<Light>().intensity = 1;
        }

        // VFX abspielen wenn Portal aktiviert wird
        PlayActivationVFX();

        GlobalMap.instance.currentMap.gotTeleporter = true;

        UI_GlobalMap.instance.CalculateExploredMaps();
        //später muss hier auch eingefügt werden, dass sich der Current MapSave um einen Teleport erweitert. Dieser muss so hinterlegt / abgespeichert werden,
        //dass dieser auch im Load an exakter Position neu spawned.
    }

    private void PlayActivationVFX()
    {
        if (VFX_Manager.instance != null)
        {
            // VFX an der Portal-Position abspielen
            VFX_Manager.instance.PlayEffect(activationVFXName, this.transform.position, Quaternion.identity);
        }
        else
        {
            // Fallback falls VFX_Manager nicht verfügbar
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Portal activated!", 2f);
            }
            else
            {
                Debug.Log("Portal activated - VFX Manager not available");
            }
        }
    }
}
