using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] public float speed;
    [SerializeField] public float damage;

    [SerializeField] protected int soulsOnDeath; 

    protected float recoilTimer;
    protected new Rigidbody2D rb;
    protected new SpriteRenderer sr;
    protected AudioSource audioSource;

    [SerializeField] AudioClip hurtSound;

    protected enum EnemyStates
    {
        Slime_Idle,
        Slime_Flip,
        Slime_Death,
        Bat_Idle,
        Bat_Chase,
        Bat_Stunned,
        Bat_Death,
        Crow_Idle,
        Crow_Attack,
        Crow_Death,
        Crow_Hurt,
        Crow_Walk,
        Skeleton_Idle,
        Skeleton_Attack,
        Skeleton_Hurt,
        Skeleton_Walk,
        Skeleton_Chase,
        Skeleton_Death,
        Necromancer_Fireball,
        Necromancer_Summon,
        Necromancer_Dash,
        Necromancer_Death,
        Necromancer_Idle,
        Mushroom_Idle,
        Mushroom_Run,
        Mushroom_Attack,
        Mushroom_Hurt,
        Mushroom_Die,
        Mushroom_Flip,
        Bandit_Idle,
        Bandit_Run,
        Bandit_Attack,
        Bandit_Hurt,
        Bandit_Death,
        Bandit_Chase,
        Ninja_Attack,
        Ninja_Death,
        Ninja_Hurt,
        Ninja_Idle,
        Ninja_Walking,
        Ninja_Chase,
        Darkwolf_Idle,
        Darkwolf_Attack,
        Darkwolf_Hurt,
        Darkwolf_Chase,
        Darkwolf_Death,
        THK_Stage1,
        THK_Stage2,
        THK_Stage3,
        THK_Stage4,
        HeroKnight_Attack,
        HeroKnight_Chase,
        HeroKnight_Death,
        HeroKnight_Hurt,
        HeroKnight_Idle,
    }

    protected EnemyStates currentEnemyState;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }
    protected virtual void Update()
    {
        if (GameManager.Instance.gameIsPaused) return;

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            audioSource.PlayOneShot(hurtSound);
            rb.velocity = _hitForce * recoilFactor * _hitDirection;
            isRecoiling = true;
            
        }

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        SaveData.Instance.money += soulsOnDeath;

        ChangeState(EnemyStates.Bandit_Death); 
        Destroy(gameObject); 
    }

    protected virtual void UpdateEnemyStates()
    {
    }

    protected EnemyStates GetCurrentEnemyState()
    {
        return currentEnemyState;
    }

    protected void ChangeState(EnemyStates _newState)
    {
        currentEnemyState = _newState;
    }

    protected virtual void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D _other)
    {
        if (_other.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}
