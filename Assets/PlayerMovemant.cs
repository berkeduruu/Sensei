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
    
    
    void Start()
    {
        // Bileşenleri al
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        
        if (rb == null || anim == null)
        {
            Debug.LogError("Gerekli bileşenler (Rigidbody2D/Animator) eksik! Lütfen ekleyin.");
        }
    }

    // --- INPUT SYSTEM GERİ ÇAĞIRMALARI ---

    // Move Eylemi (WASD/Oklar)
    public void OnMove(InputValue value)
    {
        currentMoveInput = value.Get<Vector2>();
    }

    // Run Eylemi (Shift tuşu)
    public void OnRun(InputValue value)
    {
        runHeld = value.isPressed;
    }

    public void OnAttackPrimary(InputValue value)
    {
        if (!value.isPressed) return;
        HandleComboAttack();
    }

    public void OnAirAttack(InputValue value)
    {
        if (!value.isPressed || isGrounded) return;
        TriggerAttack(airAttackTriggerName);
    }

    // Jump Eylemi (W tuşu)
    public void OnJump(InputValue value)
    {
        if (isGrounded)
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
        FlipCharacter();
    }

    void FixedUpdate()
    {
        // 1. Yer Kontrolü
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
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
            // 0.3 saniye sonra kapat (saldırı animasyonu süresine göre ayarla)
            Invoke("DisableAttackHitbox", 0.3f);
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