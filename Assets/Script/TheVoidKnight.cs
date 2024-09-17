using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheVoidKnight : Enemy
{
    public static TheVoidKnight Instance;

    [SerializeField] private GameObject slashEffect;
    public Transform SideAttackTransform;
    public Vector2 SideAttackArea;

    public Transform UpAttackTransform;
    public Vector2 UpAttackArea;

    public Transform DownAttackTransform;
    public Vector2 DownAttackArea;

    public float attackRange;
    public float attackTimer = 2f;

    [HideInInspector] public bool facingRight;
    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    int hitCounter;
    bool stunned, canStun;
    [HideInInspector] public bool alive;

    [SerializeField] public float runSpeed;
    private new SpriteRenderer sr;
    public new Animator anim;
    private new Rigidbody2D rb;

    [SerializeField] private float maxHealth; // Adjust default value as needed
    [HideInInspector] public float health;

    private float attackCooldown = 2.5f;
    private float lastAttackTime = 1f;
    [HideInInspector] public float attackCountdown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyStates.THK_Stage1);
        alive = true;

        // Initialize health
        health = maxHealth;

        // Initialize the boss health bar and name
        UIManager.Instance.SetBossHealthBarVisible(true);
        UIManager.Instance.UpdateBossHealthBar(health, maxHealth);
        UIManager.Instance.SetBossName("Voidknight"); // Set boss name here
    }

    public bool Grounded()
    {
        return Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround);
    }

    protected override void Update()
    {
        base.Update();

        if (health <= 0 && alive)
        {
            Death();
        }
        
        if (!attacking)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

    public void Flip()
    {
        if (PlayerController.Instance.transform.position.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, 180);
            facingRight = false;
        }
        else
        {
            transform.eulerAngles = new Vector2(transform.eulerAngles.x, 0);
            facingRight = true;
        }
    }

    protected override void UpdateEnemyStates()
    {
        if (PlayerController.Instance != null)
        {
            switch (GetCurrentEnemyState())
            {
                case EnemyStates.THK_Stage1:
                    break;
                case EnemyStates.THK_Stage2:
                    break;
                case EnemyStates.THK_Stage3:
                    break;
                case EnemyStates.THK_Stage4:
                    break;
            }
        }
    }

    #region Attacking
    [HideInInspector] public bool attacking;
    [HideInInspector] public bool damagedPlayer = false;
    [HideInInspector] public bool parrying;
    [HideInInspector] public Vector2 moveToPosition;
    [HideInInspector] public bool diveAttack;
    public GameObject divingCollider;
    public GameObject pillar;

    [HideInInspector] public bool barrageAttack;
    public GameObject barrageFireball;

    public void AttackHandler()
    {
        if (Time.time >= lastAttackTime + attackCooldown || Vector2.Distance(PlayerController.Instance.transform.position, rb.position) <= attackRange)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    private IEnumerator PerformAttackSequence()
    {
        // Randomly select an attack type
        int attackType = Random.Range(0, 4); // Ensure range covers all 4 attacks

        Debug.Log("Selected Attack Type: " + attackType);

        switch (attackType)
        {
            case 0:
                yield return StartCoroutine(TripleSlash());
                break;
            case 1:
                yield return StartCoroutine(Lunge());
                break;
            case 2:
                yield return StartCoroutine(DiveAttackJumpCoroutine());
                break;
            case 3:
                yield return StartCoroutine(BarrageBendDownCoroutine());
                break;
        }

        lastAttackTime = Time.time;
        ResetAllAttacks();
    }

    private IEnumerator DiveAttackJumpCoroutine()
    {
        DiveAttackJump();
        while (diveAttack)
        {
            yield return null;
        }
    }

    private IEnumerator BarrageBendDownCoroutine()
    {
        BarrageBendDown();
        while (barrageAttack)
        {
            yield return null;
        }
    }

    public void ResetAllAttacks()
    {
        attacking = false;
        StopAllCoroutines();
        diveAttack = false;
        barrageAttack = false;
    }

    #endregion
    
    #region Stage 2
    void DiveAttackJump()
    {
        attacking = true;
        moveToPosition = new Vector2(PlayerController.Instance.transform.position.x, rb.position.y + 12);
        diveAttack = true;
        anim.SetBool("Jump", true);
    }

    public void Dive()
    {
        anim.SetBool("Dive", true);
        anim.SetBool("Jump", false);
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        PlayerController player = _other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage * 2);
            PlayerController.Instance.pState.recoilingX = true;
        }
    }

    public void DivingPillars()
    {
        Vector2 _impactPoint = groundCheckPoint.position;
        float _spawnDistance = 12;

        for (int i = 0; i < 10; i++)
        {
            Vector2 _pillarSpawnPointRight = _impactPoint + new Vector2(_spawnDistance, 0);
            Vector2 _pillarSpawnPointLeft = _impactPoint - new Vector2(_spawnDistance, 0);
            Instantiate(pillar, _pillarSpawnPointRight, Quaternion.Euler(0, 0, -90));
            Instantiate(pillar, _pillarSpawnPointLeft, Quaternion.Euler(0, 0, -90));

            _spawnDistance += 10;
        }
        ResetAllAttacks();
    }

    void BarrageBendDown()
    {
        attacking = true;
        rb.velocity = Vector2.zero;
        barrageAttack = true;
        anim.SetTrigger("BendDown");
    }

    public IEnumerator Barrage()
    {
        rb.velocity = Vector2.zero;

        float _currentAngle = 30f;
        for (int i = 0; i < 10; i++)
        {
            GameObject _projectile = Instantiate(barrageFireball, transform.position, Quaternion.Euler(0, 0, _currentAngle));
            if (facingRight)
            {
                _projectile.transform.eulerAngles = new Vector3(_projectile.transform.eulerAngles.x, 0, _currentAngle + 45);
            }
            else
            {
                _projectile.transform.eulerAngles = new Vector3(_projectile.transform.eulerAngles.x, 180, _currentAngle);
            }
            _currentAngle += 5f;

            yield return new WaitForSeconds(0.4f);
        }
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("Cast", false);
        ResetAllAttacks();
    }

    #endregion

    #region Stage 1

    IEnumerator TripleSlash()
    {
        attacking = true;
        rb.velocity = Vector2.zero;

        anim.SetTrigger("Slash");
        SlashAngle();
        yield return new WaitForSeconds(0.5f);
        anim.ResetTrigger("Slash");

        anim.SetTrigger("Slash");
        SlashAngle();
        yield return new WaitForSeconds(0.4f);
        anim.ResetTrigger("Slash");

        anim.SetTrigger("Slash");
        SlashAngle();
        yield return new WaitForSeconds(0.5f);
        anim.ResetTrigger("Slash");

        ResetAllAttacks();
    }
    
    void SlashAngle()
    {
        if (PlayerController.Instance.transform.position.x > transform.position.x || PlayerController.Instance.transform.position.x < transform.position.x)
        {
            Instantiate(slashEffect, SideAttackTransform);
        }
        else if (PlayerController.Instance.transform.position.y > transform.position.y)
        {
            SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
        }
        else if (PlayerController.Instance.transform.position.y < transform.position.y)
        {
            SlashEffectAtAngle(slashEffect, -80, DownAttackTransform);
        }
    }

    void SlashEffectAtAngle(GameObject _effect, float _angle, Transform _spawnTransform)
    {
        GameObject _slashEffect = Instantiate(_effect, _spawnTransform.position, Quaternion.Euler(0, 0, _angle));
        _slashEffect.transform.localScale = new Vector3(2f, 2f, 2f);
    }

    IEnumerator Lunge()
    {
        attacking = true;
        rb.velocity = Vector2.zero;

        anim.SetTrigger("Lunge");
        yield return new WaitForSeconds(0.1f);

        // Dash for a fixed distance
        Vector2 startPosition = rb.position;
        Vector2 targetPosition = startPosition + (facingRight ? Vector2.right : Vector2.left) * 5f; // Fixed distance of 5 units
        float dashDuration = 0.2f; // Time taken to dash
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            rb.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / dashDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.position = targetPosition; // Ensure it ends exactly at the target position

        yield return new WaitForSeconds(0.5f);
        anim.ResetTrigger("Lunge");

        ResetAllAttacks();
    }

    #endregion

    #region Death
    public void Death()
    {
        if (!alive) return; // Ensure we only process death once
        alive = false;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("Die");

        // Hide the boss health bar
        UIManager.Instance.SetBossHealthBarVisible(false);

        // Call DestroyAfterDeath method here
        StartCoroutine(DestroyAfterDeath());
    }

    public IEnumerator DestroyAfterDeath()
    {
        // Wait for 3 seconds to ensure the "Die" animation plays fully
        yield return new WaitForSeconds(3f); // Adjust the delay as needed
        SceneManager.LoadScene("Ending");
        Destroy(gameObject);
    }
    #endregion

    // Update the health bar when taking damage
public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
{
    base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    health -= _damageDone; // Update current health
    UIManager.Instance.UpdateBossHealthBar(health, maxHealth);
    
    // Check if health is less than or equal to 0
    if (health <= 0)
    {
        UIManager.Instance.SetBossHealthBarVisible(false); // Hide the health bar
    }
}}
