using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Nooow, this is going to suck balls deeply, but satisfy myself even more: https://www.youtube.com/watch?v=hUERvxcKt_I
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;


    [SerializeField]
    public GameObject[] toAnimate;

    int objectToAnimate;

    private void Awake()
    {

    }

    //Wenn ich alles über einen einzelnen Animation-Controller steuern wollen würde, wäre die Lösung den State im Abhängigkeit vom Integer "objectToAnimate" zu bestimmen.
    public void AnimateMe(Vector2 inputVector, float player_distance, float attackRange, float aggroRange)
    {
        if (toAnimate.Length != 0)
        {
            objectToAnimate = SetAnimatedObject(inputVector);

            ToggleActiveObject(objectToAnimate);


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


    }


    //thanks inScope.

    public int SetAnimatedObject(Vector2 dir)
    {
        //get the normalized direction
        Vector2 normDir = dir.normalized;

        //calculate how many degrees one slice is
        float step = 360f / toAnimate.Length; // <- Diese Variabel verändern, wenn man einen Mob mit mehr als 2 Richtungen (Oben / Unten) animieren will.

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

    //Das sind Schleifen die in Runtime kontinuierlich abgerufen werden. Nicht gut. Finde bessere Methode.
    void ToggleActiveObject(int objectToAnimate)
    {

        for (int i = 0; i < toAnimate.Length; i++)
        {
            if (i == objectToAnimate)
            {
                toAnimate[i].SetActive(true);
                for(int z = 0; z < toAnimate.Length; z++)
                {
                    if (z != i)
                        toAnimate[z].SetActive(false);
                }
            }


        }
    }
}
