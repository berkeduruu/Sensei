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
        // Yürüme/Koşma Animasyonu
        bool isMoving = Mathf.Abs(currentMoveInput.x) > 0.01f;
        bool isRunning = isMoving && runHeld;
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isWalking", isMoving && !runHeld);
        
        // Zıplama/Düşme Animasyonu parametreleri
        anim.SetFloat("yVelocity", rb.linearVelocity.y); // Yükselme/Düşme kontrolü için
        anim.SetBool("isGrounded", isGrounded); // Yerde olma kontrolü için
    }

    private void TriggerAttack(string triggerName)
    {
        if (anim == null || string.IsNullOrEmpty(triggerName))
        {
            return;
        }

        anim.ResetTrigger(triggerName);
        anim.SetTrigger(triggerName);
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