using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VFX_LevelUpEffect : MonoBehaviour
{
    private ParticleSystem ps;

    // Expose the duration for other scripts
    public float Duration => ps.main.duration + ps.main.startLifetime.constantMax;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ConfigureEffect();
    }

    /// <summary>
    /// Konfiguriert die Standardeinstellungen für den Level-Up-Partikeleffekt.
    /// Diese Werte können hier oder zur Laufzeit angepasst werden.
    /// </summary>
    void ConfigureEffect()
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 1.0f;
        main.startLifetime = 1.0f;
        main.startSpeed = 5.0f; // Keep the high speed for the explosion feel
        main.startSize = 0.5f;
        main.startColor = new Color(1.0f, 0.9f, 0.4f, 1.0f); // Gold
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission: A short burst of particles
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(
            new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, 50)
            });

        // Shape: A narrow cone pointing straight up
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15.0f; // A narrower angle to focus the explosion upwards
        shape.radius = 0.1f; // Emit from a smaller base point
        shape.rotation = new Vector3(-90, 0, 0); // Point the cone upwards

        // Color over Lifetime: Fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(main.startColor.color, 0.0f), new GradientColorKey(Color.yellow, 0.5f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        // Size over Lifetime: Shrink
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, 0.0f);
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        renderer.sortingLayerName = "Umgebung_col Layer";
    }

    /// <summary>
    /// Spielt den Partikeleffekt ab.
    /// </summary>
    public void PlayEffect()
    {
        ps.Play();
    }
}