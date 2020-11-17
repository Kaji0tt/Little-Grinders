using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private GameObject Front, Back;

    //[SerializeField]
    //public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };

    [SerializeField]
    public GameObject[] toAnimate;

    int objectToAnimate;

    private void Awake()
    {

    }

    #region Own-Animator;
    
    public void AnimateMe(Vector2 inputVector, float player_distance, float attackRange, float aggroRange)
    {

        objectToAnimate = SetAnimatedObject(inputVector);

        toAnimate[objectToAnimate].SetActive(true);

        animator = toAnimate[objectToAnimate].GetComponent<Animator>();

        animator.SetFloat("AnimDistance", player_distance);

        if (animator.GetFloat("AnimDistance") <= attackRange)
        {
            animator.Play("Attacking");
        }
        else if (animator.GetFloat("AnimDistance") <= aggroRange)
        {
            animator.Play("Chasing");
        }
        else if (animator.GetFloat("AnimDistance") >= aggroRange)
        {
            animator.Play("Idle");
        }


    }
    /*
    private void AnimateSide_Right(float player_distance, float attackRange, float aggroRange)
    {
        animator.SetFloat("AnimDistance", player_distance);

        
        else if (animator.GetFloat("AnimDistance") <= aggroRange)
        {
            animator.Play("R_Chasing");
        }
        else if (animator.GetFloat("AnimDistance") >= aggroRange)
        {
            animator.Play("R_Idle");
        }
    }

    private void AnimateSide_Left(float player_distance, float attackRange, float aggroRange)
    {
        animator.SetFloat("AnimDistance", player_distance);

        if (animator.GetFloat("AnimDistance") <= attackRange)
        {
            animator.Play("L_Attacking");
        }
        else if (animator.GetFloat("AnimDistance") <= aggroRange)
        {
            animator.Play("L_Chasing");
        }
        else if (animator.GetFloat("AnimDistance") >= aggroRange)
        {
            animator.Play("L_Idle");
        }
    }

    private void AnimateFront(float player_distance, float attackRange, float aggroRange)
    {
        Front.SetActive(true);
        Back.SetActive(false);


        //animator.SetFloat("AnimDistance", player_distance);

        if (gameObject.activeSelf)
        {

            if (animator.GetFloat("AnimDistance") <= attackRange)
            {
                animator.Play("F_Attacking");
            }
            else if (animator.GetFloat("AnimDistance") <= aggroRange)
            {
                animator.Play("F_Chasing");
            }
            else if (animator.GetFloat("AnimDistance") >= aggroRange)
            {
                animator.Play("F_Idle");
            }
        }

        
    }

    private void AnimateBack(float player_distance, float attackRange, float aggroRange)
    {
        Back.SetActive(true);
        Front.SetActive(false);

        if (gameObject.activeSelf)
        {
            print("this loob is triggered");
            animator.SetFloat("AnimDistance", player_distance);
            if (animator.GetFloat("AnimDistance") <= attackRange)
            {
                animator.Play("B_Attacking");
            }
            else if (animator.GetFloat("AnimDistance") <= aggroRange)
            {
                animator.Play("B_Chasing");
            }
            else if (animator.GetFloat("AnimDistance") >= aggroRange)
            {
                animator.Play("B_Idle");
            }
        }

    }

    */
    #endregion
    //Übernahme von Funktionen des IsometricCharacterRenderers. Warum das Rad neu erfinden? Hauptsache man versteht es.
    
    //Krux: Im IsoRenderer werden die Animationen aus einem einzelnen Animator ausgelesen. Wir allerdings, haben unterschiedliche GOs,
    //die entsprechend des direction Vectors ausgelesen werden sollen.
    
    /*
    public void SetNPCDirection(Vector2 direction)
    {

        //use the Run states by default
        string[] directionArray = null;

        //measure the magnitude of the input.
        if (direction.magnitude < .01f)
        {
            //if we are basically standing still, we'll use the Static states
            //we won't be able to calculate a direction if the user isn't pressing one, anyway!
            //directionArray = staticDirections;
        }
        else
        {
            //we can calculate which direction we are going in
            //use DirectionToIndex to get the index of the slice from the direction vector
            //save the answer to lastDirection
            //directionArray = runNPCDirections;
            //lastDirection = DirectionToIndex(direction, 8);
        }

        //tell the animator to play the requested state
        //animator.Play(directionArray[lastDirection]);
    }

    */

    public static int SetAnimatedObject(Vector2 dir)
    {
        //get the normalized direction
        Vector2 normDir = dir.normalized;
        //calculate how many degrees one slice is
        float step = 360f / 4;
        //calculate how many degress half a slice is.
        //we need this to offset the pie, so that the North (UP) slice is aligned in the center
        float halfstep = step / 2;
        //get the angle from -180 to 180 of the direction vector relative to the Up vector.
        //this will return the angle between dir and North.
        float angle = Vector2.SignedAngle(Vector2.up, normDir);
        //add the halfslice offset
        angle += halfstep;
        //if angle is negative, then let's make it positive by adding 360 to wrap it around.
        if (angle < 0)
        {
            angle += 360;
        }
        //calculate the amount of steps required to reach this angle
        float stepCount = angle / step;
        //round it, and we have the answer!
        return Mathf.FloorToInt(stepCount);

        //Also zusammengefasst passiert hier mathematische Magie, aber es bleibt zu sagen, dass
        //der Return-Wert mir die Richtung zurück gibt (North, West, South, East)
    }

}
