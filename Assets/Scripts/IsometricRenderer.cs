using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//What would actually drasticly improve the Player-Combat Feedback would be, if Attack-Animation was always played, when the NPC is actually attacking.

public class IsometricRenderer : MonoBehaviour
{
    //Arrays for Player and NPC's with 8 Directions
    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };
    public static readonly string[] runDirections = { "Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };
    public static readonly string[] runNPCDirections = {"Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };


    //Arrays for NPC's which only have 4 Directions
    public static readonly string[] runNPC4Directions = { "Run NE", "Run SE", "Run SW", "Run NW" };
    public static readonly string[] static4Directions = { "Static NE", "Static SE", "Static SW", "Static NW" };
    public static readonly string[] attack4Directions = { "Attack NE", "Attack SE", "Attack SW", "Attack NW" };


    //Array created for Weapon Animation, once upon a time i didnt know about programming & logical structure.
    public static readonly string[] weaponSwing = { "Attack_N", "Attack_NW", "Attack_W", "Attack_SW", "Attack_S", "Attack_SE", "Attack_E", "Attack_NE" };

    //The animator the IsoRenderer refers to. Typically attached to the animated GameObject, e.g. the Enemy.
    Animator animator;

    //int to clalculate and safe the last direction of view.
    int lastDirection;

    int lastWeaponDirection;

    #region isoType für 8 oder 4 Directions. Könnte überflüssig sein, ggf. später säubern.
    /*
    //Create an Enum to Set number of Isometric Directions
    private enum IsoType { eightDir, forDir }

    //Set the Value of above Enum for Isometric Directions
    [SerializeField]
    IsoType isoType;
    */
    #endregion

    //Setze Zeit für die Combat-Stance (später sollte diese vom AttackSpeed des Schwertes beeinflusst werden.)
    public bool inCombatStance;

    //public bool isAttacking;

    //Die ^ inCombatStance scheint nicht ganz sinnig zu sein. Eher sollte eine Funktion "Play" her, in der beliebige Animationen gespielt werden können.
    //Entsprechende, manuell gschaltete Animationen werden wichtig, sobald NPC's Casts oder Fähigkeiten besitzen.
    public static readonly string[] castAnimations = { "CastNE", "CastSE", "CastSW", "CastNW" };



    private void Awake()
    {
        //cache the animator component
        animator = GetComponent<Animator>();
        inCombatStance = false;

        
    }


    public void SetDirection(Vector2 direction){


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

    public void SetNPCDirection(Vector2 direction)
    {

        //use the Run states by default
        string[] directionArray = null;


        
        //measure the magnitude of the input.
        if (direction.magnitude < .01f)
        {
            //if we are basically standing still, we'll use the Static states

            //check what type of iso this GO has
            //if (isoType == IsoType.eightDir)
            //    directionArray = staticDirections;

            //else if (isoType == IsoType.forDir)
                directionArray = static4Directions;


        }
        else
        {
            //we can calculate which direction we are going in
            //use DirectionToIndex to get the index of the slice from the direction vector
            //save the answer to lastDirection

            //check if GO is attacking
            /*
            if (inCombatStance)
            {
                //directionArray = attack4Directions;
                directionArray = static4Directions;

                lastDirection = DirectionToIndex(direction, 4);                

            }
            */
            if (!inCombatStance)
            {
                directionArray = runNPC4Directions;
                lastDirection = DirectionToIndex(direction, 4);
            }

        }

        //tell the animator to play the requested state
        //print(lastDirection);
        animator.Play(directionArray[lastDirection]);
    }

    public void AttackAnimation()
    {
        
        //isAttacking = true;

        animator.Play(attack4Directions[lastDirection]);

        AnimatorClipInfo[] currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);

        float attackDuration = currentClipInfo[0].clip.length;

        attackDuration -= Time.deltaTime;

        //print(attackDuration);
        if (attackDuration <= 0)
        {
            animator.Play(static4Directions[lastDirection]);
            //inCombatStance = false;
        }


    }

    public void AnimateCast(Vector2 direction)
    {

        lastDirection = DirectionToIndex(direction, 4);

        animator.Play(castAnimations[lastDirection]);

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
