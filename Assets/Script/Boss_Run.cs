using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Run : StateMachineBehaviour
{
    private Rigidbody2D rb;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponentInParent<Rigidbody2D>();   
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        TargetPlayerPosition(animator);
        if (TheVoidKnight.Instance.attackCountdown <= 0)
        {
            TheVoidKnight.Instance.AttackHandler();
            TheVoidKnight.Instance.attackCountdown = TheVoidKnight.Instance.attackTimer;
        }
    }

void TargetPlayerPosition(Animator animator)
{
    if (TheVoidKnight.Instance.Grounded())
    {
        TheVoidKnight.Instance.Flip();
        Vector2 _target = new Vector2(PlayerController.Instance.transform.position.x, rb.position.y);
        Vector2 _newPos = Vector2.MoveTowards(rb.position, _target, TheVoidKnight.Instance.runSpeed * Time.fixedDeltaTime);
        Debug.Log("Target Position: " + _target);
        Debug.Log("New Position: " + _newPos);
        rb.MovePosition(_newPos);
    }
    else
    {
        rb.velocity = new Vector2(rb.velocity.x, -25);
    }

    if (Vector2.Distance(PlayerController.Instance.transform.position, rb.position) <= TheVoidKnight.Instance.attackRange)
    {
        animator.SetBool("Run", false);
    }
}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Run", false);
    }
}
