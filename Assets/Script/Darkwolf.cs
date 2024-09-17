using System.Collections;
using UnityEngine;

public class Darkwolf : Enemy
{
    [SerializeField] private float maxHealth; // Maximum health of the Darkwolf
    [HideInInspector] public float health; // Current health of the Darkwolf

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

    [Header("Audio")]
    [SerializeField] private AudioClip howlStartClip;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] [Range(0f, 1f)] private float hurtVolume = 0.2f; // Volume for the hurt sound

    private AudioSource audioSource;

    private Animator m_animator;
    private float attackTimer;
    private float flipTimer;
    private EnemyStates previousState;
    private bool isAttacking;
    private float chaseEndTime;
    private bool m_grounded;

    // Animation state values
    private const int AnimState_Idle = 1;
    private const int AnimState_Run = 2;

    // Trigger names
    private const string Trigger_Attack = "Attack";
    private const string Trigger_Hurt = "Hurt";
    private const string Trigger_Death = "Death";

    protected override void Start()
    {
        base.Start();
        m_animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attackTimer = attackCooldown;

        // Initialize health
        health = maxHealth;

        // Initialize the boss health bar
        UIManager.Instance.SetBossHealthBarVisible(true);
        UIManager.Instance.UpdateBossHealthBar(health, maxHealth);
        UIManager.Instance.SetBossName("Darkwolf");

        ChangeState(EnemyStates.Darkwolf_Idle);
        attackRangeCollider.enabled = false; // Ensure the attack range collider is initially disabled
    }

    protected override void UpdateEnemyStates()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        m_grounded = Physics2D.Raycast(transform.position, Vector2.down, ledgeCheckY + 0.1f, whatIsGround);

        // Debugging state and grounding
        Debug.Log($"Current State: {currentEnemyState}, Grounded: {m_grounded}, Distance to Player: {distanceToPlayer}");

        if (health <= 0 && currentEnemyState != EnemyStates.Darkwolf_Death)
        {
            ChangeState(EnemyStates.Darkwolf_Death);
            StartCoroutine(DarkwolfHandleDeath());
            return;
        }

        switch (currentEnemyState)
        {
            case EnemyStates.Darkwolf_Idle:
                m_animator.SetBool("Idle", true);
                m_animator.SetBool("Run", false);
                if (distanceToPlayer < chaseDistance)
                {
                    ChangeState(EnemyStates.Darkwolf_Chase);
                    chaseEndTime = Time.time + chaseDuration;
                    PlayHowlStartSound();
                }
                break;

            case EnemyStates.Darkwolf_Chase:
                m_animator.SetBool("Idle", false);
                m_animator.SetBool("Run", true);
                if (Time.time > chaseEndTime && !isAttacking)
                {
                    ChangeState(EnemyStates.Darkwolf_Idle);
                }
                else if (distanceToPlayer < attackDistance && attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    isAttacking = true;
                    StartCoroutine(DarkwolfPerformAttack());
                }
                else if (!isAttacking)
                {
                    DarkwolfChasePlayer();
                }
                break;

            case EnemyStates.Darkwolf_Death:
                m_animator.SetTrigger(Trigger_Death);
                break;

            case EnemyStates.Darkwolf_Hurt:
                m_animator.SetTrigger(Trigger_Hurt);
                StartCoroutine(DarkwolfHandleHurt());
                break;
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
        }

        if (!isAttacking) // Only face the player when not attacking
        {
            DarkwolfFacePlayer();
        }
    }

    private void DarkwolfChasePlayer()
    {
        // Debugging ground check
        Debug.DrawRay(transform.position, Vector2.down * (ledgeCheckY + 0.1f), Color.red);

        if (m_grounded)
        {
            Vector2 directionToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.velocity = new Vector2(directionToPlayer.x * chaseSpeed, rb.velocity.y);
            Debug.Log($"Chasing player. Velocity: {rb.velocity}");
        }
        else
        {
            Debug.Log("Not grounded, can't chase player.");
        }
    }

    private void DarkwolfFacePlayer()
    {
        Vector3 scale = transform.localScale;

        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            // Player is to the left, face left
            if (scale.x < 0)
            {
                scale.x *= -1;  // Flip the scale to face left
            }
        }
        else
        {
            // Player is to the right, face right
            if (scale.x > 0)
            {
                scale.x *= -1;  // Flip the scale to face right
            }
        }

        transform.localScale = scale;
    }

    private void PlayHowlStartSound()
    {
        if (howlStartClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(howlStartClip);
        }
    }

    private void PlayAttackSound()
    {
        if (attackClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackClip);
        }
    }

    private void PlayHurtSound()
    {
        if (hurtClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtClip, hurtVolume); // Play at reduced volume
        }
    }

    private IEnumerator DarkwolfHandleHurt()
    {
        // Hurt animation duration
        PlayHurtSound();
        yield return new WaitForSeconds(0.15f);
        ChangeState(previousState);
    }

    private IEnumerator DarkwolfHandleDeath()
    {
        // Step 1: Trigger the death animation
        m_animator.SetTrigger(Trigger_Death);
        Debug.Log("Death animation triggered.");

        // Step 2: Wait one frame to ensure the Animator processes the trigger
        yield return null;

        // Step 3: Ensure the Animator has transitioned to the Death state
        AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Death")) 
        {
            Debug.Log("Animator is in Death state.");
        }
        else
        {
            Debug.LogError("Animator did not enter Death state.");
            // Optionally destroy the object here if the state isn't reached
            Destroy(gameObject);
            yield break;
        }

        // Step 4: Wait for the length of the animation
        float deathAnimLength = stateInfo.length;
        Debug.Log($"Death animation length: {deathAnimLength} seconds.");
        yield return new WaitForSeconds(deathAnimLength);
        
        // Award souls to the player
        SaveData.Instance.money += soulsOnDeath;

        // Step 5: Destroy the object
        Debug.Log("Destroying Darkwolf after death animation.");
        Destroy(gameObject);
    }

    private IEnumerator DarkwolfPerformAttack()
    {
        int attackCount = Random.Range(2, 6); // Randomly choose between 2 and 6 attacks

        for (int i = 0; i < attackCount; i++)
        {
            // Ensure the Darkwolf is in range to attack
            if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackDistance)
            {
                // Stop the Darkwolf from moving
                rb.velocity = Vector2.zero;

                // Trigger the attack animation
                m_animator.SetTrigger(Trigger_Attack);
                PlayAttackSound();

                // Wait for 0.5 seconds before enabling the attack range collider
                yield return new WaitForSeconds(0.5f);

                // Enable the attack range collider during the attack
                attackRangeCollider.enabled = true;

                // Wait for the remaining duration of the attack animation before disabling the collider
                float remainingAnimationTime = m_animator.GetCurrentAnimatorStateInfo(0).length - 0.5f;
                yield return new WaitForSeconds(remainingAnimationTime);

                // Disable the attack range collider after the attack
                attackRangeCollider.enabled = false;
            }
            else
            {
                // If not in range, cancel the attack and reset to idle
                isAttacking = false;
                ChangeState(EnemyStates.Darkwolf_Idle);
                yield break;
            }

            // Small delay between attacks
            yield return new WaitForSeconds(0.2f);
        }

        // After attacking, run back to the starting position and idle
        StartCoroutine(DarkwolfRunBackAndIdle());
    }

    private IEnumerator DarkwolfRunBackAndIdle()
    {
        // Run back to the initial position
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + new Vector2(-chaseDistance, 0); // Adjust the run back distance as needed
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
            yield return null;
        }

        // Idle for 3 seconds
        ChangeState(EnemyStates.Darkwolf_Idle);
        yield return new WaitForSeconds(3f);

        // Resume chasing the player after idling
        ChangeState(EnemyStates.Darkwolf_Chase);
        isAttacking = false;
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        Debug.Log($"Darkwolf hit! Damage: {_damageDone}");

        health -= _damageDone;  // Reduce health by the damage done

        // Update the health bar UI after taking damage
        UIManager.Instance.UpdateBossHealthBar(health, maxHealth);

        if (health <= 0 && currentEnemyState != EnemyStates.Darkwolf_Death)
        {
            Debug.Log("Health is zero or below. Starting DarkwolfHandleDeath.");
            ChangeState(EnemyStates.Darkwolf_Death);
            StartCoroutine(DarkwolfHandleDeath());

            // Hide the boss health bar on death
            UIManager.Instance.SetBossHealthBarVisible(false);
        }
        else if (health > 0 && currentEnemyState != EnemyStates.Darkwolf_Hurt && currentEnemyState != EnemyStates.Darkwolf_Death)
        {
            previousState = currentEnemyState;
            ChangeState(EnemyStates.Darkwolf_Hurt);
            StartCoroutine(DarkwolfHandleHurt());
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
            if (currentEnemyState == EnemyStates.Darkwolf_Chase)
            {
                PlayerController.Instance.TakeDamage(damage);
                PlayerController.Instance.HitStopTime(0, 5, 0.5f);
            }
        }
    }
}
