using System.Collections;
using UnityEngine;

public class HeroKnight : Enemy
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
    [SerializeField] private float blockDuration = 1f;
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float jumpCooldown = 1f;

    [SerializeField] private AudioClip swordSwingSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip blockSound;

    [SerializeField] private float maxHealth = 40f; 
    private float currentHealth; 

    private AudioSource audioSource;
    private Animator animator;
    private float attackTimer;
    private float flipTimer;
    private EnemyStates previousState;
    private bool isAttacking;
    private bool isBlocking;
    private bool isRolling;
    private bool isJumping;
    private float chaseEndTime;
    private bool isGrounded;
    private float rollEndTime;
    private float jumpEndTime;

    private const int AnimState_Idle = 0;
    private const int AnimState_Run = 1;

    private const string Trigger_Attack1 = "Attack1";
    private const string Trigger_Attack2 = "Attack2";
    private const string Trigger_Attack3 = "Attack3";
    private const string Trigger_Hurt = "Hurt";
    private const string Trigger_Death = "Death";
    private const string Trigger_Block = "Block";
    private const string Trigger_Roll = "Roll";
    private const string Trigger_Jump = "Jump";
    private const string Trigger_Run = "Run";

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attackTimer = attackCooldown;

        currentHealth = maxHealth;

        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager instance is not assigned!");
        }
        else
        {
            UIManager.Instance.SetBossHealthBarVisible(true);
            UIManager.Instance.SetBossName("HeroKnight");
            UIManager.Instance.UpdateBossHealthBar(currentHealth, maxHealth);
        }

        ChangeState(EnemyStates.HeroKnight_Idle);

        attackRangeCollider.enabled = false;
        isBlocking = false;
        isRolling = false;
        isJumping = false;
    }

    protected override void UpdateEnemyStates()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, ledgeCheckY + 0.1f, whatIsGround);

        if (currentHealth <= 0 && currentEnemyState != EnemyStates.HeroKnight_Death)
        {
            ChangeState(EnemyStates.HeroKnight_Death);
            StartCoroutine(HeroKnightHandleDeath());
            return;
        }

        if (isBlocking || isRolling || isJumping || isAttacking) return;

        switch (currentEnemyState)
        {
            case EnemyStates.HeroKnight_Idle:
                Debug.Log("Entering HeroKnight_Idle state");
                animator.SetInteger("AnimState", AnimState_Idle);
                animator.ResetTrigger(Trigger_Run);
                if (distanceToPlayer < chaseDistance)
                {
                    ChangeState(EnemyStates.HeroKnight_Chase);
                    chaseEndTime = Time.time + chaseDuration;
                }
                break;

            case EnemyStates.HeroKnight_Chase:
                Debug.Log("Entering HeroKnight_Chase state");
                animator.SetInteger("AnimState", AnimState_Run);
                animator.SetTrigger(Trigger_Run);
                if (Time.time > chaseEndTime && !isAttacking)
                {
                    ChangeState(EnemyStates.HeroKnight_Idle);
                }
                else if (distanceToPlayer < attackDistance && attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    isAttacking = true;
                    StartCoroutine(HeroKnightPerformAttack());
                }
                else if (!isAttacking)
                {
                    HeroKnightChasePlayer();
                }
                break;

            case EnemyStates.HeroKnight_Death:
                animator.SetTrigger(Trigger_Death);
                break;

            case EnemyStates.HeroKnight_Hurt:
                animator.SetTrigger(Trigger_Hurt);
                StartCoroutine(HeroKnightHandleHurt());
                break;
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
        }

        if (Time.time > rollEndTime)
        {
            isRolling = false;
        }

        if (Time.time > jumpEndTime)
        {
            isJumping = false;
        }

        if (!isAttacking && !isBlocking && !isRolling && !isJumping)
        {
            HeroKnightFacePlayer();
        }
    }

    private void HeroKnightChasePlayer()
    {
        if (isGrounded)
        {
            Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.velocity = new Vector2(directionToPlayer.x * chaseSpeed, rb.velocity.y);
        }
    }

    private void HeroKnightFacePlayer()
    {
        Vector3 scale = transform.localScale;

        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            if (scale.x > 0)
            {
                scale.x *= -1;
            }
        }
        else
        {
            if (scale.x < 0)
            {
                scale.x *= -1;
            }
        }

        transform.localScale = scale;
    }

    private IEnumerator HeroKnightHandleHurt()
    {
        audioSource.PlayOneShot(hurtSound);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        ChangeState(previousState);
    }

    private IEnumerator HeroKnightHandleDeath()
    {
        animator.SetTrigger(Trigger_Death);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        SaveData.Instance.money += soulsOnDeath;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHealthBarVisible(false);
        }

        Destroy(gameObject);
    }

    private IEnumerator HeroKnightPerformAttack()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackDistance)
            {
                rb.velocity = Vector2.zero;

                if (i == 1)
                {
                    animator.SetTrigger(Trigger_Attack1);
                }
                else if (i == 2)
                {
                    animator.SetTrigger(Trigger_Attack2);
                }
                else if (i == 3)
                {
                    animator.SetTrigger(Trigger_Attack3);
                }

                attackRangeCollider.enabled = true;
                audioSource.PlayOneShot(swordSwingSound);

                yield return new WaitForSeconds(0.2f);

                attackRangeCollider.enabled = false;

                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            }
            else
            {
                isAttacking = false;
                ChangeState(EnemyStates.HeroKnight_Idle);
                yield break;
            }
        }

        isAttacking = false;

        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < chaseDistance)
        {
            ChangeState(EnemyStates.HeroKnight_Chase);
        }
        else
        {
            ChangeState(EnemyStates.HeroKnight_Idle);
        }
    }

    private IEnumerator HeroKnightBlock()
    {
        isBlocking = true;
        animator.SetTrigger(Trigger_Block);
        audioSource.PlayOneShot(blockSound);

        TakeDamage(-1); 

        yield return new WaitForSeconds(blockDuration);

        isBlocking = false;
    }

    private IEnumerator HeroKnightRoll()
    {
        if (isRolling || isJumping || isAttacking || isBlocking) yield break;

        isRolling = true;
        animator.SetTrigger(Trigger_Roll);

        Vector2 rollDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        rb.velocity = new Vector2(rollDirection.x * 30f, rb.velocity.y); 

        rollEndTime = Time.time + rollCooldown;
        yield return new WaitForSeconds(rollCooldown);

        rb.velocity = new Vector2(0, rb.velocity.y);

        isRolling = false;
    }

    private IEnumerator HeroKnightJump()
    {
        if (isRolling || isJumping || isAttacking || isBlocking) yield break;

        isJumping = true;
        animator.SetTrigger(Trigger_Jump);

        rb.velocity = new Vector2(rb.velocity.x, 20f); 

        jumpEndTime = Time.time + jumpCooldown;
        yield return new WaitForSeconds(jumpCooldown);

        isJumping = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isAttacking)
        {
            PlayerController.Instance.TakeDamage(damage);
        }
        else if (!isBlocking && Random.value < 0.10f)
        {
            StartCoroutine(HeroKnightBlock());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            if (currentEnemyState == EnemyStates.HeroKnight_Chase)
            {
                PlayerController.Instance.TakeDamage(damage);
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackRangeCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackRangeCollider.bounds.center, attackRangeCollider.bounds.size);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBlocking && !isRolling && !isJumping && !isAttacking)
        {
            StartCoroutine(HeroKnightRoll());
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && !isBlocking && !isRolling && !isJumping && !isAttacking)
        {
            StartCoroutine(HeroKnightJump());
        }

        UpdateEnemyStates();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBossHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            ChangeState(EnemyStates.HeroKnight_Death);
            StartCoroutine(HeroKnightHandleDeath());
        }
    }
}
