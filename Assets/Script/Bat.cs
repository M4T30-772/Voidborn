using System.Collections;
using UnityEngine;

public class Bat : Enemy
{
    [Header("Bat Settings")]
    [SerializeField] private float chaseDistance;
    [SerializeField] private float stunDuration;
    [SerializeField] private float Bat_ChaseSpeed;
    [SerializeField] private float Bat_CloseSpeed;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip screechSound; // Bat screech sound clip
    [SerializeField] private float soundDistanceThreshold = 10f; // Distance within which the sound is audible

    private float timer;
    private Animator animator; // Reference to the Animator component
    private bool isStunned; // Flag to track if the bat is currently stunned
    private bool hasScreechPlayed = false; // Flag to track if the screech sound has been played
    private AudioSource audioSource; // AudioSource component

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>(); // Get the Animator component
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned!");
        }

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
                Vector2 moveDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
                float currentSpeed = (_dist > chaseDistance / 2f) ? Bat_ChaseSpeed : Bat_CloseSpeed; // Adjust speed based on distance

                rb.velocity = moveDirection * currentSpeed;

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
        
        // Only play stunned animation if health is greater than 0
        if (health > 0)
        {
            ChangeState(EnemyStates.Bat_Stunned);
        }
        else
        {
            ChangeState(EnemyStates.Bat_Death);
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

    private IEnumerator HandleDeath()
    {
        // Trigger the death animation
        animator.SetTrigger("Bat_Death");

        // Wait for the death animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Award souls to the player
        SaveData.Instance.money += soulsOnDeath;

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
