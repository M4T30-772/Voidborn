using System.Collections;
using UnityEngine;

public class Ninja : Enemy
{
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackCooldown;
    [SerializeField] private BoxCollider2D attackRangeCollider; // Collider for attack range

    private Animator m_animator;
    private float attackTimer;
    private EnemyStates previousState;
    private bool isAttacking;

    // Trigger names
    private const string Trigger_Attack = "Ninja_Attack";
    private const string Trigger_Hurt = "Ninja_Hurt";
    private const string Trigger_Death = "Ninja_Death";

    protected override void Start()
    {
        base.Start();
        m_animator = GetComponent<Animator>();
        attackTimer = attackCooldown;
        ChangeState(EnemyStates.Ninja_Idle);
        attackRangeCollider.enabled = false; // Ensure the attack range collider is initially disabled
    }

    protected override void UpdateEnemyStates()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        if (health <= 0 && currentEnemyState != EnemyStates.Ninja_Death)
        {
            ChangeState(EnemyStates.Ninja_Death);
            StartCoroutine(NinjaHandleDeath());
            return;
        }

        switch (currentEnemyState)
        {
            case EnemyStates.Ninja_Idle:
                if (distanceToPlayer < attackDistance && attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    isAttacking = true;
                    StartCoroutine(NinjaPerformAttack());
                }
                break;

            case EnemyStates.Ninja_Death:
                m_animator.SetTrigger(Trigger_Death);
                break;

            case EnemyStates.Ninja_Hurt:
                m_animator.SetTrigger(Trigger_Hurt);
                StartCoroutine(NinjaHandleHurt());
                break;
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
        }
    }

    private IEnumerator NinjaHandleHurt()
    {
        yield return new WaitForSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length);
        ChangeState(previousState);
    }

    private IEnumerator NinjaHandleDeath()
    {
        m_animator.SetTrigger(Trigger_Death);
        yield return null;

        AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Death"))
        {
            Destroy(gameObject);
            yield break;
        }

        yield return new WaitForSeconds(stateInfo.length);
        SaveData.Instance.money += soulsOnDeath;
        Destroy(gameObject);
    }

    private IEnumerator NinjaPerformAttack()
    {
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackDistance)
        {
            rb.velocity = Vector2.zero;
            m_animator.SetTrigger(Trigger_Attack);

            yield return new WaitForSeconds(0.5f);

            attackRangeCollider.enabled = true;

            float remainingAnimationTime = m_animator.GetCurrentAnimatorStateInfo(0).length - 0.5f;
            yield return new WaitForSeconds(remainingAnimationTime);

            attackRangeCollider.enabled = false;
            isAttacking = false;

            ChangeState(EnemyStates.Ninja_Idle);
        }
        else
        {
            isAttacking = false;
            ChangeState(EnemyStates.Ninja_Idle);
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;

        if (health <= 0 && currentEnemyState != EnemyStates.Ninja_Death)
        {
            ChangeState(EnemyStates.Ninja_Death);
            StartCoroutine(NinjaHandleDeath());
        }
        else if (health > 0 && currentEnemyState != EnemyStates.Ninja_Hurt && currentEnemyState != EnemyStates.Ninja_Death)
        {
            previousState = currentEnemyState;
            ChangeState(EnemyStates.Ninja_Hurt);
            StartCoroutine(NinjaHandleHurt());
        }
    }

    protected override void Attack()
    {
        if (isAttacking && Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackDistance)
        {
            PlayerController.Instance.TakeDamage(damage);
        }
    }

    private new void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            if (currentEnemyState == EnemyStates.Ninja_Idle)
            {
                PlayerController.Instance.TakeDamage(damage);
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
        }
    }
}
