using System.Collections;
using UnityEngine;

public class Bandit : Enemy
{
    [SerializeField] private float chaseDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float chaseDuration;
    [SerializeField] private BoxCollider2D attackRangeCollider; 

    private Animator m_animator;
    private float attackTimer;
    private float flipTimer;
    private EnemyStates previousState;
    private bool isAttacking;
    private float chaseEndTime;
    private bool m_grounded;

    private const int AnimState_Idle = 1;
    private const int AnimState_Run = 2;

    private const string Trigger_Attack = "Attack";
    private const string Trigger_Hurt = "Hurt";
    private const string Trigger_Death = "Death";

    protected override void Start()
    {
        base.Start();
        m_animator = GetComponent<Animator>();
        attackTimer = attackCooldown;
        ChangeState(EnemyStates.Bandit_Idle);
        attackRangeCollider.enabled = false; 
    }

    protected override void UpdateEnemyStates()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        m_grounded = Physics2D.Raycast(transform.position, Vector2.down, ledgeCheckY + 0.1f, whatIsGround);

        if (health <= 0 && currentEnemyState != EnemyStates.Bandit_Death)
        {
            ChangeState(EnemyStates.Bandit_Death);
            StartCoroutine(BanditHandleDeath());
            return;
        }

        switch (currentEnemyState)
        {
            case EnemyStates.Bandit_Idle:
                m_animator.SetInteger("AnimState", AnimState_Idle);
                if (distanceToPlayer < chaseDistance)
                {
                    ChangeState(EnemyStates.Bandit_Chase);
                    chaseEndTime = Time.time + chaseDuration;
                }
                break;

            case EnemyStates.Bandit_Chase:
                m_animator.SetInteger("AnimState", AnimState_Run);
                if (Time.time > chaseEndTime && !isAttacking)
                {
                    ChangeState(EnemyStates.Bandit_Idle);
                }
                else if (distanceToPlayer < attackDistance && attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    isAttacking = true;
                    StartCoroutine(BanditPerformAttack());
                }
                else if (!isAttacking)
                {
                    BanditChasePlayer();
                }
                break;

            case EnemyStates.Bandit_Death:
                m_animator.SetTrigger(Trigger_Death);
                break;

            case EnemyStates.Bandit_Hurt:
                m_animator.SetTrigger(Trigger_Hurt);
                StartCoroutine(BanditHandleHurt());
                break;
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
        }

        if (!isAttacking) 
        {
            BanditFacePlayer();
        }
    }

    private void BanditChasePlayer()
    {
        if (m_grounded)
        {
            Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.velocity = new Vector2(directionToPlayer.x * chaseSpeed, rb.velocity.y);
        }
    }

    private void BanditFacePlayer()
    {
        Vector3 scale = transform.localScale;

        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {

            if (scale.x < 0)
            {
                scale.x *= -1;  
            }
        }
        else
        {
            if (scale.x > 0)
            {
                scale.x *= -1;  
            }
        }

        transform.localScale = scale;
    }

    private IEnumerator BanditHandleHurt()
    {
        yield return new WaitForSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length);
        ChangeState(previousState);
    }

private IEnumerator BanditHandleDeath()
{

    m_animator.SetTrigger(Trigger_Death);
    Debug.Log("Death animation triggered.");


    yield return null;

    AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
    if (stateInfo.IsName("Death")) 
    {
        Debug.Log("Animator is in Death state.");
    }
    else
    {
        Debug.LogError("Animator did not enter Death state.");

        Destroy(gameObject);
        yield break;
    }


    float deathAnimLength = stateInfo.length;
    Debug.Log($"Death animation length: {deathAnimLength} seconds.");
    yield return new WaitForSeconds(deathAnimLength);

        SaveData.Instance.money += soulsOnDeath;

    Debug.Log("Destroying Bandit after death animation.");
    Destroy(gameObject);
}


    private IEnumerator BanditPerformAttack()
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


            if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < chaseDistance)
            {
                ChangeState(EnemyStates.Bandit_Chase);
            }
            else
            {
                ChangeState(EnemyStates.Bandit_Idle);
            }
        }
        else
        {

            isAttacking = false;
            ChangeState(EnemyStates.Bandit_Idle);
        }
    }

public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
{
    Debug.Log($"Bandit hit! Damage: {_damageDone}");

    health -= _damageDone;  

    if (health <= 0 && currentEnemyState != EnemyStates.Bandit_Death)
    {
        Debug.Log("Health is zero or below. Starting BanditHandleDeath.");
        ChangeState(EnemyStates.Bandit_Death);
        StartCoroutine(BanditHandleDeath());
    }
    else if (health > 0 && currentEnemyState != EnemyStates.Bandit_Hurt && currentEnemyState != EnemyStates.Bandit_Death)
    {
        previousState = currentEnemyState;
        ChangeState(EnemyStates.Bandit_Hurt);
        StartCoroutine(BanditHandleHurt());
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
            if (currentEnemyState == EnemyStates.Bandit_Chase)
            {
                PlayerController.Instance.TakeDamage(damage);
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
        }
    }
}
