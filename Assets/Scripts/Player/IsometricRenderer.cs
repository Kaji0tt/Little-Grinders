using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationState
{
    Idle,
    Walk,
    Attack,
    Cast,
    Hit,
    Die
}

//Update: 11.05 -> Diese Klasse sollte ein einheitliches System für alle Animationen bekommen.

public class IsometricRenderer : MonoBehaviour
{

    /// <summary>
    /// Player Section:
    /// These variables are only used for the animation of the player character.
    /// </summary>
    
    //8 directional, single created sprite sheet.
    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };
    public static readonly string[] runDirections = { "Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };

    //int to clalculate and safe the last direction of view.
    int lastDirection;

    //Der Animator, welcher für die Animation der waffe verantwortlich ist.
    public Animator weaponAnimator;

    //Das Transform des "Vaters" von WeaponAnimator - damit die Animation/Clips, welche Transform von weaponAnimator angreifen, unangetastet bleiben.
    public Transform weaponPivot;

    //used for casts, attacks etc.
    public bool isPerformingAction = false;

    //The animator the IsoRenderer refers to. Typically attached to the animated GameObject, e.g. the Enemy.
    private Animator myAnimator;

    private EnemyController myEnemyController;

    /// <summary>
    /// Movement Detection für automatische Idle/Walk Übergänge
    /// </summary>
    [Header("Movement Detection")]
    [Tooltip("Mindestgeschwindigkeit für Walk-Animation")]
    public float minMovementSpeed = 0.1f;

    /// <summary>
    /// Enemy Section:
    /// These variables are only used for the animation of the of Enemy characters.
    /// </summary>

    [Tooltip("Deaktiviert die automatische Animation für Intro-Mobs")]
    public bool turnOffIsometricRenderer = false;

    private AnimationState currentState = AnimationState.Idle;

    // Mapping von AnimationState zu möglichen Clipnamen
    private static readonly Dictionary<AnimationState, string[]> animationVariants = new Dictionary<AnimationState, string[]>()
    {
    { AnimationState.Idle,   new[] { "Idle" } },
    { AnimationState.Walk,   new[] { "Walk" } },
    { AnimationState.Attack, new[] { "Attack1", "Attack2" } }, //can overwrite
    { AnimationState.Cast,   new[] { "Casting" } },
    { AnimationState.Hit,    new[] { "Hit1", "Hit2" } }, //can overwrite
    { AnimationState.Die,    new[] { "Die1", "Die2" } }, //can overwrite, can not get overwritten
    };

    //Set the Spritesheet to auto Animate this enemy.
    public Sprite spriteSheet;

    public bool mirrorSpritesheet = false;




    [HideInInspector] public RuntimeAnimatorController generatedAnimator;

    private void Reset()
    {
        // Optional: Default Setup
        spriteSheet = null;
    }

    private void Awake()
    {
        //cache the animator component
        myAnimator = GetComponent<Animator>();
        myEnemyController = GetComponentInParent<EnemyController>();
        isPerformingAction = false;

        //RuntimController setzen!
        if (weaponAnimator != null)
        {
            weaponOverrideController = new AnimatorOverrideController(weaponAnimator.runtimeAnimatorController);
            weaponAnimator.runtimeAnimatorController = weaponOverrideController;
        }

        // Event-Listener für Enemy-Animationen registrieren (außer für Intro-Mobs)
        if (myEnemyController != null && !turnOffIsometricRenderer)
        {
            if (GameEvents.Instance != null)
            {
                GameEvents.Instance.OnEnemyStartAttack += OnEnemyStartAttack;
                GameEvents.Instance.OnEnemyAttackHit += OnEnemyAttackHit;
                GameEvents.Instance.OnEnemyEndAttack += OnEnemyEndAttack;
                
                // Ability/Cast Events
                GameEvents.Instance.OnEnemyStartCast += OnEnemyStartCast;
                GameEvents.Instance.OnEnemyCastComplete += OnEnemyCastComplete;
                
                // Movement Animation Events
                GameEvents.Instance.OnEnemyStartIdle += OnEnemyStartIdle;
                GameEvents.Instance.OnEnemyStartWalk += OnEnemyStartWalk;
                GameEvents.Instance.OnEnemyStartHit += OnEnemyStartHit;
            }
        }
    }

    private void Update()
    {
        // Movement Detection nur für Enemies (außer Intro-Mobs)
        if (myEnemyController != null && !isPerformingAction && !turnOffIsometricRenderer)
        {
            DetectMovementAndUpdateAnimation();
        }
    }

    /// <summary>
    /// Erkennt Bewegung und wechselt automatisch zwischen Idle und Walk Animationen
    /// </summary>
    private void DetectMovementAndUpdateAnimation()
    {
        // Wenn eine Action ausgeführt wird, keine automatischen Animationswechsel
            if (isPerformingAction || myEnemyController.mobStats.isDead)
            return;

        // Prüfe NavMeshAgent Geschwindigkeit
        bool isCurrentlyMoving = myEnemyController.myNavMeshAgent != null && 
                                myEnemyController.myNavMeshAgent.velocity.magnitude > minMovementSpeed;

        // Bestimme gewünschten Animationszustand basierend auf Bewegung
        AnimationState desiredState = isCurrentlyMoving ? AnimationState.Walk : AnimationState.Idle;

        // Wechsle nur wenn sich der gewünschte Zustand vom aktuellen unterscheidet
        if (currentState != desiredState)
        {
            //Debug.Log($"[IsometricRenderer] Switching from {currentState} to {desiredState}");
            Play(desiredState);
        }
    }

    public void Play(AnimationState state)
    {

        currentState = state;

        if (!animationVariants.TryGetValue(state, out string[] variants))
            return;

        string chosenAnim = variants[Random.Range(0, variants.Length)];
        myAnimator.Play(chosenAnim);

        if (state == AnimationState.Attack || state == AnimationState.Cast || state == AnimationState.Die)
        {
            isPerformingAction = true;
            StartCoroutine(ResetActionAfterAnimation());
        }
    }

    /// <summary>
    /// Spielt eine Animation mit angepasster Geschwindigkeit basierend auf gewünschter Dauer ab
    /// </summary>
    /// <param name="state">Der Animationszustand</param>
    /// <param name="desiredDuration">Gewünschte Dauer der Animation in Sekunden</param>
    public void PlayAttackWithSpeed(AnimationState state, float desiredDuration)
    {
        currentState = state;

        if (!animationVariants.TryGetValue(state, out string[] variants))
        {
            return;
        }

        string chosenAnim = variants[Random.Range(0, variants.Length)];

        // Animation starten
        myAnimator.Play(chosenAnim);
        
        // Warte einen Frame, um die korrekte Clip-Länge zu erhalten
        StartCoroutine(AdjustAnimationSpeed(chosenAnim, desiredDuration));

        if (state == AnimationState.Attack || state == AnimationState.Cast || state == AnimationState.Die)
        {
            isPerformingAction = true;

            StartCoroutine(ResetEnemyActionAfterDuration(desiredDuration));
        }
    }

    private IEnumerator AdjustAnimationSpeed(string animName, float desiredDuration)
    {
        yield return null; // Warte einen Frame
        
        AnimatorStateInfo stateInfo = myAnimator.GetCurrentAnimatorStateInfo(0);
        float originalLength = stateInfo.length;
        
        // Berechne benötigte Geschwindigkeit
        float requiredSpeed = originalLength / desiredDuration;
        myAnimator.speed = requiredSpeed;
        
        //Debug.Log($"[IsometricRenderer.AdjustAnimationSpeed] 🎬 {animName}: Original={originalLength:F2}s, Desired={desiredDuration:F2}s, Speed={requiredSpeed:F2}x");
    }

    private IEnumerator ResetEnemyActionAfterDuration(float duration)
    {
        //Debug.Log($"[IsometricRenderer.ResetEnemyActionAfterDuration] ⏱️ Starte Countdown für {duration:F2}s");
        
        yield return new WaitForSeconds(duration);
        
        //Debug.Log($"[IsometricRenderer.ResetEnemyActionAfterDuration] 🏁 Countdown vorbei! Setze Geschwindigkeit zurück und isPerformingAction = false");
        
        // Geschwindigkeit zurücksetzen
        myAnimator.speed = 1f;
        isPerformingAction = false;
        
        //Debug.Log($"[IsometricRenderer.ResetEnemyActionAfterDuration] ✅ Reset abgeschlossen");
    }

    #region Event Handlers
    /// <summary>
    /// Event-Handler: Wenn dieser Enemy einen Angriff startet
    /// </summary>
    private void OnEnemyStartAttack(EnemyController enemy, float attackDuration)
    {
        // Prüfe ob dieses Event für diesen Enemy gedacht ist
        if (enemy != myEnemyController) 
        {
            return;
        }
        
        // Spiele Attack-Sound ab
        if (AudioManager.instance != null)
        {
            string soundName = enemy.GetBasePrefabName() + "_AttackStart";
            AudioManager.instance.PlayEntitySound(soundName, enemy.gameObject);
        }
        
        // Spiele Attack-Animation mit der richtigen Geschwindigkeit ab
        PlayAttackWithSpeed(AnimationState.Attack, attackDuration);
    }
    
    /// <summary>
    /// Event-Handler: Wenn der Angriff den Impact-Punkt erreicht
    /// </summary>
    private void OnEnemyAttackHit(EnemyController enemy)
    {
        // Prüfe ob dieses Event für diesen Enemy gedacht ist
        if (enemy != myEnemyController) return;
        
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} Angriff-Impact!")
        
        // Hier könnten weitere Impact-Effects eingefügt werden (z.B. Partikelsysteme)
        if (AudioManager.instance != null)
        {
            string soundName = enemy.GetBasePrefabName() + "_Attack";
            AudioManager.instance.PlayEntitySound(soundName, enemy.gameObject);
        }
    }
    
    /// <summary>
    /// Event-Handler: Wenn der Angriff beendet ist
    /// </summary>
    private void OnEnemyEndAttack(EnemyController enemy)
    {
        // Prüfe ob dieses Event für diesen Enemy gedacht ist
        if (enemy != myEnemyController) return;
        
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} Angriff beendet");
        
        // Animation ist bereits durch PlayAttackWithSpeed gehandhabt
        // Hier könnten zusätzliche End-Effects eingefügt werden
    }
    
    /// <summary>
    /// Event-Handler: Wenn Enemy zu Idle wechselt (jetzt optional, da Movement-Detection aktiv)
    /// </summary>
    private void OnEnemyStartIdle(EnemyController enemy)
    {
        if (enemy != myEnemyController) return;
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} -> Idle Animation (Event)");
        // Movement-Detection übernimmt jetzt die Steuerung automatisch
        // Play(AnimationState.Idle);
    }
    
    /// <summary>
    /// Event-Handler: Wenn Enemy zu Walk wechselt (jetzt optional, da Movement-Detection aktiv)
    /// </summary>
    private void OnEnemyStartWalk(EnemyController enemy)
    {
        if (enemy != myEnemyController) return;
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} -> Walk Animation (Event)");
        // Movement-Detection übernimmt jetzt die Steuerung automatisch
        // Play(AnimationState.Walk);
    }
    
    /// <summary>
    /// Event-Handler: Wenn Enemy zu Hit wechselt
    /// </summary>
    private void OnEnemyStartHit(EnemyController enemy)
    {
        if (enemy != myEnemyController) return;
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} -> Hit Animation");
        Play(AnimationState.Hit);
    }
    
    /// <summary>
    /// Event-Handler: Wenn Enemy einen Cast startet
    /// </summary>
    private void OnEnemyStartCast(EnemyController enemy, float castDuration)
    {
        // Nur für diesen Enemy reagieren
        if (enemy != myEnemyController) return;
        
        // Hole Animation-Typ von der aktuell aktiven Ability
        AbilityAnimationType animType = AbilityAnimationType.Casting; // Default
        IAbilityBehavior readyAbility = enemy.GetReadyAbility();
        if (readyAbility != null)
        {
            animType = readyAbility.GetAnimationType();
        }
        
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} -> {animType} Animation (Duration: {castDuration}s)");
        
        // Sound abspielen (optional)
        if (AudioManager.instance != null)
        {
            string soundName = myEnemyController.GetBasePrefabName() + "_Cast";
            AudioManager.instance.PlayEntitySound(soundName, myEnemyController.gameObject);
        }
        
        // Spiele die entsprechende Animation basierend auf animType
        if (animType == AbilityAnimationType.None)
        {
            // Keine Animation - nur Timing
            isPerformingAction = true;
            return;
        }
        
        // Konvertiere Enum zu Animation-Clip-Namen und spiele ab
        string animationName = GetAnimationNameFromType(animType);
        if (!string.IsNullOrEmpty(animationName))
        {
            if (castDuration > 0f)
            {
                // Spiele Animation mit angepasster Geschwindigkeit
                PlayAbilityAnimationWithSpeed(animationName, castDuration);
            }
            else
            {
                // Instant-Cast: Normale Geschwindigkeit
                PlayAbilityAnimation(animationName);
            }
        }
    }
    
    /// <summary>
    /// Event-Handler: Wenn Cast abgeschlossen ist
    /// </summary>
    private void OnEnemyCastComplete(EnemyController enemy)
    {
        // Nur für diesen Enemy reagieren
        if (enemy != myEnemyController) return;
        
        //Debug.Log($"[IsometricRenderer] Enemy {enemy.name} -> Cast Complete");
        
        // Action-Flag zurücksetzen
        isPerformingAction = false;
    }
    
    /// <summary>
    /// Konvertiert AbilityAnimationType Enum zu Animation-Clip-Namen
    /// </summary>
    private string GetAnimationNameFromType(AbilityAnimationType animType)
    {
        switch (animType)
        {
            case AbilityAnimationType.Idle:     return "Idle";
            case AbilityAnimationType.Walk:     return "Walk";
            case AbilityAnimationType.Attack1:  return "Attack1";
            case AbilityAnimationType.Attack2:  return "Attack2";
            case AbilityAnimationType.Casting:  return "Casting";
            case AbilityAnimationType.Die1:     return "Die1";
            case AbilityAnimationType.Die2:     return "Die2";
            case AbilityAnimationType.Hit1:     return "Hit1";
            case AbilityAnimationType.Hit2:     return "Hit2";
            case AbilityAnimationType.Open1:    return "Open1";
            case AbilityAnimationType.Open2:    return "Open2";
            case AbilityAnimationType.None:
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Spielt eine Ability-Animation mit normaler Geschwindigkeit
    /// </summary>
    private void PlayAbilityAnimation(string animationName)
    {
        if (myAnimator == null || string.IsNullOrEmpty(animationName))
            return;
        
        myAnimator.Play(animationName);
        isPerformingAction = true;
        
        StartCoroutine(ResetActionAfterAnimation());
    }
    
    /// <summary>
    /// Spielt eine Ability-Animation mit angepasster Geschwindigkeit
    /// </summary>
    private void PlayAbilityAnimationWithSpeed(string animationName, float desiredDuration)
    {
        if (myAnimator == null || string.IsNullOrEmpty(animationName))
            return;
        
        myAnimator.Play(animationName);
        isPerformingAction = true;
        
        StartCoroutine(AdjustAnimationSpeed(animationName, desiredDuration));
        StartCoroutine(ResetEnemyActionAfterDuration(desiredDuration));
    }
    
    private void OnDestroy()
    {
        // Event-Listener beim Zerstören des Objekts entfernen (nur wenn sie registriert wurden)
        if (GameEvents.Instance != null && myEnemyController != null && !turnOffIsometricRenderer)
        {
            GameEvents.Instance.OnEnemyStartAttack -= OnEnemyStartAttack;
            GameEvents.Instance.OnEnemyAttackHit -= OnEnemyAttackHit;
            GameEvents.Instance.OnEnemyEndAttack -= OnEnemyEndAttack;
            
            // Ability/Cast Events
            GameEvents.Instance.OnEnemyStartCast -= OnEnemyStartCast;
            GameEvents.Instance.OnEnemyCastComplete -= OnEnemyCastComplete;
            
            // Movement Events
            GameEvents.Instance.OnEnemyStartIdle -= OnEnemyStartIdle;
            GameEvents.Instance.OnEnemyStartWalk -= OnEnemyStartWalk;
            GameEvents.Instance.OnEnemyStartHit -= OnEnemyStartHit;
        }
    }
    #endregion

    public float GetCurrentAnimationLength()
    {
        AnimatorStateInfo stateInfo = myAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length;
    }

    private IEnumerator ResetActionAfterAnimation()
    {
        yield return null; // Ein Frame warten, um den neuen State korrekt zu erfassen
        
        float animLength = GetCurrentAnimationLength();

        yield return new WaitForSeconds(animLength);

        isPerformingAction = false;

    }

    public void ToggleActionState(bool active)
    {
        isPerformingAction = active;
    }

    /// <summary>
    /// Spiegelt das GameObject basierend auf der Blickrichtung (nur X-Achse).
    /// </summary>
    /// <param name="direction">Die Zielrichtung (z.B. Richtung zum Spieler)</param>
    
    private Vector2 lastNonZeroDirection = Vector2.right;

    /// <summary>
    /// Spiegelt das GameObject basierend auf der Blickrichtung zum Spieler (nur X-Achse).
    /// Nur für NPCs/Enemies gedacht!
    /// </summary>
    public void SetFacingDirection()
    {
        if (myEnemyController == null || myEnemyController.Player == null)
            return;

        // Berechne Richtung vom Enemy zum Spieler
        Vector3 directionToPlayer = myEnemyController.Player.position - transform.position;
        Vector2 direction = new Vector2(directionToPlayer.x, directionToPlayer.z);

        if (direction.sqrMagnitude > 0.01f)
            lastNonZeroDirection = direction;

        if (lastNonZeroDirection.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);  // Gespiegelt (links)
        else if (lastNonZeroDirection.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);   // Normal (rechts)
    }

    public void SetPlayerDirection(Vector2 direction){


        string[] directionArray; //= null;

        if (direction.magnitude < .2f)
        { 
            directionArray = staticDirections;

            lastDirection = DirectionToIndex(direction, 8);
        }        

        else
        {
            directionArray = runDirections;

            lastDirection = DirectionToIndex(direction, 8);

        }


        myAnimator.Play(directionArray[lastDirection]);
    }








    /// <summary>
    /// Weapon Animation
    /// </summary>
    /// 
    #region Weapon Animation
    private AnimatorOverrideController weaponOverrideController;

    private bool isCurrentlyIdle = false;

        //Das WaffenObjekt, auf welchem der entsprechende Animation-Controller liegt, soll die Idle Animation darstellen.
    public void AnimateIdleWeapon()
    {
        if (weaponAnimator == null)
            return;

        weaponAnimator.enabled = true;
        weaponAnimator.SetBool("isAttacking", false);
        RotateWeaponToDirection();

        if (!weaponAnimator.GetBool("isAttacking") && !isPerformingAction)
        {
            if (!isCurrentlyIdle)
            {
                weaponAnimator.Play("Idle");
                isCurrentlyIdle = true;
            }
        }
        else
        {
            isCurrentlyIdle = false;
        }
    }

    public void PlayWeaponAttack(AnimationClip clip, Animator weaponAttackAnimator)
    {
        if (clip == null || weaponAttackAnimator == null)
        {
            Debug.LogWarning("Kein Clip oder Animator übergeben!");
            return;
        }

        // Richtung berechnen & Waffe drehen
        RotateWeaponToDirection();


        // 3. Override the correct clip BEFORE triggering the animation
        //Debug.Log("Set Clip to: " + clip.name + " and setting it to " + weaponAttackAnimator.gameObject.name + ". \n " + weaponOverrideController);
        weaponOverrideController["Placeholder"] = clip;

        // Animation abspielen
        weaponAttackAnimator.ResetTrigger("AttackTrigger");
        weaponAttackAnimator.SetTrigger("AttackTrigger");
        weaponAttackAnimator.SetBool("isAttacking", true); // optional, je nach Animator Setup

        isPerformingAction = true;
        StartCoroutine(ResetActionAfterAnimation());
    }

    //Spielt Animation mit spezifischer Dauer ab
    public void PlayWeaponAttackWithDuration(AnimationClip clip, float duration)
    {
        if (clip == null || weaponAnimator == null)
        {
            Debug.LogWarning("Kein Clip oder Animator übergeben!");
            return;
        }

        // Richtung berechnen & Waffe drehen
        RotateWeaponToDirection();

        // Animation-Speed basierend auf gewünschter Dauer berechnen
        float originalClipLength = clip.length;
        float requiredSpeed = originalClipLength / duration;
        
        // Speed setzen
        weaponAnimator.speed = requiredSpeed;

        // Override the correct clip BEFORE triggering the animation
        weaponOverrideController["Placeholder"] = clip;

        // Animation abspielen
        weaponAnimator.Play("Attack", 0, 0f);
        weaponAnimator.SetBool("isAttacking", true);

        isPerformingAction = true;
        StartCoroutine(ResetActionAfterDuration(duration));
    }

    // NEUE Coroutine: Reset nach spezifischer Dauer
    private IEnumerator ResetActionAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (weaponAnimator != null)
        {
            //weaponAnimator.SetBool("isAttacking", false);
            weaponAnimator.speed = 1f; // Speed zurücksetzen
            isCurrentlyIdle = false; // Erlaube Idle-Animation wieder
        }
        
        isPerformingAction = false;
    }

    private void RotateWeaponToDirection()
    {
        Vector3 worldDirection = DirectionCollider.instance.dirVector;
        //Debug.Log($"[WeaponRotation] dirVector: {worldDirection}");

        if (worldDirection == Vector3.zero)
            return;

        worldDirection.y = 0;
        Vector3 lookDir = worldDirection.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDir);

        SpriteRenderer weaponSprite = weaponAnimator?.GetComponent<SpriteRenderer>();
        if (weaponSprite != null)
        {
            // Optional: Basiswert merken
            //int baseOrder = 5000;
            //Debug.Log(GetComponent<MobsCamScript>());

            // Wenn Blickrichtung "nach oben", dann weiter hinten rendern
            if (worldDirection.z > 0)
                weaponAnimator.GetComponent<MobsCamScript>().ReduceSpriteStartingPoint();
            else
                weaponAnimator.GetComponent<MobsCamScript>().ResetSpriteStartingPoint();
        }

        weaponPivot.rotation = lookRotation;

        Debug.DrawRay(weaponPivot.position, worldDirection.normalized * 2, Color.red);
    }

    #endregion

    //helper functions


    #region Helper Functions
    //why would this be static? cant remember, lets make it nonstatic, so i might acces it from enemy-controller scripts.
    public int DirectionToIndex(Vector2 dir, int sliceCount){
        //get the normalized direction
        Vector2 normDir = dir.normalized;
        //calculate how many degrees one slice is
        float step = 360f / sliceCount;
        //calculate how many degress half a slice is.
        //we need this to offset the pie, so that the North (UP) slice is aligned in the center
        float halfstep = step / 2;
        //get the angle from -180 to 180 of the direction vector relative to the Up vector.
        //this will return the angle between dir and North.
        float angle = Vector2.SignedAngle(Vector2.up, normDir);
        //add the halfslice offset
        //angle += halfstep;
        //if angle is negative, then let's make it positive by adding 360 to wrap it around.
        if (angle < 0){
            angle += 360;
        }
        //calculate the amount of steps required to reach this angle
        float stepCount = angle / step;
        //round it, and we have the answer!
        return Mathf.FloorToInt(stepCount);
    }

    //this function converts a string array to a int (animator hash) array.
    public static int[] AnimatorStringArrayToHashArray(string[] animationArray)
    {
        //allocate the same array length for our hash array
        int[] hashArray = new int[animationArray.Length];
        //loop through the string array
        for (int i = 0; i < animationArray.Length; i++)
        {
            //do the hash and save it to our hash array
            hashArray[i] = Animator.StringToHash(animationArray[i]);
        }
        //we're done!
        return hashArray;
    }

    #endregion
}
