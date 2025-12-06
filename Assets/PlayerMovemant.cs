using UnityEngine;
using UnityEngine.InputSystem; 

public class SenseiController : MonoBehaviour
{
    // --- Ayarlar (Inspector'da Görünür) ---
    [Header("Hareket Ayarları")]
    public float walkSpeed = 8f; 
    public float runSpeed = 12f;

    [Header("Saldırı Ayarları")]
    public string attack1TriggerName = "Attack1";
    public string attack2TriggerName = "Attack2";
    public string attack3TriggerName = "Attack3";
    public string airAttackTriggerName = "AirAttack";
    public float comboResetTime = 0.8f;
    
    [Header("Hasar Animasyonu")]
    public string hurtTriggerName = "Hurt"; // Animator'daki Hurt trigger parametresi
    
    [Header("Zıplama Ayarları")]
    public float jumpForce = 12f; 
    public float groundCheckRadius = 0.2f; // GroundCheck sensörünün yarıçapı
    public LayerMask groundLayer; // Zeminin katmanı
    public Transform groundCheck; // Ayak altındaki boş obje referansı
    
    
    // --- Özel Bileşenler ---
    private Rigidbody2D rb;
    private Animator anim;
    
    // --- Durum Değişkenleri ---
    private Vector2 currentMoveInput; // Klavye/Gamepad'den gelen girdi
    private bool isGrounded; // Yerde miyiz?
    private bool runHeld; // Shift koşu tuşu basılı mı?
    private int currentComboIndex;
    private float lastAttackTime;
    
    // Animasyon durum takibi (gereksiz güncellemeleri önlemek için)
    private bool lastIsWalking;
    private bool lastIsRunning;
    private bool lastIsGrounded;
    
    // Ölüm kontrolü
    private bool isDead = false;
    private HealthSystem healthSystem;
    
    void Start()
    {
        // Bileşenleri al
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthSystem = GetComponent<HealthSystem>();
        
        if (rb == null || anim == null)
        {
            Debug.LogError("Gerekli bileşenler (Rigidbody2D/Animator) eksik! Lütfen ekleyin.");
        }
        
        // HealthSystem'e event'leri bağla
        if (healthSystem != null)
        {
            healthSystem.OnDeath.AddListener(OnPlayerDeath);
            healthSystem.OnDamageTaken.AddListener(OnPlayerHurt); // Hasar alındığında Hurt animasyonu
        }
    }
    
    void OnPlayerDeath()
    {
        isDead = true;
        Debug.Log("💀 Player öldü! Hareket ve input devre dışı.");
        
        // Input'u tamamen durdur
        currentMoveInput = Vector2.zero;
        runHeld = false;
    }
    
    // Hasar alındığında çağrılır
    void OnPlayerHurt(float damage)
    {
        if (isDead) return; // Ölüyse hurt animasyonu oynatma
        
        // Hurt animasyonunu tetikle
        if (anim != null && !string.IsNullOrEmpty(hurtTriggerName))
        {
            anim.ResetTrigger(hurtTriggerName);
            anim.SetTrigger(hurtTriggerName);
            Debug.Log($"💥 Player hasar aldı! Hurt animasyonu tetiklendi. (Hasar: {damage})");
        }
    }

    // --- INPUT SYSTEM GERİ ÇAĞIRMALARI ---

    // Move Eylemi (WASD/Oklar)
    public void OnMove(InputValue value)
    {
        if (isDead) return; // Ölüyse input almasın
        currentMoveInput = value.Get<Vector2>();
    }

    // Run Eylemi (Shift tuşu)
    public void OnRun(InputValue value)
    {
        if (isDead) return; // Ölüyse input almasın
        runHeld = value.isPressed;
    }

    public void OnAttackPrimary(InputValue value)
    {
        if (isDead || !value.isPressed) return; // Ölüyse saldıramasın
        HandleComboAttack();
    }

    public void OnAirAttack(InputValue value)
    {
        if (isDead || !value.isPressed || isGrounded) return; // Ölüyse saldıramasın
        TriggerAttack(airAttackTriggerName);
    }

    // Jump Eylemi (W tuşu)
    public void OnJump(InputValue value)
    {
        if (isDead) return; // Ölüyse zıplayamasın
        
        if (isGrounded && value.isPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Dikey hızı sıfırla
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); 
            
            // Animator'daki Jump Trigger'ı tetikle (Zinciri başlatır)
            anim.SetTrigger("Jump"); 
        }
    }

    // --- OYUN DÖNGÜSÜ METOTLARI ---

    void Update()
    {
        if (isDead) return; // Ölüyse input almasın
        
        FlipCharacter();
    }

    void FixedUpdate()
    {
        if (isDead) return; // Ölüyse hareket etmesin
        
        // 1. Yer Kontrolü
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        
        // 2. Fiziksel Hareketi Uygula
        HandleMovement();

        // 3. Animasyon Parametrelerini Güncelle
        UpdateAnimationParameters();
    }

    // --- YARDIMCI METOTLAR ---

    private void HandleMovement()
    {
        // HAREKETİN ASIL GERÇEKLEŞTİĞİ YER
        float targetSpeed = runHeld ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(currentMoveInput.x * targetSpeed, rb.linearVelocity.y);
    }
    private void UpdateAnimationParameters()
    {
        // Havadayken koşma/yürüme animasyonlarını devre dışı bırak
        if (!isGrounded)
        {
            // Havadayken isRunning ve isWalking false olmalı
            if (lastIsRunning)
            {
                anim.SetBool("isRunning", false);
                lastIsRunning = false;
            }
            
            if (lastIsWalking)
            {
                anim.SetBool("isWalking", false);
                lastIsWalking = false;
            }
        }
        else
        {
            // Yerdeyken normal yürüme/koşma animasyonları
            bool isMoving = Mathf.Abs(currentMoveInput.x) > 0.01f;
            bool isRunning = isMoving && runHeld;
            bool isWalking = isMoving && !runHeld;
            
            // Sadece değer değiştiğinde güncelle (gereksiz animator güncellemelerini önle)
            if (isRunning != lastIsRunning)
            {
                anim.SetBool("isRunning", isRunning);
                lastIsRunning = isRunning;
            }
            
            if (isWalking != lastIsWalking)
            {
                anim.SetBool("isWalking", isWalking);
                lastIsWalking = isWalking;
            }
        }
        
        // Zıplama/Düşme Animasyonu parametreleri
        anim.SetFloat("yVelocity", rb.linearVelocity.y); // Yükselme/Düşme kontrolü için
        
        // isGrounded sadece değiştiğinde güncelle
        if (isGrounded != lastIsGrounded)
        {
            anim.SetBool("isGrounded", isGrounded);
            lastIsGrounded = isGrounded;
        }
    }

    private void TriggerAttack(string triggerName)
    {
        if (anim == null || string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        anim.ResetTrigger(triggerName);
        anim.SetTrigger(triggerName);
        
        // Saldırı hitbox'unu aktif et
        EnableAttackHitbox();
    }
    
    void EnableAttackHitbox()
    {
        // Attack hitbox'unu bul ve aktif et
        Transform attackHitbox = transform.Find("AttackHitbox");
        if (attackHitbox != null)
        {
            attackHitbox.gameObject.SetActive(true);
            Debug.Log("✅ Player AttackHitbox aktif edildi!");
            // Daha uzun süre aktif tut (combo saldırıları için)
            // 0.5 saniye sonra kapat (daha güvenilir vuruş için)
            Invoke("DisableAttackHitbox", 0.5f);
        }
        else
        {
            Debug.LogWarning("⚠️ Player AttackHitbox bulunamadı! Lütfen Player'ın child'ı olarak 'AttackHitbox' objesi oluşturun.");
        }
    }
    
    void DisableAttackHitbox()
    {
        Transform attackHitbox = transform.Find("AttackHitbox");
        if (attackHitbox != null)
        {
            attackHitbox.gameObject.SetActive(false);
        }
    }

    private void HandleComboAttack()
    {
        if (Time.time - lastAttackTime > comboResetTime)
        {
            currentComboIndex = 0;
        }

        string triggerToUse = GetComboTrigger(currentComboIndex);
        TriggerAttack(triggerToUse);

        currentComboIndex = (currentComboIndex + 1) % 3;
        lastAttackTime = Time.time;
    }

    private string GetComboTrigger(int index)
    {
        switch (index)
        {
            case 0:
                return attack1TriggerName;
            case 1:
                return attack2TriggerName;
            case 2:
            default:
                return attack3TriggerName;
        }
    }

    private void FlipCharacter()
    {
        if (currentMoveInput.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (currentMoveInput.x < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
    }

    private void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1; 
        transform.localScale = currentScale;
    }
    
    // YENİ: Hata Ayıklama (Debug) İçin Gizmos Metodu
    // GroundCheck objesi seçiliyken sahne görünümünde kırmızı daire çizer.
    private void OnDrawGizmosSelected()
    {
        // GroundCheck objesi atanmışsa çiz.
        if (groundCheck != null)
        {
            // Yere değme kontrol dairesini kırmızı renkte çiz.
            Gizmos.color = Color.red;
            
            // groundCheckRadius değişkeninin boyutunu gösterir.
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
    }
}
