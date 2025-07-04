using UnityEngine;
using System.Collections;

public class Level_Up_FX : MonoBehaviour
{
    public GameObject levelUpFX_A;
    public GameObject levelUpFX_B;
    public GameObject spiralMesh;
    public Light levelup_Light;

    private float fadeStart = 5.0f;
    private float fadeEnd = 0;
    private float fadeTime = 2.0f;
    private float t = 0.0f;

    private Renderer spiralMeshRenderer;

    // Die Start()-Methode wird nicht mehr benötigt, um die Coroutine zu starten.
    void Awake()
    {
        // Objekte initial deaktivieren, damit sie nicht kurz aufblitzen.
        levelUpFX_A.SetActive(false);
        levelUpFX_B.SetActive(false);
        spiralMeshRenderer = spiralMesh.GetComponent<Renderer>();
    }

    // Neue öffentliche Methode, die vom VFX_Manager aufgerufen wird.
    public void Start()
    {
        StartCoroutine("LevelUp");
    }

    IEnumerator LevelUp()
    {
        levelUpFX_A.SetActive(true);
        StartCoroutine("SpiralMagic");

        yield return new WaitForSeconds(0.47f);

        levelUpFX_B.SetActive(true);
        fadeStart = 5.0f;
        StartCoroutine("FadeLight");
        
        // Warten, bis die Animation vorbei ist.
        yield return new WaitForSeconds(2.5f);

        // Am Ende das gesamte GameObject zerstören, um die Szene sauber zu halten.
        Destroy(gameObject);
    }

    IEnumerator SpiralMagic()
    {
        float offset = 0;
        while (offset < 1.0f)
        {
            offset += (Time.deltaTime * 0.4f);
            Vector2 offsetVector = new Vector2(0, -offset);
            spiralMeshRenderer.material.SetTextureOffset("_MainTex", offsetVector);
            yield return 0;
        }
    }

    IEnumerator FadeLight()
    {
        t = 0; // Sicherstellen, dass t zurückgesetzt ist
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            levelup_Light.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
            yield return 0;
        }
    }
}
