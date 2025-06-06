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
    //Arrays for Player
    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };
    public static readonly string[] runDirections = { "Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };

    //Array created for Weapon Animation, once upon a time i didnt know about programming & logical structure.
    public static readonly string[] weaponSwing = { "Attack_N", "Attack_NW", "Attack_W", "Attack_SW", "Attack_S", "Attack_SE", "Attack_E", "Attack_NE" };

    //Root Animator (Idle Animations)
    public Animator weaponRootAnimator;

    //Parent Animator (liegt im Character als Parent gepsiechert, animiert die Waffenbewegung bei Attacks.)
    public Animator weaponAttackAnimator;

    //int to clalculate and safe the last direction of view.
    int lastDirection;

    int lastWeaponDirection;

    //private EnemyController myController;

    //The animator the IsoRenderer refers to. Typically attached to the animated GameObject, e.g. the Enemy.
    Animator animator;

    private AnimationState currentState = AnimationState.Idle;


    public bool isPerformingAction = false;

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


    private static readonly Dictionary<AnimationState, int> animationPriority = new Dictionary<AnimationState, int>()
    {
        { AnimationState.Idle, 0 },
        { AnimationState.Walk, 0 },
        { AnimationState.Cast, 1 },
        { AnimationState.Hit, 2 },
        { AnimationState.Attack, 2 },
        { AnimationState.Die, 10 } // Höchste Priorität → ununterbrechbar
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
        animator = GetComponent<Animator>();
        isPerformingAction = false;

        //myController = GetComponent<EnemyController>();

    }
    public void Play(AnimationState state)
    {
        currentState = state;

        if (!animationVariants.TryGetValue(state, out string[] variants))
            return;

        string chosenAnim = variants[Random.Range(0, variants.Length)];
        animator.Play(chosenAnim);

        if (state == AnimationState.Attack || state == AnimationState.Cast || state == AnimationState.Die)
        {
            isPerformingAction = true;
            StartCoroutine(ResetActionAfterAnimation());
        }
    }

    public float GetCurrentAnimationLength()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
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


        animator.Play(directionArray[lastDirection]);
    }

    
    //Das WaffenObjekt, auf welchem der entsprechende Animation-Controller liegt, soll die Idle Animation darstellen.
    public void AnimateIdleWeapon(Vector2 direction)
    {
        string[] directionArray; //= null;

        //Animator parentAnimator = GetComponentInParent<Animator>();
        //Debug.Log(parentAnimator);
        //Falls die Combat-Stance des Charakters im Animation-Controller ist #IsometricPlayer.Attack(), führe folgende Animationen aus.
        /*
        if (charCombat.weaponAttackAnimator.GetBool("isAttacking") == true && isPerformingAction == false)
        {

            //Wähle entsprechende AnimationsArray aus
            directionArray = weaponSwing;

            //Berechne die letzte Blickrichtung in Abhängigkeit vom DirectionCollider (direction).
            lastWeaponDirection = DirectionToIndex(direction, 8);
            //print(lastWeaponDirection); - funktioniert.
            charCombat.weaponRootAnimator.SetTrigger(directionArray[lastWeaponDirection]);

        }
        */

        //Falls der Character nicht am Angreifen ist #IsometricPlayer.Attack(), führe die Standrad-Waffenanimation aus.
        if (weaponAttackAnimator.GetBool("isAttacking") == false)
        {
            weaponRootAnimator.enabled = true;

            //Wähle entsprechende AnimationsArray aus
            directionArray = runDirections;

            //Spiele die Animation ab.
            weaponRootAnimator.Play(directionArray[lastDirection]);

        }
        else
            weaponRootAnimator.enabled = true;
    }



    /// <summary>
    /// Weapon Animation
    /// </summary>

    private AnimatorOverrideController weaponOverrideController;
    public void PlayWeaponAttack(AnimationClip clip, Animator weaponAttackAnimator)
    {
        if (clip == null)
        {
            Debug.LogWarning("Kein Clip übergeben!");
            return;
        }

        Debug.Log("PlayWeaponAttack was called.");

        // 1. Rotation setzen
        Vector3 dir = DirectionCollider.instance.dirVector - PlayerManager.instance.player.transform.position;
        RotateWeaponTowardDirection(dir);

        // 2. Ensure override controller exists
        if (weaponOverrideController == null || weaponAttackAnimator.runtimeAnimatorController != weaponOverrideController)
        {
            weaponOverrideController = new AnimatorOverrideController(weaponAttackAnimator.runtimeAnimatorController);
            weaponAttackAnimator.runtimeAnimatorController = weaponOverrideController;
        }

        // 3. Override the correct clip BEFORE triggering the animation
        Debug.Log("Set Clip to: " + clip.name + " and setting it to " + weaponAttackAnimator.gameObject.name + ". \n " + weaponOverrideController);
        weaponOverrideController["Placeholder"] = clip;

        // 4. Jetzt Trigger und Bool setzen
        weaponAttackAnimator.ResetTrigger("AttackTrigger");
        weaponAttackAnimator.SetTrigger("AttackTrigger");
        //weaponAttackAnimator.SetBool("isAttacking", true);
    }

    private void RotateWeaponTowardDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;
        Debug.Log(direction);
        // Richtung normalisieren (rein zur Sicherheit)
        direction.y = 0; // Nur horizontale Rotation
        Vector3 lookDir = direction.normalized;

        // Rotation berechnen
        Quaternion lookRotation = Quaternion.LookRotation(lookDir);

        // Objekt (z. B. Waffe) drehen
        weaponAttackAnimator.transform.rotation = lookRotation;
    }

    public void OnAttackAnimationEnd()
    {
        Animator parentAnimator = GetComponentInParent<Animator>();
        if (parentAnimator != null)
        {
            parentAnimator.SetBool("isAttacking", false);
        }
    }


    //helper functions

    //this function converts a Vector2 direction to an index to a slice around a circle
    //this goes in a counter-clockwise direction.

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


}
