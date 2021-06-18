using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Alle Objekte die isometrisch dargestellt werden, werden in 8 Slices eingeteilt um korrekt in der Isometrie angezeigt zu werden.
public class IsometricRenderer : MonoBehaviour
{


    //Arrays for Player and NPC's with 8 Directions
    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };
    public static readonly string[] runDirections = { "Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };
    public static readonly string[] runNPCDirections = {"Run N", "Run NW", "Run W", "Run SW", "Run S", "Run SE", "Run E", "Run NE" };


    //Arrays for NPC's which only have 4 Directions
    public static readonly string[] runNPC4Directions = { "Run NW", "Run SW", "Run SE", "Run NE" };
    public static readonly string[] static4Directions = { "Static NW", "Static SW", "Static SE", "Static NE" };
    public static readonly string[] attack4Directions = { "Attack NW", "Attack SW", "Attack SE", "Attack NE" };

    public static readonly string[] weaponSwing = { "Attack_N", "Attack_NW", "Attack_W", "Attack_SW", "Attack_S", "Attack_SE", "Attack_E", "Attack_NE" };
    Animator animator;
    int lastDirection;

    //Create an Enum to Set number of Isometric Directions
    private enum IsoType { eightDir, forDir }

    //Set the Value of above Enum for Isometric Directions
    [SerializeField]
    IsoType isoType;

    //Setze Zeit für die Combat-Stance (später sollte diese vom AttackSpeed des Schwertes beeinflusst werden.)
    public bool inCombatStance;

    private void Awake()
    {
        //cache the animator component
        animator = GetComponent<Animator>();
        inCombatStance = false;

        
    }


    public void SetDirection(Vector2 direction){


        string[] directionArray; //= null;

        if (direction.magnitude < .01f)
        { 
            directionArray = staticDirections;


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

            //Spiele die Animation über entsprechenden Integer im AnimationController ab.
            weaponAnim.SetTrigger(directionArray[lastDirection]);

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
            //we won't be able to calculate a direction if the user isn't pressing one, anyway!

            //check what type of iso this GO has
            if (isoType == IsoType.eightDir)
                directionArray = staticDirections;

            else if (isoType == IsoType.forDir)
                directionArray = static4Directions;


        }
        else
        {
            //we can calculate which direction we are going in
            //use DirectionToIndex to get the index of the slice from the direction vector
            //save the answer to lastDirection

            //check if GO is attacking
            //We are currently not planning on implementing more 8 directional Iso-SpriteSheets, for which we check
            if (inCombatStance && isoType == IsoType.forDir)
            {
                directionArray = attack4Directions;

                lastDirection = DirectionToIndex(direction, 4);

            }

            else if (!inCombatStance || isoType == IsoType.eightDir)
            {
                //again, check for isoType of this GO
                if (isoType == IsoType.eightDir)
                {
                    directionArray = runNPCDirections;
                    lastDirection = DirectionToIndex(direction, 8);
                }


                else if (isoType == IsoType.forDir)
                {
                    directionArray = runNPC4Directions;
                    lastDirection = DirectionToIndex(direction, 4);
                }
            }




        }

        //tell the animator to play the requested state
        animator.Play(directionArray[lastDirection]);
    }



    //helper functions

    //this function converts a Vector2 direction to an index to a slice around a circle
    //this goes in a counter-clockwise direction.
    public static int DirectionToIndex(Vector2 dir, int sliceCount){
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
