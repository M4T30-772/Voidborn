using System.Collections;
using UnityEngine;

public class Crow : Enemy
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
    [SerializeField] private BoxCollider2D attackRangeCollider; // Collider for attack range
    [SerializeField] private AudioClip attackSound; // Sound effect for attack
    private AudioSource audioSource; // AudioSource component

    private Animator animator;
    private float attackTimer;
    private float flipTimer;
    private EnemyStates previousState;
    private bool isAttacking;
    private float chaseEndTime;
    private bool isGrounded;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // Initialize AudioSource
        attackTimer = attackCooldown;
        ChangeState(EnemyStates.Crow_Idle);
        attackRangeCollider.enabled = false; // Ensure the attack range collider is initially disabled
    }

    protected override void UpdateEnemyStates()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, ledgeCheckY + 0.1f, whatIsGround);

        if (health <= 0 && currentEnemyState != EnemyStates.Crow_Death)
        {
            ChangeState(EnemyStates.Crow_Death);
            StartCoroutine(HandleDeath());
            return;
        }

        switch (currentEnemyState)
        {
            case EnemyStates.Crow_Idle:
                animator.SetBool("Crow_Idle", true);
                animator.SetBool("Crow_Walking", false);
                if (distanceToPlayer < chaseDistance)
                {
                    ChangeState(EnemyStates.Crow_Attack);
                    chaseEndTime = Time.time + chaseDuration;
                }
                break;

            case EnemyStates.Crow_Attack:
                if (Time.time > chaseEndTime && !isAttacking)
                {
                    ChangeState(EnemyStates.Crow_Idle);
                }
                else if (distanceToPlayer < attackDistance && attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    isAttacking = true;
                    animator.SetTrigger("Crow_Attack");
                    StartCoroutine(PerformAttack());
                }
                else if (distanceToPlayer >= attackDistance || !isAttacking)
                {
                    isAttacking = false;
                    ChasePlayer();
                }
                break;

            case EnemyStates.Crow_Death:
                animator.SetTrigger("Crow_Death");
                break;

            case EnemyStates.Crow_Hurt:
                animator.SetTrigger("Crow_Hurt");
                StartCoroutine(HandleHurt());
                break;
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
        }

        FacePlayer();
    }

    private void ChasePlayer()
    {
        if (isGrounded)
        {
            Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.velocity = new Vector2(directionToPlayer.x * chaseSpeed, rb.velocity.y);
        }
    }

    private void FacePlayer()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            if (transform.localScale.x > 0)
            {
                transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
            }
        }
        else
        {
            if (transform.localScale.x < 0)
            {
                transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
            }
        }
    }

    private IEnumerator HandleHurt()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        ChangeState(previousState);
    }

    private IEnumerator HandleDeath()
    {
        animator.SetTrigger("Crow_Death");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        // Award souls to the player
        SaveData.Instance.money += soulsOnDeath;
        Destroy(gameObject);
    }

    private IEnumerator PerformAttack()
    {
        attackRangeCollider.enabled = true; // Enable the attack range collider during the attack
        
        // Play attack sound
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        attackRangeCollider.enabled = false; // Disable the attack range collider after the attack
        isAttacking = false;
        ChangeState(EnemyStates.Crow_Idle);
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);

        if (health <= 0 && currentEnemyState != EnemyStates.Crow_Death)
        {
            ChangeState(EnemyStates.Crow_Death);
            StartCoroutine(HandleDeath());
        }
        else if (health > 0 && currentEnemyState != EnemyStates.Crow_Hurt)
        {
            previousState = currentEnemyState;
            ChangeState(EnemyStates.Crow_Hurt);
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
            if (currentEnemyState == EnemyStates.Crow_Attack)
            {
                PlayerController.Instance.TakeDamage(damage);
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
        }
    }
}
