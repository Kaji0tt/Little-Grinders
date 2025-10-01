using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VFX_XPOrb : MonoBehaviour
{
    [Header("XP Configuration")]
    public int xpValue;
    
    [Header("Movement Settings")]
    public float attractionDistance = 2f;
    public float attractionSpeed = 2f;
    public float baseSpeed = 1f;
    
    [Header("Magnetic Repulsion Settings")]
    public float magneticRepulsionDistance = 0.4f;
    public float magneticRepulsionForce = 0.5f;
    public float magneticDamping = 0.85f;
    
    [Header("Implosion Settings")]
    public float implosionDuration = 0.5f;
    public AnimationCurve implosionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Scale Settings")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float scaleAnimationDuration = 0.3f;
    
    private Transform player;
    private bool isBeingAttracted = false;
    private bool isCollected = false;
    private bool isImploding = false;
    private bool isInitialized = false;
    
    // Alle PartikelSysteme (Haupt + Children)
    private ParticleSystem[] allParticleSystems;
    private ParticleSystemData[] originalParticleData;
    
    // Für sanfte Bewegung
    private Vector3 velocity = Vector3.zero;
    private List<VFX_XPOrb> nearbyOrbs = new List<VFX_XPOrb>();
    
    // Collision-Kontrolle
    private Collider myCollider;
    
    // Struktur für Original-Werte jedes PartikelSystems
    [System.Serializable]
    private struct ParticleSystemData
    {
        public float originalStartSize;
        public float originalStartSpeed;
        public float originalEmissionRate;
        public Color originalStartColor;
        public string name; // Für Debug
    }
    
    void Awake()
    {
        // Initialisierung in Awake() um sicherzustellen, dass es vor Initialize() läuft
        InitializeParticleSystems();
    }
    
    void InitializeParticleSystems()
    {
        if (isInitialized) return;
        
        myCollider = GetComponent<Collider>();
        
        // Sammle ALLE PartikelSysteme (Haupt + Children)
        allParticleSystems = GetComponentsInChildren<ParticleSystem>();
        
        if (allParticleSystems.Length == 0)
        {
            return;
        }
        
        // Speichere Original-Werte für ALLE PartikelSysteme
        StoreOriginalParticleData();
        
        isInitialized = true;
    }
    
    void Start()
    {
        player = PlayerManager.instance.player.transform;
        
        if (player == null)
        {
            return;
        }
        
        // Falls nicht bereits in Awake initialisiert
        if (!isInitialized)
        {
            InitializeParticleSystems();
        }
        
        // Setze Scale basierend auf XP-Wert (falls bereits gesetzt)
        if (xpValue > 0)
        {
            SetScaleByXPValue();
        }
    }
    
    void StoreOriginalParticleData()
    {
        originalParticleData = new ParticleSystemData[allParticleSystems.Length];
        
        for (int i = 0; i < allParticleSystems.Length; i++)
        {
            var ps = allParticleSystems[i];
            var main = ps.main;
            var emission = ps.emission;
            
            originalParticleData[i] = new ParticleSystemData
            {
                originalStartSize = main.startSize.constant,
                originalStartSpeed = main.startSpeed.constant,
                originalEmissionRate = emission.rateOverTime.constant,
                originalStartColor = main.startColor.color,
                name = ps.gameObject.name
            };
        }
    }
    
    void Update()
    {
        if (isCollected || player == null) return;
        
        // Wenn Implosion läuft, überspringen wir normale Bewegung
        if (isImploding) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 totalForce = Vector3.zero;
        
        // Magnetische Abstoßung von anderen Orbs
        Vector3 magneticRepulsion = CalculateMagneticRepulsion();
        totalForce += magneticRepulsion;
        
        // Anziehung zum Spieler
        if (distanceToPlayer <= attractionDistance)
        {
            if (!isBeingAttracted)
            {
                isBeingAttracted = true;
            }
            
            float attractionStrength = Mathf.Lerp(baseSpeed, attractionSpeed, 
                                         (attractionDistance - distanceToPlayer) / attractionDistance);
            
            Vector3 attractionDirection = (player.position - transform.position).normalized;
            Vector3 attractionForce = attractionDirection * attractionStrength;
            totalForce += attractionForce;
        }
        
        // DIREKTE BEWEGUNG - ohne Physics
        if (totalForce.magnitude > 0.01f)
        {
            Vector3 directMovement = totalForce * Time.deltaTime;
            transform.position += directMovement;
        }
    }
    
    Vector3 CalculateMagneticRepulsion()
    {
        Vector3 totalRepulsion = Vector3.zero;
        
        foreach (VFX_XPOrb otherOrb in nearbyOrbs)
        {
            if (otherOrb == null || otherOrb == this || otherOrb.isCollected) continue;
            
            Vector3 toOther = otherOrb.transform.position - transform.position;
            float distance = toOther.magnitude;
            
            if (distance < magneticRepulsionDistance && distance > 0.01f)
            {
                float repulsionStrength = magneticRepulsionForce * (magneticRepulsionDistance - distance) / magneticRepulsionDistance;
                Vector3 repulsionDirection = -toOther.normalized;
                
                Vector3 repulsionForce = repulsionDirection * repulsionStrength;
                totalRepulsion += repulsionForce;
            }
        }
        
        return totalRepulsion;
    }
    
    // Collision Detection für Orb-zu-Orb Interaktion
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected && !isImploding)
        {
            StartImplosion();
        }
        
        VFX_XPOrb otherOrb = other.GetComponent<VFX_XPOrb>();
        if (otherOrb != null && !nearbyOrbs.Contains(otherOrb))
        {
            nearbyOrbs.Add(otherOrb);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        VFX_XPOrb otherOrb = other.GetComponent<VFX_XPOrb>();
        if (otherOrb != null && nearbyOrbs.Contains(otherOrb))
        {
            nearbyOrbs.Remove(otherOrb);
        }
    }
    
    void StartImplosion()
    {
        if (isCollected || isImploding) return;
        
        isImploding = true;
        isCollected = true;
        
        // WICHTIG: Deaktiviere Collider um weitere Kollisionen zu verhindern
        if (myCollider != null)
        {
            myCollider.enabled = false;
        }
        
        // Entferne diesen Orb aus allen anderen Listen
        foreach (VFX_XPOrb otherOrb in nearbyOrbs)
        {
            if (otherOrb != null)
                otherOrb.nearbyOrbs.Remove(this);
        }
        
        // XP sofort geben (da keine Bewegung mehr stattfindet)
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.Gain_xp(xpValue);
        }
        
        // Audio abspielen
        if (AudioManager.instance != null && playerStats != null)
        {
            AudioManager.instance.PlayEntitySound("XP_Orb", playerStats.gameObject);
        }
        
        StartCoroutine(ImplosionAnimation());
    }
    
    IEnumerator ImplosionAnimation()
    {
        if (allParticleSystems.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }
        
        float elapsed = 0f;
        
        // Aktiviere Module für Implosion bei ALLEN PartikelSystemen
        foreach (var ps in allParticleSystems)
        {
            var velocityOverLifetime = ps.velocityOverLifetime;
            var colorOverLifetime = ps.colorOverLifetime;
            
            velocityOverLifetime.enabled = true;
            colorOverLifetime.enabled = true;
        }
        
        while (elapsed < implosionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / implosionDuration;
            
            // Verwende AnimationCurve für die Skalierung (1 → 0)
            float scaleValue = 1f - implosionCurve.Evaluate(progress);
            
            // Bearbeite ALLE PartikelSysteme
            for (int i = 0; i < allParticleSystems.Length; i++)
            {
                var ps = allParticleSystems[i];
                var originalData = originalParticleData[i];
                
                var main = ps.main;
                var emission = ps.emission;
                var velocityOverLifetime = ps.velocityOverLifetime;
                
                // 1. PARTIKEL-GRÖSSE reduzieren
                main.startSize = originalData.originalStartSize * GetScaleMultiplier(xpValue) * scaleValue;
                
                // 2. EMISSION-RATE reduzieren (weniger neue Partikel)
                emission.rateOverTime = originalData.originalEmissionRate * GetScaleMultiplier(xpValue) * scaleValue;
                
                // 3. GESCHWINDIGKEIT zum Zentrum (Implosions-Effekt)
                velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(-progress * 5f);
                
                // 4. FARBE/ALPHA fade out
                Color fadeColor = originalData.originalStartColor;
                fadeColor.a = scaleValue; // Alpha reduzieren
                main.startColor = fadeColor;
                
                // 5. PARTIKEL-GESCHWINDIGKEIT erhöhen für dramatischen Effekt
                main.startSpeed = originalData.originalStartSpeed * (1f + progress * 2f);
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void SetScaleByXPValue()
    {
        // Sicherheitscheck: Falls noch nicht initialisiert, mache es jetzt
        if (!isInitialized)
        {
            InitializeParticleSystems();
        }
        
        if (allParticleSystems == null || allParticleSystems.Length == 0)
        {
            return;
        }
        
        if (originalParticleData == null)
        {
            return;
        }
        
        float scaleMultiplier = GetScaleMultiplier(xpValue);
        
        for (int i = 0; i < allParticleSystems.Length; i++)
        {
            var ps = allParticleSystems[i];
            var originalData = originalParticleData[i];
            
            var main = ps.main;
            var emission = ps.emission;
            
            main.startSize = originalData.originalStartSize * scaleMultiplier;
            main.startSpeed = originalData.originalStartSpeed * scaleMultiplier;
            emission.rateOverTime = originalData.originalEmissionRate * scaleMultiplier;
        }
    }
    
    float GetScaleMultiplier(int xp)
    {
        return xp switch
        {
            1 => 0.1f,
            2 => 0.2f,
            5 => 0.3f,
            10 => 0.5f,
            20 => 0.75f,
            50 => 1.00f,
            _ => .1f
        };
    }
    
    public void Initialize(int xp)
    {
        xpValue = xp;
        
        // Sicherheitscheck: Falls noch nicht initialisiert, mache es jetzt
        if (!isInitialized)
        {
            InitializeParticleSystems();
        }
        
        SetScaleByXPValue();
    }
    
    void OnDestroy()
    {
        foreach (VFX_XPOrb otherOrb in nearbyOrbs)
        {
            if (otherOrb != null)
                otherOrb.nearbyOrbs.Remove(this);
        }
    }
}