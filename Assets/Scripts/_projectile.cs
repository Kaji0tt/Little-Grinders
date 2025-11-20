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

    [Header("Collision Behavior")]
    [Tooltip("Kann das Projektil nach einer Umgebungskollision noch den Spieler treffen?")]
    public bool hitAfterEnvCollision = false;
    
    public bool hasHitEnvironment = false;
    private bool hasHitPlayer = false;

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

        if(trajectory == Trajectory.Direction)
        {
            //_pDirection = new Vector2((PlayerManager.instance.player.transform.position.x - transform.position.x), (PlayerManager.instance.player.transform.position.z - transform.position.z));
            _pDirection = new Vector3(PlayerManager.instance.player.transform.position.x - transform.position.x, 0, PlayerManager.instance.player.transform.position.z - transform.position.z);
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
        transform.Rotate(new Vector3(-90, 0, 0));
        
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
        // Ignoriere DirCollider
        if (collider.gameObject.name == "DirCollider")
        {
            return;
        }
        
        if (collider.tag == "Player")
        {
            // Wenn bereits Umgebung getroffen wurde und hitAfterEnvCollision = false, ignoriere Spielerkollision
            if (hasHitEnvironment && !hitAfterEnvCollision)
            {
                return;
            }
            
            // Wenn bereits Spieler getroffen wurde, ignoriere weitere Spielerkollisionen
            if (hasHitPlayer)
            {
                return;
            }
            
            hasHitPlayer = true;
            
            PlayerStats playerStats = PlayerManager.instance.player.transform.GetComponent<PlayerStats>();
            
            if (playerStats == null)
            {
                DestroyProjectileAfterDelay();
                return;
            }

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
        //Erschaffe eine Kopie des angegebenen Buffs
        BuffInstance buffInstance = BuffDatabase.instance.GetInstance(buff.buffName);

        //Und f�ge diese der Ziel-Entitie des Projektils hinzu und vermittel ?Der Ziel-Entitie? die Informationen des Ursprungs.
        //->Die Ziel-Entitie muss nicht wissen, wo der Urpsrung des Buffs liegt, lediglich der Buff muss dies wissen.
        buffInstance.ApplyBuff(targetEntitie, _origin);

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
        
        // Wenn auf Curve gesetzt, deaktiviere automatische Physik
        if (trajectory == Trajectory.Curve && _pRbody != null)
        {
            _pRbody.isKinematic = true;
        }
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
        rotation *= Quaternion.Euler(0, -90, 0); // Korrektur für Modell-Orientierung (Y-Achse)
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
