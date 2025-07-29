using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI damageText;
    private CanvasGroup canvasGroup;

    private float lifetime = 1.0f;

    private bool isCrit = false;
    private float moveDelay = 0f; // Zeit, bis das Popup sich bewegt
    private float fadeDuration = 0.5f;
    private float scaleDuration = 0.2f;
    private float elapsed;

    private Vector3 moveVector = new Vector3(0, 1f, 0.5f);
    private Vector3 startScale = Vector3.one * .1f;
    private Vector3 startScaleCrit = Vector3.one * .3f;
    private Vector3 maxScale = Vector3.one * .5f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        damageText = GetComponentInChildren<TextMeshProUGUI>();

        if (damageText == null)
        {
            Debug.LogError("TextMeshProUGUI not found in children!");
        }
    }

    public void Setup(float damage)
    {
        isCrit = false;
        moveDelay = 0f;

        if (damageText != null)
        {
            //Debug.Log("No Crit!");
            damageText.text = damage.ToString("0");
            damageText.color = Color.white;
        }
        transform.localScale = startScale;

        if (canvasGroup != null && canvasGroup.isActiveAndEnabled)
            canvasGroup.alpha = 1f;
    }

    public void SetupCrit(float damage)
    {
        isCrit = true;
        moveDelay = 0.8f; // Crits bleiben z.B. 0.8 Sekunden stehen

        if (damageText != null)
        {
            Debug.Log("Crit!");
            damageText.text = damage.ToString("0");
            damageText.color = Color.red;
        }
        transform.localScale = startScaleCrit;

        if (canvasGroup != null && canvasGroup.isActiveAndEnabled)
            canvasGroup.alpha = 1f;
    }

    public void SetupDirect(float damage)
    {
        isCrit = false;
        moveDelay = 0f;

        if (damageText != null)
        {
            damageText.text = damage.ToString("0");
            damageText.color = new Color(0.7f, 0.5f, 1f); // Lila/Weißlich
        }
        transform.localScale = startScale;

        if (canvasGroup != null && canvasGroup.isActiveAndEnabled)
            canvasGroup.alpha = 1f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        // 1. Zur Kamera drehen
        transform.forward = Camera.main.transform.forward;

        // 2. Nach oben wandern (erst nach moveDelay)
        if (elapsed > moveDelay)
        {
            transform.position += moveVector * Time.deltaTime;
        }

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

        // 5. Zerst�ren am Ende
        if (elapsed > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
}