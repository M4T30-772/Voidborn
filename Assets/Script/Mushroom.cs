using System.Collections;
using UnityEngine;

public class Mushroom : Enemy
{
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float flipWaitTime;
    [SerializeField] private float ledgeCheckX;
    [SerializeField] private float ledgeCheckY;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float audioRange = 10f; // New field for visualizing audio range

    private Animator m_animator;
    private bool movingRight = true;
    private bool isHurt = false;
    private bool isRunning = false;

    private const string AnimState_Run = "Run";
    private const string AnimState_Hurt = "Hurt";

    protected override void Start()
    {
        base.Start();
        m_animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        ChangeState(EnemyStates.Mushroom_Run);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        Debug.Log("Mushroom started. Current state: Run");

        // Set the AudioSource to 2D sound and configure spatial blend
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Linear rolloff mode
        audioSource.minDistance = 1f; // Minimum distance to start attenuating
        audioSource.maxDistance = audioRange; // Maximum distance to stop attenuating
        audioSource.dopplerLevel = 0f; // No doppler effect
    }

    protected override void UpdateEnemyStates()
    {
        Vector2 groundCheckPos = new Vector2(transform.position.x + (movingRight ? ledgeCheckX : -ledgeCheckX), transform.position.y - ledgeCheckY);
        bool isGroundAhead = Physics2D.Raycast(groundCheckPos, Vector2.down, 0.1f, whatIsGround);

        if (health <= 0 && currentEnemyState != EnemyStates.Mushroom_Die)
        {
            ChangeState(EnemyStates.Mushroom_Die);
            StartCoroutine(MushroomHandleDeath());
            Debug.Log("Mushroom died.");
            return;
        }

        if (isHurt) return;

        switch (currentEnemyState)
        {
            case EnemyStates.Mushroom_Run:
                if (isGroundAhead)
                {
                    MushroomPatrol();
                    m_animator.SetBool(AnimState_Run, true);
                    m_animator.SetBool(AnimState_Hurt, false);
                    if (!isRunning)
                    {
                        PlaySound(runSound);
                        isRunning = true;
                    }
                    Debug.Log("Mushroom is running.");
                }
                else
                {
                    StartCoroutine(Flip());
                    Debug.Log("Mushroom is flipping.");
                }
                break;

            case EnemyStates.Mushroom_Hurt:
                m_animator.SetBool(AnimState_Run, false);
                m_animator.SetBool(AnimState_Hurt, true);
                PlaySound(hurtSound);
                Debug.Log("Mushroom is hurt.");
                break;

            case EnemyStates.Mushroom_Die:
                m_animator.SetTrigger("Death");
                Debug.Log("Mushroom is playing death animation.");
                break;
        }
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce); // Call the base method to handle common logic

        // Now handle Mushroom-specific hurt logic
        if (health > 0 && !isHurt)
        {
            HandleHurt();
        }
    }

    public void HandleHurt()
    {
        if (currentEnemyState != EnemyStates.Mushroom_Hurt)
        {
            ChangeState(EnemyStates.Mushroom_Hurt);
            isHurt = true;
            StartCoroutine(MushroomHandleHurt());
            Debug.Log("HandleHurt called. Mushroom is hurt.");
        }
    }

    private IEnumerator MushroomHandleHurt()
    {
        Debug.Log("MushroomHandleHurt called.");
        Debug.Log("Mushroom is hit. Health: " + health);

        m_animator.SetBool(AnimState_Hurt, true);
        yield return new WaitForSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length);

        isHurt = false;
        m_animator.SetBool(AnimState_Hurt, false);

        ChangeState(EnemyStates.Mushroom_Run);
        m_animator.SetBool(AnimState_Run, true);
        isRunning = true;
        Debug.Log("Mushroom recovered from hurt and is running again.");
    }

    private IEnumerator MushroomHandleDeath()
    {
        m_animator.SetTrigger("Death");
        yield return new WaitForSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length);
        SaveData.Instance.money += soulsOnDeath;
        Destroy(gameObject);
    }

    private void MushroomPatrol()
    {
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
    }

    private IEnumerator Flip()
    {
        yield return new WaitForSeconds(flipWaitTime);
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        isRunning = false; // Stop running sound when flipping
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a sphere representing the audio range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, audioRange);
    }
}
