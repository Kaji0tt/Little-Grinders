using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;



    //[SerializeField]
    //public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E", "Static NE" };

    [SerializeField]
    public GameObject[] toAnimate;

    int objectToAnimate;

    private void Awake()
    {

    }

    
    public void AnimateMe(Vector2 inputVector, float player_distance, float attackRange, float aggroRange)
    {

        objectToAnimate = SetAnimatedObject(inputVector);


        if (objectToAnimate == 0)
        {
            toAnimate[0].SetActive(true);
            toAnimate[1].SetActive(false);
        }
        else 
        {
            toAnimate[1].SetActive(true);
            toAnimate[0].SetActive(false);
        }




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


    public static int SetAnimatedObject(Vector2 dir)
    {
        //get the normalized direction
        Vector2 normDir = dir.normalized;

        //calculate how many degrees one slice is
        float step = 360f / 2; // <- Diese Variabel verändern, wenn man einen Mob mit mehr als 2 Richtungen (Oben / Unten) animieren will.

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
