using UnityEngine;

public class BossKagami : MonoBehaviour
{
    [Header("Boss Ayarları")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float detectionRange = 10f;
    
    [Header("Referanslar")]
    public Transform playerTarget;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    [Header("Durum")]
    public bool isAlive = true;
    public bool isAttacking = false;
    
    private Vector2 startPosition;
    private float lastAttackTime;
    public float attackCooldown = 2f;
    
    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        
        // Eğer player target atanmamışsa, otomatik bul
        if (playerTarget == null)
        {
            // Önce tag ile dene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            // Tag ile bulunamazsa, isimle dene
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            
            // Hala bulunamazsa, SenseiController script'ine sahip objeyi bul
            if (player == null)
            {
                SenseiController playerController = FindObjectOfType<SenseiController>();
                if (playerController != null)
                {
                    player = playerController.gameObject;
                }
            }
            
            if (player != null)
            {
                playerTarget = player.transform;
                Debug.Log($"✅ Boss: Player bulundu! ({player.name})");
            }
            else
            {
                Debug.LogWarning("⚠️ Boss: Player bulunamadı! Lütfen Player objesini Inspector'dan manuel olarak atayın.");
            }
        }
        else
        {
            Debug.Log($"✅ Boss: Player target atanmış ({playerTarget.name})");
        }
        
        // Animator'ı al (opsiyonel - animasyon olmadan da çalışır)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // SpriteRenderer'ı al
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Rigidbody2D'nin Y eksenindeki hareketini kısıtla (sadece X ekseninde hareket etsin)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }
        
        Debug.Log($"🎮 Boss başlatıldı! Detection Range: {detectionRange}, Attack Range: {attackRange}, Move Speed: {moveSpeed}");
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // Player'ı takip et ve saldır
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            
            // Debug bilgisi (sadece ilk frame'de veya değişiklik olduğunda)
            if (Time.frameCount % 60 == 0) // Her 60 frame'de bir (yaklaşık 1 saniyede bir)
            {
                Debug.Log($"📊 Boss - Player mesafesi: {distanceToPlayer:F2} (Detection: {detectionRange}, Attack: {attackRange})");
            }
            
            if (distanceToPlayer <= detectionRange)
            {
                // Player'a doğru bak
                FlipTowardsPlayer();
                
                if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
                else if (distanceToPlayer > attackRange)
                {
                    MoveTowardsPlayer();
                }
            }
        }
        else
        {
            // Player bulunamadıysa, tekrar dene
            if (Time.frameCount % 120 == 0) // Her 2 saniyede bir dene
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) player = GameObject.Find("Player");
                if (player != null)
                {
                    playerTarget = player.transform;
                    Debug.Log("✅ Boss: Player sonradan bulundu!");
                }
            }
        }
        
        // Animasyon parametrelerini güncelle (animator varsa)
        UpdateAnimations();
    }
    
    void MoveTowardsPlayer()
    {
        if (playerTarget == null || isAttacking) return;
        
        // Sadece X ekseninde hareket et, Y pozisyonunu sabit tut
        Vector3 targetPosition = new Vector3(playerTarget.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    
    void FlipTowardsPlayer()
    {
        if (playerTarget == null) return;
        
        if (playerTarget.position.x > transform.position.x && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (playerTarget.position.x < transform.position.x && transform.localScale.x > 0)
        {
            Flip();
        }
    }
    
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    void Attack()
    {
        if (isAttacking) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        
        Debug.Log("⚔️ Boss saldırıyor!");
        
        // Animator'da attack trigger'ı tetikle (animator varsa)
        if (animator != null && animator.enabled)
        {
            // Rastgele bir saldırı animasyonu seç
            int attackType = Random.Range(1, 4); // 1, 2, veya 3
            animator.SetTrigger($"Attack{attackType}");
        }
        
        // Saldırı hitbox'unu aktif et (eğer varsa)
        EnableAttackHitbox();
        
        // Saldırı sonrası isAttacking'i false yap (animasyon event'i ile de yapılabilir)
        Invoke("ResetAttack", 1f);
    }
    
    void EnableAttackHitbox()
    {
        // Attack hitbox'unu bul ve aktif et
        Transform attackHitbox = transform.Find("AttackHitbox");
        if (attackHitbox != null)
        {
            attackHitbox.gameObject.SetActive(true);
            Debug.Log("✅ Boss AttackHitbox aktif edildi!");
            // 0.5 saniye sonra kapat (saldırı animasyonu süresine göre ayarla)
            Invoke("DisableAttackHitbox", 0.5f);
        }
        else
        {
            Debug.LogWarning("⚠️ Boss AttackHitbox bulunamadı! Lütfen Boss'un child'ı olarak 'AttackHitbox' objesi oluşturun.");
        }
    }
    
    void DisableAttackHitbox()
    {
        Transform attackHitbox = transform.Find("AttackHitbox");
        if (attackHitbox != null)
        {
            attackHitbox.gameObject.SetActive(false);
            Debug.Log("Boss AttackHitbox kapatıldı.");
        }
    }
    
    void ResetAttack()
    {
        isAttacking = false;
    }
    
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        // HealthSystem kullanıyorsa onu kullan
        HealthSystem health = GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else
        {
            // Eski sistem (geriye dönük uyumluluk)
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        // Hurt animasyonu
        if (animator != null && animator.enabled)
        {
            animator.SetTrigger("Hurt");
        }
    }
    
    void Die()
    {
        isAlive = false;
        
        // Death animasyonu
        if (animator != null && animator.enabled)
        {
            animator.SetTrigger("Death");
        }
        
        // Collider'ı devre dışı bırak
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Rigidbody'yi devre dışı bırak
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        // Hareketi durdur
        moveSpeed = 0;
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Hareket durumunu animator'a bildir
        bool isMoving = playerTarget != null && 
                       Vector2.Distance(transform.position, playerTarget.position) > attackRange &&
                       !isAttacking;
        
        animator.SetBool("isMoving", isMoving);
    }
    
    // Gizmos ile görselleştirme (sahne görünümünde)
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

