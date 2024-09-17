using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UIElements;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    
    public bool teleportToVoid=false;
    public bool unlockedVoid = false;
    public Transform player; // Reference to the player's transform
    [Header("Horizontal Movement Settings:")]
    [SerializeField] private float walkSpeed = 1; //sets the players movement speed on the ground
    [Space(5)]

    [Header("Audio:")]
    [SerializeField] AudioClip landingSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip dashAndAttackSound;
    [SerializeField] AudioClip spellCastSound;
    [SerializeField] AudioClip hurtSound;
    [Space(5)]

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45f; //sets how hight the player can jump

    private int jumpBufferCounter = 0; //stores the jump button input
    [SerializeField] private int jumpBufferFrames; //sets the max amount of frames the jump buffer input is stored

    private float coyoteTimeCounter = 0; //stores the Grounded() bool
    [SerializeField] private float coyoteTime; //sets the max amount of frames the Grounded() bool is stored

    private int airJumpCounter = 0; //keeps track of how many times the player has jumped in the air
    [SerializeField] private int maxAirJumps; //the max no. of air jumps

    private float gravity; //stores the gravity scale at start
    [Space(5)]

    [Header("Wall Jump Settings")]
    [SerializeField] private float wallSlidingSpeed = 2;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallJumpingDuration;
    [SerializeField] private Vector2 wallJumpingPower;
    float wallJumpingDirection;
    bool isWallSliding;
    bool isWallJumping;
    [Space(5)]

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint; //point at which ground check happens
    [SerializeField] private float groundCheckY = 0.2f; //how far down from ground chekc point is Grounded() checked
    [SerializeField] private float groundCheckX = 0.5f; //how far horizontally from ground chekc point to the edge of the player is
    [SerializeField] private LayerMask whatIsGround; //sets the ground layer
    [Space(5)]



    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed; //speed of the dash
    [SerializeField] private float dashTime; //amount of time spent dashing
    [SerializeField] private float dashCooldown; //amount of time between dashes
    [SerializeField] GameObject dashEffect;
    private bool canDash = true, dashed;
    [Space(5)]



    [Header("Attack Settings:")]
    [SerializeField] private Transform SideAttackTransform; //the middle of the side attack area
    [SerializeField] private Vector2 SideAttackArea; //how large the area of side attack is

    [SerializeField] private Transform UpAttackTransform; //the middle of the up attack area
    [SerializeField] private Vector2 UpAttackArea; //how large the area of side attack is

    [SerializeField] private Transform DownAttackTransform; //the middle of the down attack area
    [SerializeField] private Vector2 DownAttackArea; //how large the area of down attack is

    [SerializeField] private LayerMask attackableLayer; //the layer the player can attack and recoil off of

    [SerializeField] private float timeBetweenAttack;
    private float timeSinceAttack;

    [SerializeField] public float damage = 1; //the damage the player does to an enemy

    [SerializeField] private GameObject slashEffect; //the effect of the slashs

    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]



    [Header("Recoil Settings:")]
    [SerializeField] private int recoilXSteps = 5; //how many FixedUpdates() the player recoils horizontally for
    [SerializeField] private int recoilYSteps = 5; //how many FixedUpdates() the player recoils vertically for

    [SerializeField] private float recoilXSpeed = 100; //the speed of horizontal recoil
    [SerializeField] private float recoilYSpeed = 100; //the speed of vertical recoil

    private int stepsXRecoiled, stepsYRecoiled; //the no. of steps recoiled horizontally and verticall
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    public int maxTotalHealth = 8;
    public int heartShards;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    float healTimer;
    [SerializeField] float timeToHeal;
    [Space(5)]

    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image manaStorage;

    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    [SerializeField] private GameObject healingEffectPrefab;
    [Space(5)]

    [Header("Spell Settings")]
    //spell stats
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    [SerializeField] public float spellDamage = 2; //upspellexplosion and downspellfireball
    [SerializeField] float downSpellForce; // desolate dive only
    //spell cast objects
    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;
    float castOrHealTimer;
    [Space(5)]


    [HideInInspector] public PlayerStateList pState;
    private Animator anim;
    public Rigidbody2D rb { get; private set; }
    private SpriteRenderer sr;
    private AudioSource audioSource;

    //Input Variables
    private float xAxis, yAxis;
    private bool attack = false;
    bool openMap;
    bool openInventory;

    //creates a singleton of the PlayerController
    public static PlayerController Instance;

    // unlocking abilities
    public bool unlockedWallJump;
    public bool unlockedDash;
    public bool unlockedVarJump;
    public bool unlockedSideCast;
    public bool unlockedUpCast;
    public bool unlockedDownCast;

    private bool landingSoundPlayed;

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
        DontDestroyOnLoad(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        SaveData.Instance.LoadPlayerData();

        FindObjectOfType<HeartController>().InstantiateHeartContainers();

        gravity = rb.gravityScale;

        Health = maxHealth;
        Mana = mana;
        manaStorage.fillAmount = Mana;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update is called once per frame
void Update()
{
    if (GameManager.Instance.gameIsPaused) return;
    if(Input.GetKeyDown(KeyCode.O)){
        SaveData.Instance.SavePlayerData();
    }
    if (pState.cutscene) return;
    if (pState.alive)
    {
        if(unlockedWallJump)
        {
        WallSlide();
        WallJump();
        }
        if(unlockedVoid){
            HandleTeleportation();
        }
        if(unlockedDash)
        {
            StartDash();
        }
        if(!isWallJumping)
        {
            Flip();
            Jump();
            Move();
        }

        GetInputs();
        ToggleMap();
        ToggleInventory();
                if (Input.GetKeyDown(KeyCode.Q) && teleportToVoid)
        {
            TeleportToVoid();
        }

    }

    UpdateJumpVariables();

    if (pState.dashing) return;
    RestoreTimeScale();
    FlashWhileInvincible();
    Move();
    Heal();
    CastSpell();
    if (pState.healing) return;


    Attack();
}

void ToggleMap(){
    if (Input.GetButton("Inventory")) {
        UIManager.Instance.inventory.SetActive(true);
    } else {
        UIManager.Instance.inventory.SetActive(false);
    }
}

void ToggleInventory(){
    if (Input.GetButton("Map")) {
        UIManager.Instance.mapHandler.SetActive(true);
    } else {
        UIManager.Instance.mapHandler.SetActive(false);
    }
}

    private void OnTriggerEnter2D(Collider2D _other) //for up and down cast spell
    {
        if(_other.GetComponent<Enemy>() != null && pState.casting)
        {
            _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (pState.dashing || pState.healing || pState.cutscene) return;
        Recoil();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        openMap = Input.GetButtonDown("Map");
        openInventory = Input.GetButtonDown("Inventory");
        if (Input.GetButton("Cast/Heal"))
        {
            castOrHealTimer += Time.deltaTime;
        }
        else
        {
            castOrHealTimer = 0;
        }

        
    }

    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-4, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(4, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    private void Move()
    {
        if (pState.healing) rb.velocity = new Vector2(0, 0);
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

void StartDash()
{
    if (Input.GetButtonDown("Dash") && canDash && !dashed)
    {
        StartCoroutine(Dash());
        dashed = true;
    }

    if (Grounded())
    {
        dashed = false;
    }
}

IEnumerator Dash()
{
    canDash = false;
    pState.dashing = true;
    pState.invincible = true; 
    anim.SetTrigger("Dashing");
    audioSource.PlayOneShot(dashAndAttackSound);
    rb.gravityScale = 0;
    int _dir = pState.lookingRight ? 1 : -1;
    rb.velocity = new Vector2(_dir * 90, 0);
    
    if (Grounded())
    {
        Instantiate(dashEffect, transform);
    }
    float dashStartTime = Time.time;
    while (Time.time < dashStartTime + dashTime)
    {
        rb.velocity = new Vector2(_dir * 90, rb.velocity.y);
        yield return null; 
    }

    rb.gravityScale = gravity;
    rb.velocity = Vector2.zero; 
    pState.dashing = false;
    pState.invincible = false; 
    anim.SetTrigger("Idle"); 

    yield return new WaitForSeconds(dashCooldown);
    canDash = true;
}
    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        pState.invincible = true;
        if(_exitDir.y > 0)
        {
            rb.velocity = jumpForce * _exitDir;
        }
        if(_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;
            Move();
        }

        Flip();
        yield return new WaitForSeconds(_delay);
        pState.invincible = false;
        pState.cutscene = false;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            int _recoilLeftOrRight = pState.lookingRight ? 1 : -1;
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
            audioSource.PlayOneShot(dashAndAttackSound);

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * _recoilLeftOrRight, recoilXSpeed);

                Instantiate(slashEffect, SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
            }
        }


    }
void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilBool, Vector2 _recoilDir, float _recoilStrength)
{
    Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
    List<Enemy> hitEnemies = new List<Enemy>();

    if (objectsToHit.Length > 0)
    {
        _recoilBool = true;  // Corrected this line
    } 
    for (int i = 0; i < objectsToHit.Length; i++)
    {
        Enemy e = objectsToHit[i].GetComponent<Enemy>();
        if (e && !hitEnemies.Contains(e))
        {
            e.EnemyHit(damage, _recoilDir, _recoilStrength);
            hitEnemies.Add(e);  // Corrected this line

            if (objectsToHit[i].CompareTag("Enemy"))
            {
                Mana += manaGain;
            }
        }
    }
}


    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }
    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //stop recoil
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }
    public void TakeDamage(float _damage)
    {
        if (pState.alive)
        {
                if (!pState.alive) return; 
            StopCoroutine("StopTakingDamage");
            audioSource.PlayOneShot(hurtSound);
            Health -= Mathf.RoundToInt(_damage);

            if (Health <= 0)
            {
                Health = 0;
                StartCoroutine(Death());
            }
            else
            {
                StartCoroutine("StopTakingDamage");
            }
        }
    }

 private IEnumerator StopTakingDamage()
    {
            if (!pState.alive) yield break; 
        pState.invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);

        anim.SetTrigger("TakeDamage");

        yield return new WaitForSeconds(0.25f);

        anim.ResetTrigger("TakeDamage");
        pState.invincible = false;
    }

    private void FlashWhileInvincible()
    {
    if (pState.cutscene || !pState.alive) return; 

        if (pState.invincible)
        {
            sr.material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f));
        }
        else
        {
            sr.material.color = Color.white;
        }
    }

    private void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
                if (Time.timeScale > 1) Time.timeScale = 1;
            }
            else
            {
                restoreTime = false;
            }
        }
    }

    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;

        if (_delay > 0)
        {
            StopCoroutine("StartTimeAgain");
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }

        Time.timeScale = _newTimeScale;
    }

    private IEnumerator StartTimeAgain(float _delay)
    {
        yield return new WaitForSecondsRealtime(_delay);
        restoreTime = true;
    }

    public IEnumerator Death()
    {
        pState.alive = false;
        Time.timeScale = 1;

        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1f);

        anim.SetTrigger("Death");

        yield return new WaitForSeconds(0.9f);
        UIManager.Instance.ShowDeathScreen();

    }
    public void Respawn()
    {
        if(!pState.alive){
            pState.alive = true;
            Health = maxHealth;
            anim.Play("Player_Idle");
        }

StartCoroutine(UIManager.Instance.DeactivateDeathScreen());

    }


    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

void Heal()
{
    if (Input.GetButton("Cast/Heal") && castOrHealTimer > 0.05f && Health < maxHealth && Mana > 0 && Grounded() && !pState.dashing)
    {
        pState.healing = true;
        anim.SetBool("Healing", true);

        // Instantiate the healing effect at the player's position
        if (healingEffectPrefab != null)
        {
            Instantiate(healingEffectPrefab, transform.position, Quaternion.identity);
        }

        // Healing
        healTimer += Time.deltaTime;
        if (healTimer >= timeToHeal)
        {
            Health++;
            healTimer = 0;
        }

        // Drain mana
        Mana -= Time.deltaTime * manaDrainSpeed;
    }
    else
    {
        pState.healing = false;
        anim.SetBool("Healing", false);
        healTimer = 0;
    }
}

public float Mana
{
    get { return mana; }
    set
    {
        //if mana stats change
        if (mana != value)
        {
            mana = Mathf.Clamp(value, 0, 1);
            manaStorage.fillAmount = Mana;
        }
    }
}

    void CastSpell()
    {
        if (Input.GetButtonUp("Cast/Heal") && castOrHealTimer <= 0.05f && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }

        if(Grounded())
        {
            downSpellFireball.SetActive(false);
        }
        if(downSpellFireball.activeInHierarchy)
        {
            rb.velocity += downSpellForce * Vector2.down;
        }
    }
    IEnumerator CastCoroutine()
    {
        audioSource.PlayOneShot(spellCastSound);
        if ((yAxis == 0 || (yAxis < 0 && Grounded())) && unlockedSideCast)
        {
            anim.SetBool("Casting", true);
            yield return new WaitForSeconds(0.15f);
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            if(pState.lookingRight)
            {
                _fireBall.transform.eulerAngles = Vector3.zero; 
            }
            else
            {
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180); 

            }
            pState.recoilingX = true;
            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.35f);
        }

        else if( yAxis > 0 && unlockedUpCast)
        {
            anim.SetBool("Casting", true);
            yield return new WaitForSeconds(0.15f);
            Instantiate(upSpellExplosion, transform);
            rb.velocity = Vector2.zero;
            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.35f);
        }

else if (yAxis < 0 && !Grounded() && unlockedDownCast)
{

    anim.SetBool("Casting", true);

    yield return new WaitForSeconds(0.15f);


    if (downSpellFireball != null)
    {
        // Activate the downSpellFireball
        downSpellFireball.SetActive(true);

        MeshRenderer renderer = downSpellFireball.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogWarning("MeshRenderer component is missing from downSpellFireball.");
        }
    }
    else
    {
        Debug.LogError("downSpellFireball is not assigned in the Inspector.");
    }

    Mana -= manaSpellCost;


    yield return new WaitForSeconds(0.35f);
}
        anim.SetBool("Casting", false);
        pState.casting = false;
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

void Jump()
{
    if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping)
    {
        audioSource.PlayOneShot(jumpSound);
        rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        pState.jumping = true;
    }
    if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
    {
        audioSource.PlayOneShot(jumpSound);
        if (unlockedVarJump)
        {
            pState.jumping = true;
            airJumpCounter++;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }
    }

    if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
    {
        pState.jumping = false;
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }
    
    anim.SetBool("Jumping", !Grounded());
}

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            if(!landingSoundPlayed)
            {
                audioSource.PlayOneShot(landingSound);
                landingSoundPlayed = true;
            }
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            landingSoundPlayed = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
    private bool Walled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

void WallSlide()
{
    if (Walled() && !Grounded() && xAxis != 0)
    {
        isWallSliding = true;
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
    }
    else
    {
        isWallSliding = false;
    }
}

void WallJump()
{
    if (isWallSliding)
    {
        isWallJumping = false;
        wallJumpingDirection = !pState.lookingRight ? 1 : -1;

        CancelInvoke(nameof(StopWallJump));
    }
    if (Input.GetButtonDown("Jump") && isWallSliding)
    {
        isWallJumping = true;
        rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
        dashed = false;  
        airJumpCounter = 0;

        pState.lookingRight=!pState.lookingRight;
        transform.eulerAngles = new Vector2(transform.eulerAngles.x, 180);

        Invoke(nameof(StopWallJump), wallJumpingDuration);
    }
}

void StopWallJump()
{
    isWallJumping = false;
    transform.eulerAngles = new Vector2(transform.eulerAngles.x, 0);
}
private void HandleTeleportation()
    {

        if (unlockedVoid && teleportToVoid && Input.GetKeyDown(KeyCode.Q))
        {

            Debug.Log("Teleportation triggered");

            if (player != null)
            {

                Debug.Log($"Current Position: {player.position}");

                Vector3 targetPosition = new Vector3(-30f, -200f, player.position.z); 
                player.position = targetPosition;
                Debug.Log($"New Position: {player.position}");
            }
            else
            {
                Debug.LogWarning("Player transform is not assigned.");
            }
        }
    }



    private void TeleportToVoid()
    {
        SceneManager.LoadScene("Void_1");
    }
}