using System.Collections;
using UnityEngine;

public class Slime : Enemy
{
    [Header("Slime Settings")]
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound; // Walking sound clip
    [SerializeField] private AudioClip deathSound; // Death sound clip
    [SerializeField] private float soundDistanceThreshold = 10f; // Distance within which the sound is audible

    private AudioSource audioSource; // AudioSource component
    private Animator animator; // Reference to the Animator component
    private Transform playerTransform; // Reference to the player's transform

    private float timer;
    private bool isWalking; // To track if the enemy is walking

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 12f;
        animator = GetComponent<Animator>(); // Get the Animator component

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned!");
        }

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found!");
        }
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0 && currentEnemyState != EnemyStates.Slime_Death)
        {
            ChangeState(EnemyStates.Slime_Death);
            return; // Ensure we return after changing state
        }

        switch (currentEnemyState)
        {
            case EnemyStates.Slime_Idle:
                // Calculate ledge check start position based on slime size and direction
                Vector3 ledgeCheckStart = transform.position + (transform.localScale.x > 0 ? new Vector3(ledgeCheckX, -1.5f) : new Vector3(-ledgeCheckX, -1.5f));
                Vector2 wallCheckDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

                // Check if there's ground ahead or if there's a wall
                if (!Physics2D.Raycast(ledgeCheckStart, Vector2.down, ledgeCheckY, whatIsGround) ||
                    Physics2D.Raycast(transform.position, wallCheckDir, ledgeCheckX, whatIsGround))
                {
                    ChangeState(EnemyStates.Slime_Flip);
                }

                // Move in the current direction
                rb.velocity = new Vector2(speed * (transform.localScale.x > 0 ? 1 : -1), rb.velocity.y);
                if (!isWalking)
                {
                    ManageWalkingSound();
                    isWalking = true;
                }
                break;

            case EnemyStates.Slime_Flip:
                timer += Time.deltaTime;
                if (timer >= flipWaitTime)
                {
                    timer = 0;
                    // Flip the slime's scale to change direction
                    transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                    ChangeState(EnemyStates.Slime_Idle);
                    StopWalkingSound(); // Stop walking sound when flipping
                }
                break;

            case EnemyStates.Slime_Death:
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Slime_Death"))
                {
                    // Trigger the death animation if not already playing
                    animator.SetTrigger("Slime_Death");
                    PlayDeathSound(); // Play death sound
                    StartCoroutine(HandleDeath());
                }
                break;
        }
    }

    private IEnumerator HandleDeath()
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Destroy the game object after the animation finishes
        Destroy(gameObject);
    }

    private void ManageWalkingSound()
    {
        if (IsPlayerClose())
        {
            PlayWalkingSound();
        }
        else
        {
            StopWalkingSound();
        }
    }

    private bool IsPlayerClose()
    {
        if (playerTransform == null)
        {
            return false;
        }
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= soundDistanceThreshold;
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

    private void PlayDeathSound()
    {
        if (audioSource != null && deathSound != null && IsPlayerClose())
        {
            audioSource.PlayOneShot(deathSound); // Play death sound
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);

        if (health <= 0 && currentEnemyState != EnemyStates.Slime_Death)
        {
            ChangeState(EnemyStates.Slime_Death);
            StartCoroutine(HandleDeath());
        }
    }
}
