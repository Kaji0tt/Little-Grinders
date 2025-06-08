using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI damageText;
    private CanvasGroup canvasGroup;

    private float lifetime = 1.0f;
    private float fadeDuration = 0.5f;
    private float scaleDuration = 0.2f;
    private float elapsed;

    private Vector3 moveVector = new Vector3(0, 1f, 0.5f); // Richtung nach oben + leicht zur Kamera
    private Vector3 startScale = Vector3.one * 2f;
    private Vector3 maxScale = Vector3.one * 3.5f;


    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        damageText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(float damage)
    {
        damageText.text = damage.ToString("0");
        transform.localScale = startScale;
        canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        // 1. Zur Kamera drehen
        transform.forward = Camera.main.transform.forward;

        // 2. Nach oben wandern
        transform.position += moveVector * Time.deltaTime;

        // 3. Skalieren (Pop-in Effekt)
        if (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            transform.localScale = Vector3.Lerp(startScale, maxScale, t);
        }

        // 4. Ausfaden am Ende
        if (elapsed > lifetime - fadeDuration)
        {
            float t = 1 - (lifetime - elapsed) / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
        }

        // 5. Zerstören am Ende
        if (elapsed > lifetime)
        {
            Destroy(gameObject);
        }
    }
}