using System.Collections;
using UnityEngine;

public class BossBat : Enemy
{
    [Header("Bat Settings")]
    [SerializeField] private float chaseDistance;
    [SerializeField] private float stunDuration;
    [SerializeField] private float Bat_ChaseSpeed;
    [SerializeField] private float Bat_CloseSpeed;
    [SerializeField] private float chaseDuration = 10f; // Duration for chasing player
    [SerializeField] private float flyDuration = 5f; // Duration for flying around

    [Header("Audio Settings")]
    [SerializeField] private AudioClip screechSound; // Bat screech sound clip
    [SerializeField] private float soundDistanceThreshold = 10f; // Distance within which the sound is audible

    private float timer;
    private Animator animator; // Reference to the Animator component
    private bool isStunned; // Flag to track if the bat is currently stunned
    private bool hasScreechPlayed = false; // Flag to track if the screech sound has been played
    private AudioSource audioSource; // AudioSource component

    [SerializeField] private float maxHealth = 20f; 
    private float currentHealth; 

    private enum BatState
    {
        Chasing,
        FlyingAround
    }
    private BatState batState;
    
    private Vector2 randomFlyTarget;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>(); // Get the Animator component
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned!");
        }

        currentHealth = maxHealth;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHealthBarVisible(true);
            UIManager.Instance.SetBossName("Bat");
            UIManager.Instance.UpdateBossHealthBar(currentHealth, maxHealth);
        }

        batState = BatState.Chasing; // Start with the chasing state
        StartCoroutine(StateCycle()); // Start the coroutine to cycle between states

        ChangeState(EnemyStates.Bat_Idle);
    }

    protected override void Update()
    {
        base.Update();
        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Bat_Idle);
        }
    }

    protected override void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        switch (currentEnemyState)
        {
            case EnemyStates.Bat_Idle:
                if (_dist < chaseDistance)
                {
                    ChangeState(EnemyStates.Bat_Chase);
                }
                break;

            case EnemyStates.Bat_Chase:
                if (!hasScreechPlayed && IsPlayerClose())
                {
                    PlayScreechSound(); // Play screech sound when chasing
                    hasScreechPlayed = true; // Ensure sound is played only once
                }

                if (batState == BatState.Chasing)
                {
                    Vector2 moveDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
                    float currentSpeed = (_dist > chaseDistance / 2f) ? Bat_ChaseSpeed : Bat_CloseSpeed; // Adjust speed based on distance

                    rb.velocity = moveDirection * currentSpeed;
                }
                else if (batState == BatState.FlyingAround)
                {
                    // Move towards random fly target
                    Vector2 moveDirection = (randomFlyTarget - (Vector2)transform.position).normalized;
                    rb.velocity = moveDirection * Bat_ChaseSpeed;

                    // Check if close to random target
                    if (Vector2.Distance(transform.position, randomFlyTarget) < 1f)
                    {
                        // Choose a new random target
                        ChooseRandomFlyTarget();
                    }
                }

                FlipBat();  // Ensure the bat is flipping correctly based on player position
                break;

            case EnemyStates.Bat_Stunned:
                if (!isStunned)
                {
                    PlayStunnedAnimation(); // Play stunned animation
                    isStunned = true;
                    timer = 0; // Reset timer when stunned animation starts
                }

                timer += Time.deltaTime;
                if (timer > stunDuration)
                {
                    ChangeState(EnemyStates.Bat_Idle);
                    isStunned = false;
                    timer = 0;
                }
                break;

            case EnemyStates.Bat_Death:
                StartCoroutine(HandleDeath()); // Start coroutine to handle death animation
                break;
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        
        // Reduce health and update health bar
        TakeDamage(_damageDone);

        // Only play stunned animation if health is greater than 0
        if (currentHealth > 0)
        {
            ChangeState(EnemyStates.Bat_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Bat_Death);
        }
    }

    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateBossHealthBar(currentHealth, maxHealth);
        }
    }

    private void FlipBat()
    {
        // Flip the bat based on player's position
        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            sr.flipX = false;  // Facing left
        }
        else
        {
            sr.flipX = true;   // Facing right
        }
    }

    private IEnumerator StateCycle()
    {
        while (true)
        {
            if (batState == BatState.Chasing)
            {
                yield return new WaitForSeconds(chaseDuration);
                batState = BatState.FlyingAround;
                ChooseRandomFlyTarget(); // Choose a random target when switching to flying
            }
            else if (batState == BatState.FlyingAround)
            {
                yield return new WaitForSeconds(flyDuration);
                batState = BatState.Chasing;
            }
        }
    }

    private void ChooseRandomFlyTarget()
    {
        // Choose a random target within a defined range
        float randomX = Random.Range(transform.position.x - 10f, transform.position.x + 10f);
        float randomY = Random.Range(transform.position.y - 10f, transform.position.y + 10f);
        randomFlyTarget = new Vector2(randomX, randomY);
    }

    private IEnumerator HandleDeath()
    {
        // Trigger the death animation
        animator.SetTrigger("Bat_Death");

        // Wait for the death animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Award souls to the player
        SaveData.Instance.money += soulsOnDeath;

        // Hide the boss health bar
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetBossHealthBarVisible(false);
        }

        // Destroy the game object after the animation finishes
        Destroy(gameObject);
    }

    private void PlayStunnedAnimation()
    {
        animator.SetTrigger("Bat_Stunned");
    }

    private bool IsPlayerClose()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        return distanceToPlayer <= soundDistanceThreshold;
    }

    private void PlayScreechSound()
    {
        if (audioSource != null && screechSound != null)
        {
            if (!audioSource.isPlaying) // Play only if not already playing
            {
                audioSource.PlayOneShot(screechSound); // Play the screech sound
            }
        }
    }
}
