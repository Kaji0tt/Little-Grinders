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
    
    // Enhanced feedback properties
    private bool useShake = false;
    private float shakeIntensity = 0f;
    private Vector3 originalPosition;
    
    // Damage type colors
    private static readonly Color normalColor = Color.white;
    private static readonly Color critColor = Color.red;
    private static readonly Color directColor = new Color(0.7f, 0.5f, 1f); // Purple
    private static readonly Color healColor = Color.green;
    private static readonly Color specialColor = Color.yellow;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        damageText = GetComponentInChildren<TextMeshProUGUI>();

        if (damageText == null)
        {
            Debug.LogError("TextMeshProUGUI not found in children!");
        }
        
        originalPosition = transform.position;
    }

    public void Setup(float damage)
    {
        SetupInternal(damage, normalColor, false, false);
    }

    public void SetupCrit(float damage)
    {
        SetupInternal(damage, critColor, true, true);
    }

    public void SetupDirect(float damage)
    {
        SetupInternal(damage, directColor, false, false);
    }
    
    public void SetupHeal(float amount)
    {
        SetupInternal(amount, healColor, false, false, "+");
    }
    
    public void SetupSpecial(float damage, string prefix = "")
    {
        SetupInternal(damage, specialColor, false, true, prefix);
    }

    private void SetupInternal(float value, Color color, bool isCritical, bool useShakeEffect, string prefix = "")
    {
        isCrit = isCritical;
        moveDelay = isCritical ? 0.8f : 0f;
        useShake = useShakeEffect;
        shakeIntensity = useShakeEffect ? (isCritical ? 0.05f : 0.02f) : 0f;

        if (damageText != null)
        {
            string displayText = prefix + value.ToString("0");
            damageText.text = displayText;
            damageText.color = color;
            
            // Enhanced font styling for critical hits
            if (isCritical)
            {
                damageText.fontStyle = FontStyles.Bold;
                damageText.fontSize *= 1.2f; // Make crit text larger
            }
        }
        
        transform.localScale = isCritical ? startScaleCrit : startScale;

        if (canvasGroup != null && canvasGroup.isActiveAndEnabled)
            canvasGroup.alpha = 1f;
            
        originalPosition = transform.position;
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
        
        // 2.5. Shake effect for enhanced feedback
        if (useShake && elapsed < scaleDuration)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );
            transform.position = originalPosition + shakeOffset;
        }

        // 3. Skalieren (Pop-in Effekt mit enhanced curve für kritische Treffer)
        if (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            
            // Enhanced scaling curve for critical hits
            if (isCrit)
            {
                // Overshoot curve for more impact
                float overshoot = 1.2f;
                if (t < 0.6f)
                {
                    float overshootT = t / 0.6f;
                    transform.localScale = Vector3.Lerp(startScaleCrit, maxScale * overshoot, overshootT);
                }
                else
                {
                    float settleT = (t - 0.6f) / 0.4f;
                    transform.localScale = Vector3.Lerp(maxScale * overshoot, maxScale, settleT);
                }
            }
            else
            {
                transform.localScale = Vector3.Lerp(startScale, maxScale, t);
            }
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