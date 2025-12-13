using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Um bestimmte Buffs hinzuzuf�gen, kann im Projektil GetComponent<Buff> gecalled werden, falls dies != null ist, kann der entsprechende Buff bei
//Kollision applied werden.
public class _projectile : MonoBehaviour
{
    private Vector3 _pDirection;

    private Quaternion _pRotation;

    public bool _pSpecialEffect = true;

    //Schön wäre eine Liste, bzw. ein Dict in welchem die Buffs liegen. Perfekter Weise ein Enum mit ScriptableObjects zum auswählen.
    public Buff buff;

    //Der Ursprung des Projektils um gegebenenfalls AP Values zur Bearbeitung des Schadens zu verwenden.
    //public GameObject projectileOrigin;

    [HideInInspector]
    public float _pDamage;

    public float _pSpeed;

    [Header("Arrow Scaling and Rotation")]
    [Tooltip("Wendet spezielle Rotation (-90° Z) und Skalierung (0.5x Y) für Pfeil-Projektile an")]
    public bool applyArrowTransform = false;

    [Header("Collision Behavior")]
    [Tooltip("Kann das Projektil nach einer Umgebungskollision noch den Spieler treffen?")]
    public bool hitAfterEnvCollision = false;
    
    [Tooltip("Ist das Projektil aktuell in der Luft? (wird automatisch verwaltet)")]
    [HideInInspector]
    public bool isInAir = true;
    
    private bool hasHitPlayer = false;
    private bool hasHitEnvironment = false;

    //public float _pYOffSet;

    public ParticleSystem _hitParticles;

    public GameObject _hitEnvParticles;

    private Rigidbody _pRbody;

    public IEntitie _origin { get; private set; }


    public enum Trajectory { FollowTarget, Direction, Falling, Curve}

    [SerializeField]
    private Trajectory trajectory;

    private Vector3 _curveStart;
    private Vector3 _curveMidPoint;
    private Vector3 _curveTarget;
    private float _curveTravelTime;
    private float _curveElapsedTime;
    private float _curveHeight = 3f;

    void Start()
    {
        _pRbody = GetComponent<Rigidbody>();
        
        // Projektil startet immer in der Luft
        isInAir = true;

        // Wende Arrow-Skalierung an falls aktiviert
        if (applyArrowTransform)
        {
            Vector3 scale = transform.localScale;
            scale.y *= 0.5f;
            transform.localScale = scale;
        }

        if(trajectory == Trajectory.Direction)
        {
            //_pDirection = new Vector2((PlayerManager.instance.player.transform.position.x - transform.position.x), (PlayerManager.instance.player.transform.position.z - transform.position.z));
            _pDirection = new Vector3(PlayerManager.instance.player.transform.position.x - transform.position.x, 0, PlayerManager.instance.player.transform.position.z - transform.position.z);
            
            // ✅ FIX: Setze Collision Detection auf Continuous Dynamic für schnelle Projektile
            if (_pRbody != null)
            {
                _pRbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Debug.Log($"[Projectile] {gameObject.name} - Direction Trajectory mit ContinuousDynamic Collision Detection");
            }
        }
        if (trajectory == Trajectory.FollowTarget)
        {
            print("Not implented yet");
        }
        if (trajectory == Trajectory.Falling)
        {
            print("Not implented yet");
        }
        if (trajectory == Trajectory.Curve)
        {
            // Curve-Trajectory wird durch SetCurveTarget() konfiguriert
            _curveElapsedTime = 0f;
        }

    }

    void Update()
    {
        if (trajectory == Trajectory.Direction)
        {
            ProjectileFlightByDirection();
        }
        if (trajectory == Trajectory.Curve)
        {
            ProjectileFlightByCurve();
        }
    }

    private void ProjectileFlightByDirection()
    {
        //print(_pDirection.normalized);
        _pRbody.linearVelocity = (_pDirection.normalized * _pSpeed);

        _pRotation = Quaternion.LookRotation(_pDirection);
        transform.localRotation = Quaternion.Lerp(transform.rotation, _pRotation, 1);
        
        // Wende Arrow-Rotation an (-90° Y und -90° Z falls aktiviert)
        if (applyArrowTransform)
        {
            transform.rotation = Quaternion.LookRotation(_pDirection) * Quaternion.Euler(0, -90, -90);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(_pDirection) * Quaternion.Euler(0, -90, 0);
        }
    }

    /// <summary>
    /// Konfiguriert die Bogenbahn zum Ziel mit einer quadratischen Bézier-Kurve
    /// </summary>
    public void SetCurveTarget(Vector3 targetPosition, float height = 3f)
    {
        _curveStart = transform.position;
        _curveTarget = targetPosition;
        _curveHeight = height;
        
        // Berechne Mittelpunkt (höher als Start und Ziel)
        _curveMidPoint = (_curveStart + _curveTarget) / 2f;
        _curveMidPoint.y += _curveHeight;
        
        // Berechne Reisezeit basierend auf Distanz und Geschwindigkeit
        _curveTravelTime = Vector3.Distance(_curveStart, _curveTarget) / _pSpeed;
        _curveElapsedTime = 0f;
    }

    /// <summary>
    /// Bewegt das Projektil entlang einer Bogenbahn (Bézier-Kurve)
    /// </summary>
    private void ProjectileFlightByCurve()
    {
        if (_curveTravelTime <= 0)
            return;

        _curveElapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_curveElapsedTime / _curveTravelTime);

        // Berechne Position auf der Bogenbahn
        Vector3 currentPosition = CalculateBezierPoint(_curveStart, _curveMidPoint, _curveTarget, progress);
        transform.position = currentPosition;

        // Rotiere Projektil in Bewegungsrichtung
        if (progress < 1f)
        {
            Vector3 nextPosition = CalculateBezierPoint(_curveStart, _curveMidPoint, _curveTarget, progress + 0.01f);
            Vector3 direction = (nextPosition - currentPosition).normalized;
            SetRotationByDirection(direction);
        }

        // Projektil hat Ziel erreicht
        if (progress >= 1f)
        {
            // Trigger Collision/Destruction
            OnCurveComplete();
        }
    }

    /// <summary>
    /// Berechnet einen Punkt auf einer quadratischen Bézier-Kurve
    /// </summary>
    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    /// <summary>
    /// Wird aufgerufen wenn die Bogenbahn das Ziel erreicht
    /// </summary>
    private void OnCurveComplete()
    {
        // Hier kann man auch einen Collision-Check machen oder das Projektil zerstören
        //Destroy(gameObject);
    }

    //Falls der Collider der Spieler ist
    private void OnTriggerEnter(Collider collider)
    {
        // Debug-Log für alle Kollisionen (aktiviert für Debugging)
        Debug.Log($"[Projectile {gameObject.name}] OnTriggerEnter mit: {collider.gameObject.name} | Tag: {collider.tag} | Trajectory: {trajectory} | isInAir: {isInAir}");
        
        // Ignoriere DirCollider
        if (collider.gameObject.name == "DirCollider")
        {
            Debug.Log("[Projectile] DirCollider ignoriert");
            return;
        }
        
        // Prüfe ob Collider oder Parent den Tag "Player" hat
        bool isPlayer = collider.CompareTag("Player") || (collider.transform.parent != null && collider.transform.parent.CompareTag("Player"));
        
        if (isPlayer)
        {
            // Wenn Projektil nicht mehr in der Luft ist und hitAfterEnvCollision = false, ignoriere Spielerkollision
            if (!isInAir && !hitAfterEnvCollision)
            {
                //Debug.Log($"[Projectile] Spieler-Kollision ignoriert - Projektil nicht in der Luft (isInAir={isInAir}, hitAfterEnvCollision={hitAfterEnvCollision})");
                return;
            }
            
            //Debug.Log($"[Projectile] Spieler-Treffer erkannt! isInAir={isInAir}, hasHitPlayer={hasHitPlayer}");
            
            // Wenn bereits Spieler getroffen wurde, ignoriere weitere Spielerkollisionen
            if (hasHitPlayer)
            {
                return;
            }
            
            hasHitPlayer = true;
            
            // Suche PlayerStats entweder am Collider selbst oder am Parent (falls Child-Collider getroffen)
            PlayerStats playerStats = collider.GetComponent<PlayerStats>();
            if (playerStats == null && collider.transform.parent != null)
            {
                playerStats = collider.transform.parent.GetComponent<PlayerStats>();
            }
            
            // Fallback: Nutze PlayerManager
            if (playerStats == null && PlayerManager.instance?.player != null)
            {
                playerStats = PlayerManager.instance.player.transform.GetComponent<PlayerStats>();
            }
            
            if (playerStats == null)
            {
                Debug.LogWarning($"[Projectile] PlayerStats nicht gefunden auf {collider.gameObject.name}!");
                DestroyProjectileAfterDelay();
                return;
            }
            
            //Debug.Log($"[Projectile] PlayerStats gefunden, füge {_pDamage} Schaden zu!");

            //Und falls das Projektil einen SpecialEffect besitzt
            if(_pSpecialEffect)
            {
                ApplySpecialEffect(playerStats);
                
                // Nur Partikel instantiieren falls zugewiesen
                if (_hitParticles != null)
                {
                    Instantiate(_hitParticles, PlayerManager.instance.player.transform.position, Quaternion.identity);
                }
            }

            // Prüfe ob Kritischer Treffer (3% Chance, ähnlich wie bei EnemyController.PerformAttack)
            bool isCrit = UnityEngine.Random.value < 0.03f;
            
            // Wende Schaden an
            playerStats.TakeDamage(_pDamage, isCrit);
            
            // Nur Partikel instantiieren falls zugewiesen
            if (_hitParticles != null)
            {
                Instantiate(_hitParticles, PlayerManager.instance.player.transform.position, Quaternion.identity);
            }

            // Spiele Projektil-Einschlag Sound ab
            PlayProjectileImpactSound();
            
            // Deaktiviere Physik und Rendering, aber zerstöre GameObject erst nach Delay
            DisableProjectileComponents();
            DestroyProjectileAfterDelay();
        }
    }

    // Für normale Collider (Umgebung, Boden)
    private void OnCollisionEnter(Collision collision)
    {
        // Verhindere mehrfache Umgebungskollisionen
        if (hasHitEnvironment)
        {
            return;
        }
        
        // Spiele Projektil-Einschlag Sound ab
        PlayProjectileImpactSound();
        
        hasHitEnvironment = true;
        isInAir = false; // Projektil ist nicht mehr in der Luft
        
        // Freeze alle Rigidbody Constraints um Wackeln zu verhindern
        if (_pRbody != null)
        {
            //_pRbody.isKinematic = true;
            _pRbody.constraints = RigidbodyConstraints.FreezeAll;
            _pRbody.linearVelocity = Vector3.zero;
            _pRbody.angularVelocity = Vector3.zero;
        }
        
        // Entferne Collider um weitere Kollisionen zu verhindern
        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider != null)
        {
            Destroy(projectileCollider);
        }
        
        // Deaktiviere Update-Logik
        this.enabled = false;
        
        DestroyProjectileAfterDelay();
    }

    public virtual void ApplySpecialEffect(IEntitie targetEntitie)
    {
        // Prüfe ob Buff zugewiesen ist
        if (buff == null)
        {
            Debug.LogWarning($"[Projectile] Kein Buff im Projektil {gameObject.name} zugewiesen! SpecialEffect wird übersprungen.");
            return;
        }
        
        // Erstelle BuffInstance mit ScriptableObject.CreateInstance (korrekter Weg für SO)
        BuffInstance buffInstance = ScriptableObject.CreateInstance<BuffInstance>();
        
        // Kopiere Daten vom Buff ScriptableObject
        buffInstance.buffName = buff.buffName;
        buffInstance.MyDuration = buff.MyDuration;
        buffInstance.tick = buff.tickIntervall;
        buffInstance.stackable = buff.stackable;
        buffInstance.icon = buff.icon;
        buffInstance.MyBaseDamage = buff.MyBaseDamage;
        buffInstance.particleEffect = buff.particleEffect;
        buffInstance.originalBuff = buff;
        buffInstance.MyDescription = buff.MyDescription;
        
        // Kopiere auch die Buff-Logik (wichtig für SlowDebuff!)
        buffInstance.CopyFrom(buff);
        
        if (buffInstance == null)
        {
            Debug.LogWarning($"[Projectile] BuffInstance konnte nicht erstellt werden für Buff '{buff.buffName}'!");
            return;
        }

        // Füge den Buff der Ziel-Entitie hinzu
        buffInstance.ApplyBuff(targetEntitie, _origin);
        
        Debug.Log($"[Projectile] Buff '{buff.buffName}' erfolgreich auf {targetEntitie.GetTransform().name} angewendet!");
    }

    public void SetOrigin(IEntitie origin)
    {
        _origin = origin;
    }

    /// <summary>
    /// Setzt das Trajectory des Projektils (für externe Steuerung)
    /// </summary>
    public void SetTrajectory(Trajectory newTrajectory)
    {
        trajectory = newTrajectory;
        
        // Wenn auf Direction gesetzt und noch keine Richtung gesetzt, berechne zum Spieler
        if (trajectory == Trajectory.Direction && _pDirection == Vector3.zero)
        {
            if (PlayerManager.instance != null && PlayerManager.instance.player != null)
            {
                _pDirection = new Vector3(
                    PlayerManager.instance.player.transform.position.x - transform.position.x, 
                    0, 
                    PlayerManager.instance.player.transform.position.z - transform.position.z
                );
            }
        }
        
        // Wenn auf Curve gesetzt, deaktiviere automatische Physik
        if (trajectory == Trajectory.Curve && _pRbody != null)
        {
            _pRbody.isKinematic = true;
        }
    }

    /// <summary>
    /// Setzt eine manuelle Richtung für Direction-Trajectory
    /// </summary>
    public void SetDirection(Vector3 direction)
    {
        _pDirection = direction;
    }

    /// <summary>
    /// Setzt die Rotation des Projektils basierend auf der Bewegungsrichtung
    /// Wendet automatisch die notwendige Korrektur an (z.B. -90° für Pfeil-Modelle)
    /// </summary>
    public void SetRotationByDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion rotation = Quaternion.LookRotation(direction);
        
        // Wende Arrow-Rotation an (-90° Y und -90° Z falls aktiviert)
        if (applyArrowTransform)
        {
            rotation *= Quaternion.Euler(0, -90, -90); // Korrektur für Pfeil-Modelle (Y- und Z-Achse)
        }
        else
        {
            rotation *= Quaternion.Euler(0, -90, 0); // Korrektur für Standard-Modelle (Y-Achse)
        }
        
        transform.rotation = rotation;
    }

    /// <summary>
    /// Spielt den passenden Einschlag-Sound für dieses Projektil ab.
    /// Sucht in Resources/Sounds/Effects/Projectile/<PrefabName>/ nach Sounds mit dem gleichen Namensprefix.
    /// </summary>
    private void PlayProjectileImpactSound()
    {
        if (AudioManager.instance == null)
            return;

        string prefabName = GetBasePrefabName();
        
        if (string.IsNullOrEmpty(prefabName))
            return;

        // Der AudioManager lädt alle Sounds rekursiv und gruppiert sie nach Clip-Namen
        // Da die Clips "Arrow-1", "Arrow-2", etc. heißen, ist der groupKey einfach "Arrow"
        AudioManager.instance.PlayEntitySound(prefabName, gameObject);
    }

    /// <summary>
    /// Ermittelt den Basis-Prefab-Namen ohne Suffixe wie "(Clone)"
    /// </summary>
    private string GetBasePrefabName()
    {
        string objectName = gameObject.name;
        
        // Entferne "(Clone)" falls vorhanden
        if (objectName.EndsWith("(Clone)"))
        {
            objectName = objectName.Replace("(Clone)", "").Trim();
        }
        
        return objectName;
    }

    /// <summary>
    /// Deaktiviert Rigidbody und SpriteRenderer nach Spielertreffer
    /// </summary>
    private void DisableProjectileComponents()
    {
        // Deaktiviere Rigidbody
        if (_pRbody != null)
        {
            Destroy(_pRbody);
        }
        
        // Deaktiviere SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Destroy(spriteRenderer);
        }
        
        // Deaktiviere auch MeshRenderer falls vorhanden
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Destroy(meshRenderer);
        }
        
        // Deaktiviere Collider um weitere Kollisionen zu verhindern
        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }
        
        // Deaktiviere Update-Logik
        this.enabled = false;
    }

    /// <summary>
    /// Zerstört das Projektil nach 5 Sekunden Verzögerung
    /// </summary>
    private void DestroyProjectileAfterDelay()
    {
        StartCoroutine(DestroyAfterDelay(5f));
    }

    /// <summary>
    /// Coroutine: Wartet die angegebene Zeit und zerstört dann das GameObject
    /// </summary>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
