using System.Collections;
using UnityEngine;

/// <summary>
/// Basit yapay zekâya sahip, zayıf küçük ninja NPC.
/// Player'ı görürse yaklaşır, menzile girince tek saldırı yapar.
/// Kensin animasyon state isimleri Animator Controller'daki adlarla eşleşir.
/// </summary>
public class SmallNinja : MonoBehaviour
{
    [Header("Can")]
    public float maxHealth = 25f;

    [Header("Hareket ve Takip")]
    public float moveSpeed = 2.2f;
    public float detectionRange = 8f;
    public float attackRange = 1.4f;

    [Header("Saldırı")]
    public float attackCooldown = 1.1f;
    public float attackActiveTime = 0.35f;
    public float contactDamage = 5f;

    [Header("Animasyon State İsimleri")]
    public string idleState = "IDLE";
    public string runState = "koşma";
    public string attackState = "saldırı";
    public string hurtState = "hasar_alma";
    public string deathState = "ölme";

    [Header("Referanslar")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform playerTarget;
    public Transform attackHitbox;

    private HealthSystem healthSystem;
    private DamageDealer damageDealer;
    private Rigidbody2D rb;
    private float nextAttackTime;
    private bool isAttacking;
    private bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = animator ?? GetComponent<Animator>();
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();

        if (attackHitbox == null)
        {
            attackHitbox = transform.Find("AttackHitbox");
        }
        if (attackHitbox != null)
        {
            attackHitbox.gameObject.SetActive(false);
            damageDealer = attackHitbox.GetComponent<DamageDealer>();
            if (damageDealer != null)
            {
                damageDealer.damage = contactDamage;
                damageDealer.targetTags = new[] { "Player" };
            }
        }
    }

    void Start()
    {
        if (healthSystem != null)
        {
            healthSystem.maxHealth = maxHealth;
            healthSystem.currentHealth = maxHealth;
            healthSystem.OnDeath.AddListener(OnDeath);
            healthSystem.OnDamageTaken.AddListener(OnHurt);
        }

        if (playerTarget == null)
        {
            FindPlayer();
        }
    }

    void Update()
    {
        if (isDead) return;

        if (playerTarget == null)
        {
            if (Time.frameCount % 60 == 0)
            {
                FindPlayer();
            }
            PlayIdle();
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTarget.position);
        FlipTowardsPlayer();

        if (!isAttacking && distance <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        if (!isAttacking && distance <= detectionRange)
        {
            MoveTowardsPlayer();
            PlayRun();
        }
        else
        {
            PlayIdle();
        }
    }

    void MoveTowardsPlayer()
    {
        if (playerTarget == null || rb == null) return;

        Vector2 target = new Vector2(playerTarget.position.x, transform.position.y);
        Vector2 newPos = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        PlayAttack();
        ToggleHitbox(true);
        yield return new WaitForSeconds(attackActiveTime);
        ToggleHitbox(false);

        // Kalan cooldown süresini bekle
        float remaining = Mathf.Max(0f, nextAttackTime - Time.time);
        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
        }

        isAttacking = false;
    }

    void ToggleHitbox(bool state)
    {
        if (attackHitbox != null)
        {
            attackHitbox.gameObject.SetActive(state);
        }
    }

    void OnHurt(float dmg)
    {
        if (isDead) return;
        PlayState(hurtState);
    }

    void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        ToggleHitbox(false);
        PlayState(deathState);

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.simulated = false;
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    void FlipTowardsPlayer()
    {
        if (playerTarget == null) return;
        float dir = playerTarget.position.x - transform.position.x;
        Vector3 scale = transform.localScale;
        if (dir > 0 && scale.x < 0)
        {
            scale.x *= -1;
            transform.localScale = scale;
        }
        else if (dir < 0 && scale.x > 0)
        {
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void PlayIdle() => PlayState(idleState);
    void PlayRun() => PlayState(runState);
    void PlayAttack() => PlayState(attackState);

    void PlayState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return;
        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        if (!current.IsName(stateName))
        {
            animator.CrossFade(stateName, 0.05f, 0);
        }
    }
}

