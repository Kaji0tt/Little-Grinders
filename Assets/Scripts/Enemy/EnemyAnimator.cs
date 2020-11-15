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

    private void Awake()
    {
        animator.GetComponent<Animator>();
    }


    public void AnimateMe(Vector2 inputVector, float player_distance, float attackRange, float aggroRange)
    {
        animator.SetFloat("AnimDistance", player_distance);

        if (inputVector.y <= 0)
        {
            AnimateFront(player_distance, attackRange, aggroRange);
        }
            

        if (inputVector.y >= 0)
        {

            AnimateBack(player_distance, attackRange, aggroRange);
        }

        /*
        if (inputVector.x <= 0)
            AnimateSide_Left(player_distance, attackRange, aggroRange);

        if (inputVector.x >= 0)
            AnimateSide_Right(player_distance, attackRange, aggroRange);
            */


    }

    private void AnimateSide_Right(float player_distance, float attackRange, float aggroRange)
    {
        animator.SetFloat("AnimDistance", player_distance);

        if (animator.GetFloat("AnimDistance") <= attackRange)
        {
            animator.Play("R_Attacking");
        }
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
        
}
