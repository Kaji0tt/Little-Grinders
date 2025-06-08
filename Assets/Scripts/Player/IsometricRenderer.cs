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
    Animator animator;


    /// <summary>
    /// Enemy Section:
    /// These variables are only used for the animation of the of Enemy characters.
    /// </summary>

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
        animator = GetComponent<Animator>();
        isPerformingAction = false;

        //RuntimController setzen!
        if (weaponAnimator != null)
        {
            weaponOverrideController = new AnimatorOverrideController(weaponAnimator.runtimeAnimatorController);
            weaponAnimator.runtimeAnimatorController = weaponOverrideController;
        }

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
        if (weaponAnimator == null)
            return;

        weaponAnimator.enabled = true;

        // Drehung erfolgt immer – egal ob gerade angegriffen wird
        RotateWeaponToDirection();

        // Nur Idle abspielen, wenn nicht gerade Attack läuft
        if (!weaponAnimator.GetBool("isAttacking"))
        {
            weaponAnimator.Play("Idle");
        }
    }



    /// <summary>
    /// Weapon Animation
    /// </summary>

    private AnimatorOverrideController weaponOverrideController;
    public void PlayWeaponAttack(AnimationClip clip, Animator weaponAttackAnimator)
    {
        if (clip == null || weaponAttackAnimator == null)
        {
            Debug.LogWarning("Kein Clip oder Animator übergeben!");
            return;
        }

        // Richtung berechnen & Waffe drehen
        //Vector3 dir = DirectionCollider.instance.dirVector - PlayerManager.instance.player.transform.position;
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

    public void OnAttackAnimationEnd()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetBool("isAttacking", false);
        }
        isPerformingAction = false;
    }

    private void RotateWeaponToDirection()
    {
        Vector3 worldDirection = DirectionCollider.instance.dirVector;

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
