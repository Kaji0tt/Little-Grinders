using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PPVolumeManager : MonoBehaviour
{
    #region Singleton
    public static PPVolumeManager instance;
    private void Awake()
    {
        instance = this;

    }
    #endregion

    public Volume volume;

    void Start()
    {


    }


    public void LowHealthPP(float value)
    {
        float vigMaxVol = 0.5f;
        float vigStandardVol = 0.32f;


        Vignette vignette;

        if (volume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.value = Mathf.Lerp(vigMaxVol, vigStandardVol, value);
        }

    }
}
