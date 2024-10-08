using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Dive : StateMachineBehaviour
{
    Rigidbody2D rb;
    bool callOnce;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb=animator.GetComponentInParent<Rigidbody2D>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        TheVoidKnight.Instance.divingCollider.SetActive(true);

        if(TheVoidKnight.Instance.Grounded())
        {
            TheVoidKnight.Instance.divingCollider.SetActive(false);
            if(!callOnce)
            {
                TheVoidKnight.Instance.DivingPillars();
                animator.SetBool("Dive",false);
                TheVoidKnight.Instance.ResetAllAttacks();
                callOnce=true;
            }

        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       callOnce=false; 
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
