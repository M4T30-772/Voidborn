using System.Collections;
using UnityEngine;

public class Necromancer : Enemy
{
    [SerializeField] private float maxHealth; // Maximum health of the Necromancer
    [HideInInspector] public float health; // Current health of the Necromancer

    [SerializeField] private float rangeDistance;
    [SerializeField] private float fireballCooldown;
    [SerializeField] private float summonCooldown;
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashDistance; // Distance to dash away from player
    [SerializeField] private GameObject minionPrefab; // Prefab to summon
    [SerializeField] private GameObject fireballPrefab; // Fireball prefab to instantiate

    private float fireballTimer;
    private float summonTimer;
    private float dashTimer;
    private bool isPerformingAction = false; // Flag to check if an action is currently being performed

    private Animator animator; // Reference to the Animator component
    private SpriteRenderer spriteRenderer; // Renamed field to avoid conflict

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>(); // Get the Animator component
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        ChangeState(EnemyStates.Necromancer_Idle);

        // Initialize health
        health = maxHealth;

        // Initialize the boss health bar
        UIManager.Instance.SetBossHealthBarVisible(true);
        UIManager.Instance.UpdateBossHealthBar(health, maxHealth);
        UIManager.Instance.SetBossName("Necromancer");
    }

    protected override void Update()
    {
        base.Update();

        if (!PlayerController.Instance.pState.alive)
        {
            ChangeState(EnemyStates.Necromancer_Idle);
            return;
        }

        // Handle timers for abilities
        fireballTimer += Time.deltaTime;
        summonTimer += Time.deltaTime;
        dashTimer += Time.deltaTime;

        // Flip to face the player
        FlipTowardsPlayer();
    }

    protected override void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        switch (currentEnemyState)
        {
            case EnemyStates.Necromancer_Idle:
                if (_dist < rangeDistance)
                {
                    HandleAbilities(); // Handle abilities when within range
                }
                break;

            case EnemyStates.Necromancer_Death:
                StartCoroutine(HandleDeath()); // Start coroutine to handle death animation
                break;
        }
    }

    private void HandleAbilities()
    {
        float _dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        if (isPerformingAction) return; // Do not perform another action if one is already in progress

        // Fireball ability
        if (fireballTimer >= fireballCooldown && _dist < rangeDistance)
        {
            StartCoroutine(PerformFireball());
            fireballTimer = 0; // Reset fireball timer
        }

        // Summon ability
        if (summonTimer >= summonCooldown && _dist < rangeDistance)
        {
            SummonMinions();
            summonTimer = 0; // Reset summon timer
        }

        // Dash ability
        if (dashTimer >= dashCooldown && _dist < rangeDistance)
        {
            StartCoroutine(PerformDashAwayFromPlayer());
            dashTimer = 0; // Reset dash timer
        }
    }

    private IEnumerator PerformFireball()
    {
        isPerformingAction = true; // Set flag to true
        animator.SetTrigger("Necromancer_Fireball");

        // Wait for fireball animation to finish if needed
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        CastFireball(); // Cast the fireball

        isPerformingAction = false; // Reset flag
        ChangeState(EnemyStates.Necromancer_Idle); // Return to idle state
    }

    private void CastFireball()
    {
        // Instantiate the fireball
        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);

        // Set the fireball direction
        Vector2 fireballDirection = (PlayerController.Instance.transform.position - transform.position).normalized;
        Fireball fireballScript = fireball.GetComponent<Fireball>();

        if (fireballScript != null)
        {
            fireballScript.Initialize(fireballDirection); // Initialize with direction
        }
    }

    private void SummonMinions()
    {
        // Trigger summon animation and create minions
        animator.SetTrigger("Necromancer_Summon");

        // Summon minions in front of the Necromancer
        Vector3 spawnPosition = transform.position + (spriteRenderer.flipX ? Vector3.left : Vector3.right) * 2f; // Adjust the offset as needed
        Instantiate(minionPrefab, spawnPosition, Quaternion.identity);

        ChangeState(EnemyStates.Necromancer_Idle); // Return to idle state after summoning
    }

    private IEnumerator PerformDashAwayFromPlayer()
    {
        isPerformingAction = true; // Set flag to true
        animator.SetTrigger("Necromancer_Dash");

        Vector3 directionAwayFromPlayer = (transform.position - PlayerController.Instance.transform.position).normalized;

        // Restrict the dash direction to horizontal only
        if (Mathf.Abs(directionAwayFromPlayer.x) > Mathf.Abs(directionAwayFromPlayer.y))
        {
            directionAwayFromPlayer.y = 0; // Zero out vertical movement
        }
        else
        {
            directionAwayFromPlayer.x = spriteRenderer.flipX ? -1 : 1; // Dash left or right based on flip
            directionAwayFromPlayer.y = 0; // Ensure vertical movement is zero
        }

        Vector3 dashPosition = transform.position + directionAwayFromPlayer * dashDistance;

        yield return StartCoroutine(MoveToPosition(dashPosition));

        isPerformingAction = false; // Reset flag
        ChangeState(EnemyStates.Necromancer_Idle); // Return to idle state after dashing
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        float dashDuration = 0.5f; // Adjust this value based on your dash animation length
        Vector3 startPosition = transform.position;

        while (elapsedTime < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / dashDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure it reaches the exact target position
    }

    private IEnumerator HandleDeath()
    {
        // Trigger the death animation
        animator.SetTrigger("Necromancer_Death");

        // Wait for the death animation to finish
        yield return new WaitForSeconds(0.6f); // Hardcoded value, adjust if necessary

        // Destroy the game object after the animation finishes
        Destroy(gameObject);
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);

        health -= _damageDone; // Update current health
        UIManager.Instance.UpdateBossHealthBar(health, maxHealth);

        // Handle death directly if health is 0 or below
        if (health <= 0)
        {
            UIManager.Instance.SetBossHealthBarVisible(false); // Hide the health bar
            ChangeState(EnemyStates.Necromancer_Death);
        }
    }

    private void FlipTowardsPlayer()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;  // Flip to face left
        }
        else
        {
            spriteRenderer.flipX = false; // Flip to face right
        }
    }
}
