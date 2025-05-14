using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Update: 11.05 -> Diese Klasse sollte ein einheitliches System für alle Animationen bekommen.

public class IsometricRenderer : MonoBehaviour
{
    //Arrays for Player
    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };
    public static readonly string[] runDirections = { "Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };
    public static readonly string[] runNPCDirections = {"Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };

    //Array created for Weapon Animation, once upon a time i didnt know about programming & logical structure.
    public static readonly string[] weaponSwing = { "Attack_N", "Attack_NW", "Attack_W", "Attack_SW", "Attack_S", "Attack_SE", "Attack_E", "Attack_NE" };

    //int to clalculate and safe the last direction of view.
    int lastDirection;

    int lastWeaponDirection;

    private EnemyController myController;
    /*
    //Arrays for NPC's which only have 4 Directions -> dieser bereich kann sehr gerne überarbeitet werden. die logik soll komplett dem neuen system angepasst werden.
    public static readonly string[] runNPC4Directions = { "Run NE", "Run SE", "Run SW", "Run NW" };
    public static readonly string[] static4Directions = { "Static NE", "Static SE", "Static SW", "Static NW" };
    public static readonly string[] attack4Directions = { "Attack NE", "Attack SE", "Attack SW", "Attack NW" };
    public static readonly string[] hit4Directions = { "Hit NE", "Hit SE", "Hit SW", "Hit NW" };
    */



    //The animator the IsoRenderer refers to. Typically attached to the animated GameObject, e.g. the Enemy.
    Animator animator;




    //Setze Zeit für die Combat-Stance (später sollte diese vom AttackSpeed des Schwertes beeinflusst werden.)
    public bool inCombatStance;

    //Set the Spritesheet to auto Animate this enemy.
    public Sprite spriteSheet; 
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
        inCombatStance = false;

        myController = GetComponent<EnemyController>();

    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    public void UpdateMovement()
    {
        if(myController != null && myController.navMeshAgent != null)
        if (myController.navMeshAgent.velocity.sqrMagnitude > 0.01f)
            PlayWalk();
        else
            PlayIdle();
    }

    public void PlayIdle() => animator.Play("Idle");
    public void PlayWalk() => animator.Play("Walk");
    public void PlayAttack() => animator.Play("Attack");
    public void PlayHit() => animator.Play("Hit");
    public void PlayCast() => animator.Play("Cast");
    public void PlayDie() => animator.Play("Die");



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

    //Das WaffenObjekt, auf welchem der entsprechende Animation-Controller liegt, soll animiert werden.
    public void SetWeaponDirection(Vector2 direction, Animator weaponAnim)
    {
        string[] directionArray; //= null;

        //Falls die Combat-Stance des Charakters im Animation-Controller ist #IsometricPlayer.Attack(), führe folgende Animationen aus.
        if (weaponAnim.GetBool("isAttacking") == true && inCombatStance == false)
        {

            //Aktiviere den Animation-Controller, falls dieser deaktivert war.
            weaponAnim.enabled = true;

            //Wähle entsprechende AnimationsArray aus
            directionArray = weaponSwing;

            //Berechne die letzte Blickrichtung in Abhängigkeit vom DirectionCollider (direction).
            lastWeaponDirection = DirectionToIndex(direction, 8);
            //print(lastWeaponDirection); - funktioniert.
            weaponAnim.SetTrigger(directionArray[lastWeaponDirection]);

        }


        //Falls der Character nicht am Angreifen ist #IsometricPlayer.Attack(), führe die Standrad-Waffenanimation aus.
        else if (weaponAnim.GetBool("isAttacking") == false)
        {

            //Falls sich der Charakter nicht bewegt, soll die Animation des Schwertes ebenfalls stillstehen.
            if(direction.magnitude < 0.1f)
            {
                weaponAnim.enabled = false;
            }

            else
            {
                weaponAnim.enabled = true;

                //Wähle entsprechende AnimationsArray aus
                directionArray = runDirections;

                //Spiele die Animation ab.
                weaponAnim.Play(directionArray[lastDirection]);
            }
        }

    }


    public void AttackAnimation()
    {


        AnimatorClipInfo[] currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);

        float attackDuration = currentClipInfo[0].clip.length;

        attackDuration -= Time.deltaTime;

        //print(attackDuration);
        if (attackDuration <= 0)
        {

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
