using System.Collections;
using UnityEngine;

public class SkeletonEnemy : Enemy
{
    [Header("Patrolling Settings")]
    [SerializeField] private float patrolRange;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float patrolTime;

    [Header("Chasing Settings")]
    [SerializeField] private float chaseRange;
    [SerializeField] private float chaseSpeed;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private BoxCollider2D attackCollider;
    [SerializeField] private AudioClip attackSound; // Attack sound clip

    [Header("Death Settings")]
    [SerializeField] private AudioClip deathSound; // Death sound clip

    [Header("Health Settings")]
    [SerializeField] private float maxHealth;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private Transform groundCheckTransform;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound; // Walking sound clip
    [SerializeField] private float soundDistanceThreshold = 10f; // Distance within which the sound is audible
    private AudioSource audioSource; // AudioSource component

    private Vector2 patrolDirection;
    private float patrolTimer;
    private float lastAttackTime;
    private Transform playerTransform;
    private Animator animator;
    private Rigidbody2D rb;

    private float currentHealth;
    private bool isDead;
    private bool isAttacking;
    private bool isGrounded;
    private bool isWalking; // To track if the enemy is walking

    private EnemyStates previousState;

    protected override void Start()
    {
        base.Start();
        Initialize();
    }

    private void Initialize()
    {
        patrolDirection = GetRandomDirection();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1;
        rb.isKinematic = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned!");
        }

        if (attackCollider == null)
        {
            Debug.LogError("Attack Collider is not assigned!");
        }
        else
        {
            attackCollider.enabled = false;
        }

        damage = 1;
        lastAttackTime = -attackCooldown;
        currentHealth = maxHealth;
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();
        UpdateGroundStatus();
        if (isRecoiling || isAttacking) return;

        HandleStateChange();
        UpdateEnemyStates();
        ManageWalkingSound(); // Manage the walking sound based on distance
    }

    private void UpdateGroundStatus()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, LayerMask.GetMask("Ground"));
    }

    private void HandleStateChange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            ChangeState(EnemyStates.Skeleton_Attack);
        }
        else if (distanceToPlayer <= chaseRange)
        {
            ChangeState(EnemyStates.Skeleton_Chase);
        }
        else
        {
            ChangeState(EnemyStates.Skeleton_Walk);
        }
    }

    protected override void UpdateEnemyStates()
    {
        if (isDead) return;

        switch (currentEnemyState)
        {
            case EnemyStates.Skeleton_Walk:
                HandlePatrolling();
                break;
            case EnemyStates.Skeleton_Chase:
                HandleChasing();
                break;
            case EnemyStates.Skeleton_Attack:
                StartCoroutine(PerformAttack());
                break;
            case EnemyStates.Skeleton_Idle:
                PlayAnimation("Skeleton_Idle");
                StopWalkingSound();
                break;
            case EnemyStates.Skeleton_Hurt:
                if (!isDead) StartCoroutine(HandleHurt());
                StopWalkingSound();
                break;
            case EnemyStates.Skeleton_Death:
                if (!isDead) HandleDeath();
                StopWalkingSound();
                break;
        }
    }

    private void HandlePatrolling()
    {
        if (isGrounded)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolTime)
            {
                patrolTimer = 0;
                patrolDirection = GetRandomDirection();
            }

            rb.velocity = patrolDirection * patrolSpeed;
            Flip(patrolDirection.x);
            PlayAnimation("Skeleton_Walk");

            if (!isWalking)
            {
                PlayWalkingSound();
                isWalking = true;
            }
        }
    }

    private void HandleChasing()
    {
        if (isGrounded)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.velocity = direction * chaseSpeed;
            Flip(direction.x);
            PlayAnimation("Skeleton_Walk");

            if (!isWalking)
            {
                PlayWalkingSound();
                isWalking = true;
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        PlayAnimation("Skeleton_Attack");
        PlayAttackSound(); // Play attack sound

        yield return new WaitForSeconds(0.33f);

        if (attackCollider != null) attackCollider.enabled = true;

        yield return new WaitForSeconds(0.17f);

        if (attackCollider != null) attackCollider.enabled = false;

        lastAttackTime = Time.time;
        isAttacking = false;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        ChangeState(distanceToPlayer <= chaseRange ? EnemyStates.Skeleton_Chase : EnemyStates.Skeleton_Walk);
    }

    private IEnumerator HandleHurt()
    {
        Debug.Log("Handling Hurt State.");

        PlayAnimation("Skeleton_Hurt");

        float hurtAnimationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(hurtAnimationLength);

        if (!isDead)
        {
            ChangeState(previousState == EnemyStates.Skeleton_Hurt ? EnemyStates.Skeleton_Idle : previousState);
        }
    }

    private void HandleDeath()
    {
        if (!isDead)
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            PlayAnimation("Skeleton_Death");
            PlayDeathSound(); // Play death sound
            Destroy(gameObject, 1f);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    private Vector2 GetRandomDirection()
    {
        return Random.value > 0.5f ? Vector2.left : Vector2.right;
    }

    private void Flip(float direction)
    {
        if (direction != 0)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = Mathf.Sign(direction) * Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }

    private void PlayWalkingSound()
    {
        if (audioSource != null && walkSound != null)
        {
            if (!audioSource.isPlaying) // Play only if not already playing
            {
                audioSource.clip = walkSound;
                audioSource.loop = true; // Loop the walking sound
                audioSource.Play();
            }
        }
    }

    private void StopWalkingSound()
    {
        if (audioSource != null && isWalking)
        {
            audioSource.Stop();
            isWalking = false;
        }
    }

    private void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound); // Play attack sound
        }
    }

    private void PlayDeathSound()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound); // Play death sound
        }
    }

    private void ManageWalkingSound()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= soundDistanceThreshold)
        {
            if (!isWalking)
            {
                PlayWalkingSound();
                isWalking = true;
            }
        }
        else
        {
            StopWalkingSound();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRange);
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }

    public override void EnemyHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        Debug.Log($"EnemyHit: Damage = {damageDone}, Direction = {hitDirection}, Force = {hitForce}");

        base.EnemyHit(damageDone, hitDirection, hitForce);

        if (isDead) return;

        currentHealth -= damageDone;
        Debug.Log($"Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Enemy is dead.");
            Die();
        }
        else
        {
            previousState = currentEnemyState;
            ChangeState(EnemyStates.Skeleton_Hurt);
        }
    }

    private void Die()
    {
        if (!isDead)
        {
            isDead = true;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            PlayAnimation("Skeleton_Death");
            PlayDeathSound(); // Play death sound
            Destroy(gameObject, 1f);
        }
    }
}
