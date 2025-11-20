using UnityEngine;
using System.Collections;

public class RangedArcAttack : AttackBehavior
{
    [Header("Ranged Arc Attack Behavior")]
    [Tooltip("Projektil-Prefab das abgefeuert werden soll")]
    public GameObject projectilePrefab;
    
    [Tooltip("Transform von dem aus das Projektil abgefeuert wird (z.B. Waffenspitze)")]
    public Transform projectileSpawnPoint;
    
    [Tooltip("Höhe der Bogenbahn (je höher, desto ausgeprägter der Bogen)")]
    public float arcHeight = 3f;
    
    [Tooltip("Geschwindigkeit des Projektils (überschreibt Prefab-Einstellung)")]
    public float projectileSpeed = 8f;
    
    [Tooltip("Zeitpunkt beim Abschuss des Projektils als Prozentsatz der Animation (0-1, z.B. 0.3 = 30%)")]
    public float projectileFireTiming = 0.3f;
    
    //[Tooltip("Animationsname für Projektil 1 (wird zufällig ausgewählt)")]
    //private string projectileAnimation1 = "Open1";
    
    //[Tooltip("Animationsname für Projektil 2 (wird zufällig ausgewählt)")]
    //public string projectileAnimation2 = "Open2";
    
    private float timer;
    private float attackCooldown;
    private bool isAttacking = false;
    private Animator animator;
    private string currentProjectileAnimation;
    
    public override void Enter(EnemyController controller)
    {
        base.Enter(controller);
        
        // Berechne Angriffs-Cooldown basierend auf AttackSpeed (1 / AttackSpeed = Sekunden pro Angriff)
        attackCooldown = 1f / controller.mobStats.AttackSpeed.Value;
        timer = 0f; // Starte sofort mit Angriff
        isAttacking = false;
        
        // Hole Animator-Komponente
        animator = controller.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[RangedArcAttack] Kein Animator auf {controller.gameObject.name} gefunden!");
        }
    }

    /// <summary>
    /// UpdateAttack wird von der Base-Klasse aufgerufen
    /// FacingDirection wird automatisch in OnUpdateAttack gesteuert
    /// </summary>
    protected override void UpdateAttack()
    {
        timer -= Time.deltaTime;
        // Angriff ausführen wenn bereit
        if (timer <= 0 && !isAttacking)
        {
            isAttacking = true;  // ← NEU: Setze sofort auf true, bevor Coroutine startet!
            StartCoroutine(ExecuteRangedAttack(controller));
        }
    }

    /// <summary>
    /// Führt den Fernkampf-Angriff mit Projektil aus
    /// </summary>
    private IEnumerator ExecuteRangedAttack(EnemyController controller)
    {
        isAttacking = true;
        
        // Berechne die Animationsdauer basierend auf AttackSpeed
        float attackDuration = 1f / controller.mobStats.AttackSpeed.Value;
        float projectileFireDelay = attackDuration * projectileFireTiming; // Nutze den öffentlichen Parameter
        
        GameEvents.Instance?.EnemyStartAttack(controller, attackDuration);
        
        // Speichere gewählte Projektil-Animation für später
        //currentProjectileAnimation = selectedProjectileAnimation;
        
        // Warte bis zum Projektil-Abschuss (60% der Animation)
        yield return new WaitForSeconds(projectileFireDelay);
        
        // Feuere Projektil ab
        FireArcProjectile(controller);
        
        // Warte bis zum Ende der Animation
        yield return new WaitForSeconds(attackDuration - projectileFireDelay);
        
        // Event: Angriff beendet
        GameEvents.Instance?.EnemyEndAttack(controller);
        
        isAttacking = false;
        // Setze Timer für nächsten Angriff
        timer = attackCooldown;

    }
    
    /// <summary>
    /// Feuert ein Projektil mit Bogenbahn auf den Spieler ab
    /// </summary>
    private void FireArcProjectile(EnemyController controller)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[RangedArcAttack] Kein Projektil-Prefab zugewiesen!");
            return;
        }
        
        // Bestimme Spawn-Position
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : controller.transform.position + Vector3.up * 1.5f;
        
        // Erstelle Projektil
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        _projectile projectileScript = projectile.GetComponent<_projectile>();
        
        if (projectileScript == null)
        {
            Debug.LogError("[RangedArcAttack] Projektil-Prefab hat keine _projectile Komponente!");
            Destroy(projectile);
            return;
        }
        
        // Konfiguriere Projektil
        projectileScript._pDamage = controller.mobStats.AttackPower.Value;
        projectileScript._pSpeed = projectileSpeed;
        projectileScript.SetTrajectory(_projectile.Trajectory.Curve);
        projectileScript.SetOrigin(controller);
        
        // Starte Bogenbahn zum Spieler
        Vector3 targetPosition = PlayerManager.instance.player.transform.position;
        projectileScript.SetCurveTarget(targetPosition, arcHeight);
        
        // Kopiere AnimationController und spiele Projektil-Animation ab
        SetupProjectileAnimation(projectile, controller);
    }
    
    /// <summary>
    /// Kopiert den AnimationController vom Enemy zum Projektil und spielt die gewählte Animation ab
    /// </summary>
    private void SetupProjectileAnimation(GameObject projectile, EnemyController controller)
    {
        if (animator == null || string.IsNullOrEmpty(currentProjectileAnimation))
        {
            Debug.LogWarning("[RangedArcAttack] Kein Animator oder Projektil-Animation verfügbar");
            return;
        }
        
        // Hole oder erstelle Animator auf dem Projektil
        Animator projectileAnimator = projectile.GetComponent<Animator>();
        if (projectileAnimator == null)
        {
            projectileAnimator = projectile.AddComponent<Animator>();
        }
        
        // Kopiere den AnimatorController vom Enemy
        projectileAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
        
        // Spiele die gewählte Projektil-Animation ab
        projectileAnimator.Play(currentProjectileAnimation);
    }

    public override void Exit(EnemyController controller)
    {
        isAttacking = false;

            // Optional: Stoppe laufende Coroutines
        StopAllCoroutines();
    }

    public override bool IsAttackReady(EnemyController controller)
    {
        // Simpel: Wenn Timer abgelaufen und nicht gerade angreifend
        bool ready = timer <= 0 && controller.IsPlayerInAttackRange() && !isAttacking;
        
        return ready;
    }
}