using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script to Manage the Sorting-Order of the Sprite in reference to the Camera Distance. Essential for 2.5D visualisation for Mobs.
public class MobsCamScript : MonoBehaviour
{
    //Sorting Order Values
    private Vector3 CameraPosition;
    private float DistSelfCamera;

    [SerializeField] //5000 is Standard value, from which the Distance is being substracted.
    private int sortingOrderBase = 5000;

    private SpriteRenderer sprite;
    private ParticleSystemRenderer particleRenderer;

    public void ReduceSpriteStartingPoint()
    {
        sortingOrderBase = 4800;
    }

    public void ResetSpriteStartingPoint()
    {
        sortingOrderBase = 5000;
    }

    void Start()
    {
        // Prüfe auf SpriteRenderer
        if (GetComponent<SpriteRenderer>() != null)
        {
            sprite = GetComponent<SpriteRenderer>();
            //Debug.Log($"[MobsCamScript] SpriteRenderer gefunden auf {gameObject.name}");
        }

        // Prüfe auf ParticleSystemRenderer
        if (GetComponent<ParticleSystemRenderer>() != null)
        {
            particleRenderer = GetComponent<ParticleSystemRenderer>();
            //Debug.Log($"[MobsCamScript] ParticleSystemRenderer gefunden auf {gameObject.name}");
        }

        // Warnung wenn nichts gefunden wurde
        if (sprite == null && particleRenderer == null)
        {
            Debug.LogWarning($"[MobsCamScript] Weder SpriteRenderer noch ParticleSystemRenderer auf {gameObject.name} gefunden!");
        }
    }

    void Update()
    {
        CameraPosition = CameraManager.instance.GetCameraPosition();
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;

        int calculatedSortingOrder = (int)(sortingOrderBase - DistSelfCamera);

        // Update SpriteRenderer sorting order
        if (sprite != null)
        {
            sprite.sortingOrder = calculatedSortingOrder;
        }

        // Update ParticleSystemRenderer sorting order
        if (particleRenderer != null)
        {
            particleRenderer.sortingOrder = calculatedSortingOrder;
        }
    }
}
